using System.Text;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// Produces a human-readable, markdown-style description of a
    /// <see cref="ISystemVerilogNamedElement"/> suitable for an LSP
    /// <c>textDocument/hover</c> response. The Core assembly only knows
    /// about the read-only interface, so the description is intentionally
    /// generic; consumers (or the plugin adapter layer) can register a
    /// custom <see cref="IHoverContentProvider"/> through
    /// <see cref="RegisterProvider"/> to enrich the default output.
    /// </summary>
    public static class HoverContent
    {
        private static IHoverContentProvider? _customProvider;

        /// <summary>
        /// Installs a process-wide custom provider. Pass <c>null</c> to
        /// revert to the built-in description. The setter is intentionally
        /// not thread-safe; install the provider during host startup.
        /// </summary>
        public static void RegisterProvider(IHoverContentProvider? provider)
        {
            _customProvider = provider;
        }

        /// <summary>
        /// Builds a short description. Returns <c>null</c> if the element
        /// does not have a name.
        /// </summary>
        public static string? Build(ISystemVerilogNamedElement? element) =>
            Build(element, null);

        /// <summary>
        /// Builds a short description, using <paramref name="signatureBlock"/>
        /// in place of the generated signature code block.
        ///
        /// <para>
        /// A host that can render the parser's own label (the language server,
        /// through a TextMate grammar) passes it here so the hover shows the
        /// same coloured declaration the editor's mouse-over hint paints. The
        /// definition path and the overall layout stay in this one place, so
        /// both hosts keep the same shape.
        /// </para>
        /// </summary>
        /// <param name="signatureBlock">
        /// Pre-rendered markdown block to show instead of the plain
        /// <c>systemverilog</c> fence, or null to generate the signature.
        /// </param>
        public static string? Build(ISystemVerilogNamedElement? element, string? signatureBlock)
        {
            if (element == null) return null;
            if (string.IsNullOrEmpty(element.Name)) return null;

            // Give the custom provider a chance to add plugin-side detail
            // (DataType, BitWidth, port direction, ...). If it returns a
            // non-null string we use it verbatim, otherwise we fall back to
            // the default signature.
            if (_customProvider != null)
            {
                string? custom = _customProvider.Build(element);
                if (!string.IsNullOrEmpty(custom)) return custom;
            }

            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(signatureBlock))
            {
                sb.Append("```systemverilog\n");
                sb.Append(FormatSignature(element));
                sb.Append("\n```");
            }
            else
            {
                sb.Append(signatureBlock);
            }

            if (element.File != null)
            {
                string? path = element.File.AbsolutePath ?? element.File.Id;
                if (!string.IsNullOrEmpty(path))
                {
                    sb.Append("\n\n_Defined in: `");
                    sb.Append(path);
                    sb.Append("`_");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// The one-line signature of an element (e.g. <c>module foo</c>).
        ///
        /// <para>
        /// Exposed so a host that renders its own fence can reuse the exact
        /// wording, keeping the two hosts' hovers identical.
        /// </para>
        /// </summary>
        public static string BuildSignature(ISystemVerilogNamedElement element) =>
            FormatSignature(element);

        private static string FormatSignature(ISystemVerilogNamedElement element)
        {
            return element.Kind switch
            {
                SystemVerilogNamedElementKind.Module => $"module {element.Name}",
                SystemVerilogNamedElementKind.Interface => $"interface {element.Name}",
                SystemVerilogNamedElementKind.Package => $"package {element.Name}",
                SystemVerilogNamedElementKind.Program => $"program {element.Name}",
                SystemVerilogNamedElementKind.Checker => $"checker {element.Name}",
                SystemVerilogNamedElementKind.Primitive => $"primitive {element.Name}",
                SystemVerilogNamedElementKind.Class => $"class {element.Name}",
                SystemVerilogNamedElementKind.Variable => $"{element.Name}",
                SystemVerilogNamedElementKind.Net => $"{element.Name}",
                SystemVerilogNamedElementKind.Parameter => $"parameter {element.Name}",
                SystemVerilogNamedElementKind.LocalParameter => $"localparam {element.Name}",
                SystemVerilogNamedElementKind.Port => $"port {element.Name}",
                SystemVerilogNamedElementKind.Typedef => $"typedef {element.Name}",
                SystemVerilogNamedElementKind.Function => $"function {element.Name}",
                SystemVerilogNamedElementKind.Task => $"task {element.Name}",
                SystemVerilogNamedElementKind.Modport => $"modport {element.Name}",
                SystemVerilogNamedElementKind.Instance => $"{element.Name} (instance)",
                SystemVerilogNamedElementKind.PackageItem => $"{element.Name}",
                SystemVerilogNamedElementKind.Macro => $"`{element.Name}`",
                SystemVerilogNamedElementKind.GenerateBlock => $"generate {element.Name}",
                _ => element.Name,
            };
        }
    }
}
