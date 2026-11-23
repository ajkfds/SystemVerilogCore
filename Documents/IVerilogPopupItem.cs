using System.Collections.Generic;
using System.Text;

namespace SystemVerilogCore.Documents
{
    /// <summary>
    /// The state a partial parse collects for one completion request: the word
    /// being typed and the candidates / hints found for it.
    ///
    /// <para>
    /// This used to be the editor's <c>CodeComplete.CompletionContext</c> base
    /// class, which the parser inherited. Its contents are entirely UI-free
    /// (two plain values and two lists of the contracts above), so the parser
    /// now owns the shape and the editor derives from it. That removes the last
    /// base-class dependency the parser had on the editor assembly.
    /// </para>
    ///
    /// <para>
    /// Hosts treat it as plain data: the editor turns each candidate into a
    /// popup row and each hint into a PopupItem, and the language server maps
    /// the candidates to LSP completion items.
    /// </para>
    /// </summary>
    public class VerilogCompletionContext
    {
        /// <summary>
        /// The identifier fragment before the caret that candidates are
        /// filtered by. Empty when the caret is not on a word.
        /// </summary>
        public string CandidateWord = "";

        /// <summary>
        /// Document offset where <see cref="CandidateWord"/> starts, i.e. the
        /// start of the range an accepted candidate replaces.
        /// </summary>
        public int CandidateStartIndex;

        /// <summary>
        /// Candidates for the current word (the autocomplete popup menu).
        /// </summary>
        public List<IVerilogAutocompleteItem> AutoCompleteItems = new();

        /// <summary>
        /// Caret-anchored hint entries for the current word (port / parameter
        /// labels shown while typing an argument).
        /// </summary>
        public List<IVerilogPopupItem> CarletPopupItems = new();
    }

    /// <summary>
    /// A caret-hint entry the parser produces (a port / parameter label shown
    /// while typing an argument).
    ///
    /// <para>
    /// The editor's <c>PopupItem</c> derives from <c>AjkAvaloniaLibs.ColorLabel</c>,
    /// i.e. it is an ordered list of individually coloured text / image spans.
    /// The parser built those spans directly, which tied every hint call site to
    /// the editor assembly. This contract carries the same information as plain
    /// data - text runs with a palette index - so a host can render it however it
    /// likes (the editor into a <c>ColorLabel</c>, the language server into a
    /// signature-help string).
    /// </para>
    /// </summary>
    public interface IVerilogPopupItem
    {
        /// <summary>
        /// The hint as plain text (runs concatenated, newlines preserved). This
        /// is what a host without rich rendering (signature help) shows.
        /// </summary>
        string CreateString();
    }

    /// <summary>One run of a <see cref="VerilogLabel"/>.</summary>
    public readonly struct VerilogLabelRun
    {
        public VerilogLabelRun(string text, VerilogColorType? colorType)
        {
            Text = text;
            ColorType = colorType;
        }

        /// <summary>The text of this run.</summary>
        public string Text { get; }

        /// <summary>
        /// Palette index for this run, or null to use the host's default
        /// foreground colour.
        /// </summary>
        public VerilogColorType? ColorType { get; }
    }

    /// <summary>
    /// UI-free equivalent of the editor's <c>ColorLabel</c>: an ordered list of
    /// coloured text runs that the parser can build and a host can render.
    ///
    /// <para>
    /// Runs are stored as (text, palette index) rather than concrete colours so
    /// this assembly stays free of any UI toolkit. The indices match the
    /// editor's palette slots (see <see cref="VerilogColorType"/>), so a host
    /// maps them straight back to its own colour table.
    /// </para>
    /// </summary>
    public sealed class VerilogLabel : IVerilogPopupItem
    {
        private readonly List<VerilogLabelRun> runs = new();

        /// <summary>The runs in display order.</summary>
        public IReadOnlyList<VerilogLabelRun> Runs => runs;

        /// <summary>Number of runs.</summary>
        public int RunCount => runs.Count;

        /// <summary>Appends text in the host's default foreground colour.</summary>
        public void AppendText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            runs.Add(new VerilogLabelRun(text, null));
        }

        /// <summary>Appends text in the given palette colour.</summary>
        public void AppendText(string text, VerilogColorType colorType)
        {
            if (string.IsNullOrEmpty(text)) return;
            runs.Add(new VerilogLabelRun(text, colorType));
        }

        /// <summary>Appends every run of <paramref name="label"/> in order.</summary>
        public void AppendLabel(VerilogLabel? label)
        {
            if (label == null) return;
            runs.AddRange(label.runs);
        }

        /// <summary>
        /// Appends an icon placeholder.
        ///
        /// <para>
        /// The editor's <c>ColorLabel</c> can embed a rendered <c>IImage</c>;
        /// this contract cannot (it is UI-free). The parser only uses icons as
        /// a leading decoration for a net's declaration label, and the text
        /// that follows already names the net kind ("wire" / "tri" / ...), so
        /// the icon is dropped rather than abstracted. Hosts that want it can
        /// add their own decoration from the run list.
        /// </para>
        /// </summary>
        public void AppendIconImage() { }

        /// <summary>
        /// The label as plain text. Runs are concatenated; a run that already
        /// ends with a line break keeps it (the editor's <c>ColorLabel</c>
        /// behaves the same way).
        /// </summary>
        public string CreateString()
        {
            StringBuilder sb = new();
            foreach (VerilogLabelRun run in runs)
            {
                sb.Append(run.Text);
            }
            return sb.ToString();
        }
    }
}
