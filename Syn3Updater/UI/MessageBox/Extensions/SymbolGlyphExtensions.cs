namespace Syn3Updater.UI.MessageBox.Extensions {
    internal static class SymbolGlyphExtensions {
        public static string ToGlyph(this SymbolGlyph symbol) {
            return char.ConvertFromUtf32((int)symbol);
        }
    }
}
