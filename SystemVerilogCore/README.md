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
| `SystemVerilogCore`                       | Top-level `ISystemVerilogCore` and `ISystemVerilogProject` entry points. |
| `SystemVerilogCore.Documents`             | `ISystemVerilogFile`, `ISystemVerilogDocument`, `ISystemVerilogBuildingBlock`, `ISystemVerilogNamedElement`, `ISystemVerilogCodeDocument`, and the `SystemVerilogRange` / `SystemVerilogPosition` value types. |
| `SystemVerilogCore.Diagnostics`           | `ISystemVerilogDiagnostic` and the `SystemVerilogSeverity` enum. |

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

## First-cut status

The following methods are wired to real data:

* `ISystemVerilogProject.Files` – enumerates Verilog/SystemVerilog
  files in the project.
* `ISystemVerilogProject.FindFile` – locates a file by id.
* `ISystemVerilogProject.GetDocumentAsync` – returns the cached
  `ParsedDocument` (with diagnostics) if one is available.
* `ISystemVerilogDocument.Diagnostics` – the parser's accumulated
  `Messages`, mapped to `ISystemVerilogDiagnostic`.

The following are stubbed and will be filled in once the parser-backed
adapter is built (next phase):

* `ISystemVerilogProject.FindDefinitionAsync`
* `ISystemVerilogProject.FindReferencesAsync`
* `ISystemVerilogDocument.FindElementAt`
* `ISystemVerilogBuildingBlock.Members` /
  `ISystemVerilogBuildingBlock.BuildingBlocks`
