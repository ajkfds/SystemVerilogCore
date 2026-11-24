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
                                   │  depends on
                                   ▼
                            SystemVerilogCore  (this assembly)
                                   ▲
                                   │  implemented by
                                   │
                       SystemVerilogLanguageServer.Server
                            InMemorySystemVerilogCore
                                   │
                                   │  drives the real parser via
                                   ▼
                       CodeEditor2VerilogPlugin
                         (CoreBridge/ParseEngine)
```

* `SystemVerilogLanguageServer` is the **only** consumer of this
  assembly. It implements the interfaces in
  `Server/InMemorySystemVerilogCore.cs` and drives the real plugin parser
  through `pluginVerilog.CoreBridge.ParseEngine`, which is UI-free.
* `CodeEditor2VerilogPlugin` does **not** reference this assembly. It
  exposes parser-facing, UI-free helpers (`ParseEngine`,
  `CompletionAdapter`, `SnippetAdapter`, `AutoConnectAdapter`, ...) that
  return its own DTOs. The desktop editor and the language server both
  call those helpers, but neither the plugin nor the desktop editor
  depends on the LSP contract types.
* This assembly has **no package references**: it must stay free of
  Avalonia (and any other UI stack) so the server can be hosted in a
  plain process.

> **History.** The plugin used to carry an adapter stack
> (`VerilogSystemVerilogCore`, `SystemVerilogProjectAdapter`,
> `SystemVerilogFileAdapter`, `SystemVerilogDocumentAdapter`,
> `BuildingBlockAdapter`, `NamedElementAdapter`, `SymbolResolver`,
> `PluginHoverContentProvider`) that implemented these interfaces for the
> desktop side. It was unreachable: the only entry point wrapped a
> `Project` and nothing ever read the result, and the language server
> never used it (it builds its own `InMemory*` model from `ParseEngine`).
> That stack has been deleted so there is exactly one implementation of
> this seam. `HoverContent` / `IHoverContentProvider` remain as an
> extension point for the server.

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

The default `HoverContent` output is already a complete markdown block
(signature + defining file). Hosts that know more about an element can
install a provider once at startup; `SystemVerilogLanguageServer` does
not currently install one, so it serves the default text.

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
