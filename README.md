# SystemVerilogCore

UI-agnostic interface definitions for the SystemVerilog language tooling.

See [`SystemVerilogCore/README.md`](SystemVerilogCore/README.md) for the
detailed design, the seam layout, the wired status table, and the
hover / diagnostic code documentation.

## Building

This repository is a git submodule of the main `RtlEditor2` checkout
and is built as part of the overall solution. The actual project file
lives under the inner `SystemVerilogCore/` directory:

```
SystemVerilogCore/SystemVerilogCore/SystemVerilogCore.csproj
```

> ⚠️ Do **not** add another `SystemVerilogCore.csproj` directly under
> `SystemVerilogCore/` (the outer directory). The SDK would pick it up
> alongside the inner project and the `Class1.cs` placeholder would
> cause duplicate `TargetFrameworkAttribute` /
> `AssemblyTitleAttribute` build errors (MSB0579).

Build the inner project in isolation:

```
dotnet build SystemVerilogCore/SystemVerilogCore/SystemVerilogCore.csproj -clp:ErrorsOnly
```

Or build the whole solution from the main repository:

```
dotnet build RtlEditor2.Desktop.csproj -clp:ErrorsOnly
```
