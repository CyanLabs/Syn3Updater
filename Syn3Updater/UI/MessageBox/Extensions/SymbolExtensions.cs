using ModernWpf.Controls;

namespace Syn3Updater.UI.MessageBox.Extensions {
    internal static class SymbolExtensions {
        public static string ToGlyph(this Symbol symbol) {
            return char.ConvertFromUtf32((int)symbol);
        }
    }
}
