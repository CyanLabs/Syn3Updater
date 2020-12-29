using System;
using System.Windows;

namespace ModernWpf.Extensions {
    internal static class MessageBoxImageExtensions {
        public static SymbolGlyph ToSymbol(this MessageBoxImage image) {
            switch (image) {
                case MessageBoxImage.Error:
                    return SymbolGlyph.Error;
                case MessageBoxImage.Information:
                    return SymbolGlyph.Info;
                case MessageBoxImage.Warning:
                    return SymbolGlyph.Warning;
                case MessageBoxImage.Question:
                    return SymbolGlyph.StatusCircleQuestionMark;
                case MessageBoxImage.None:
                    return (SymbolGlyph)0x2007;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
