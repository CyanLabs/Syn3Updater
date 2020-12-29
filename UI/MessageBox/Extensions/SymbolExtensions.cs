using ModernWpf.Controls;

namespace ModernWpf.Extensions {
    internal static class SymbolExtensions {
        public static string ToGlyph(this Symbol symbol) {
            return char.ConvertFromUtf32((int)symbol);
        }
    }
}
