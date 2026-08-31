# SystemVerilogCore

UI-agnostic interface definitions for the SystemVerilog language tooling.

This assembly is the *seam* between the parsing/symbol-resolution
implementation (currently the `CodeEditor2VerilogPlugin` plugin) and the
LSP server (and any other tool that wants to consume SystemVerilog
semantics without taking a dependency on Avalonia or the rest of the
editor).

## Layout

| Namespace | Purpose |
|-----------|---------|
| `SystemVerilogCore`             | Top-level `ISystemVerilogCore` and `ISystemVerilogProject` entry points. |
| `SystemVerilogCore.Documents`   | `ISystemVerilogFile`, `ISystemVerilogDocument`, `ISystemVerilogBuildingBlock`, `ISystemVerilogNamedElement`, `ISystemVerilogCodeDocument`, `HoverContent`, `IHoverContentProvider`, and the `SystemVerilogRange` / `SystemVerilogPosition` value types. |
| `SystemVerilogCore.Diagnostics` | `ISystemVerilogDiagnostic` and the `SystemVerilogSeverity` enum. |

The interfaces deliberately do **not** expose:

- `Avalonia.*` or any other UI type
- `AvaloniaEdit.*` (the editor's `TextDocument` is hidden behind
  `ISystemVerilogCodeDocument`)
- The plugin's `pluginVerilog.*` types
- JSON serialisation attributes

## How the seam is wired today

```
                          SystemVerilogLanguageServer
                                   │
                                   │  depends only on
                                   ▼
                            SystemVerilogCore  (this assembly)
                                   ▲
                                   │  implemented by
                                   │
                       CodeEditor2VerilogPlugin
                       (CoreBridge/...Adapter.cs)
```

* `CodeEditor2VerilogPlugin` ships adapters under
  `pluginVerilog.CoreBridge` that wrap the plugin's existing
  `VerilogFile`, `CodeDocument`, `ParsedDocument`, and `Project` types
  in the SystemVerilogCore interfaces.
* `SystemVerilogLanguageServer` consumes the SystemVerilogCore
  interfaces only. It can therefore be hosted in any process, including
  one without Avalonia.

## Status

The SystemVerilogCore seam is fully wired on the plugin side. The
following methods produce real data when a parser-backed project is
passed to the language server:

| Surface                       | Status | Notes |
|-------------------------------|--------|-------|
| `ISystemVerilogProject.Files`                | ✅ | enumerates Verilog/SystemVerilog files in the project |
| `ISystemVerilogProject.FindFile`            | ✅ | locates a file by id |
| `ISystemVerilogProject.GetDocumentAsync`    | ✅ | returns the cached `ParsedDocument` (with diagnostics) |
| `ISystemVerilogProject.FindDefinitionAsync` | ✅ | local resolution + cross-file via `ProjectProperty.DefinitionNameSpace` / `PackageNameSpace` |
| `ISystemVerilogProject.FindReferencesAsync` | ✅ | `DataObject` declarations return the full `UsedReferences` / `AssignedReferences` list; cross-file declarations return the declaration only |
| `ISystemVerilogDocument.Diagnostics`        | ✅ | `Code` field populated by `DiagnosticCodeMap` (e.g. `verilog/undriven`) |
| `ISystemVerilogDocument.FindElementAt`      | ✅ | walks the namespace tree to return the deepest element that contains the index |
| `ISystemVerilogDocument.Root`               | ✅ | built from `ParsedDocument.Root` |
| `ISystemVerilogBuildingBlock.Members`       | ✅ | adapted from `NameSpace.NamedElements` |
| `ISystemVerilogBuildingBlock.BuildingBlocks`| ✅ | adapted from `BuildingBlock.BuildingBlocks` |
| `HoverContent.Build` / `IHoverContentProvider` | ✅ | markdown signatures + custom plugin extension point (data type, bit width, port direction, port list) |

## Extending hover

Hosts that have richer information than the default `HoverContent`
provider (for example, a plugin that knows an element's data type) can
install a custom `IHoverContentProvider` once at startup:

```csharp
SystemVerilogCore.Documents.HoverContent.RegisterProvider(
    new MyPluginHoverProvider());
```

The plugin-side adapter (`pluginVerilog.CoreBridge.PluginHoverContentProvider`)
already does this from `Plugin.Register()` and adds data type, bit
width, port direction and a port list to the markdown block.

## Diagnostic codes

`SystemVerilogCore.Documents.SystemVerilogRange` is the source of truth
for source ranges. The diagnostic `Code` field is a free-form string;
the plugin-side `DiagnosticCodeMap` produces stable, slugified values
such as:

| Text contains          | Code                              |
|------------------------|-----------------------------------|
| `undriven`             | `verilog/undriven`                |
| `unused`               | `verilog/unused`                  |
| `not defined here`     | `verilog/undefined`               |
| `duplicate`            | `verilog/duplicate`               |
| `implicit net`         | `verilog/implicit-net-declaration`|
| `bitwidth`             | `verilog/assignment-bitwidth-mismatch` |
| (other)                | `verilog/<slug-of-text>`          |

Clients can use these codes for rule suppressions, filter UI, or
diagnostic grouping.
