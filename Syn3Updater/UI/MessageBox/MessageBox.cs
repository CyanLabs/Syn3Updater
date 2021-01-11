﻿#nullable enable
using System.Linq;
using System.Windows;
using FontAwesome5;
using ModernWpf.Controls;
using Syn3Updater.UI.MessageBox.Extensions;
// ReSharper disable UnusedParameter.Local

namespace Syn3Updater.UI.MessageBox {
    public static class MessageBox {

        public static MessageBoxResult? Show(string messageBoxText) =>
            Show(null, messageBoxText, null, null, null, null);
        public static MessageBoxResult? Show(string messageBoxText, string? caption) =>
            Show(null, messageBoxText, caption, null, null, null);
        public static MessageBoxResult? Show(string messageBoxText, string? caption, MessageBoxButton button) =>
            Show(null, messageBoxText, caption, button, null, null);
        public static MessageBoxResult? Show(string messageBoxText, string? caption, MessageBoxButton button, Symbol symbol) =>
            Show(null, messageBoxText, caption, button, symbol, null);
        public static MessageBoxResult? Show(string messageBoxText, string? caption, MessageBoxButton button, MessageBoxImage image) =>
            Show(null, messageBoxText, caption, button, image, null);
        public static MessageBoxResult Show(string messageBoxText, string? caption, MessageBoxButton button, Symbol symbol, MessageBoxResult defaultResult) =>
            Show(null, messageBoxText, caption, button, symbol, defaultResult);
        public static MessageBoxResult? Show(string messageBoxText, string? caption, MessageBoxButton button, Symbol symbol, MessageBoxResult? defaultResult) =>
            Show(null, messageBoxText, caption, button, symbol, defaultResult);
        public static MessageBoxResult Show(string messageBoxText, string? caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult) =>
            Show(null, messageBoxText, caption, button, image, defaultResult);
        public static MessageBoxResult? Show(string messageBoxText, string? caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult? defaultResult) =>
            Show(null, messageBoxText, caption, button, image, defaultResult);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText) =>
            Show(owner, messageBoxText, null, null, null, null);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption) =>
            Show(owner, messageBoxText, caption, null, null, null);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button) =>
            Show(owner, messageBoxText, caption, button, null, null);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, Symbol symbol) =>
            Show(owner, messageBoxText, caption, button, symbol, null);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, MessageBoxImage image) =>
            Show(owner, messageBoxText, caption, button, image, null);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, EFontAwesomeIcon? glyph) =>
            Show(owner, messageBoxText, caption, button, glyph, null);
        public static MessageBoxResult Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, Symbol symbol, MessageBoxResult defaultResult) =>
            Show(owner, messageBoxText, caption, button, symbol, defaultResult);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, Symbol symbol, MessageBoxResult? defaultResult) =>
            Show(owner, messageBoxText, caption, button, symbol, defaultResult);
        public static MessageBoxResult Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, MessageBoxImage image, MessageBoxResult defaultResult) =>
            Show(owner, messageBoxText, caption, button, image.ToSymbol(), defaultResult);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, MessageBoxImage image, MessageBoxResult? defaultResult) =>
            Show(owner, messageBoxText, caption, button, image.ToSymbol(), defaultResult);
        public static MessageBoxResult Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, EFontAwesomeIcon? glyph, MessageBoxResult defaultResult) =>
            ShowInternal(owner, messageBoxText, caption, button, glyph, defaultResult);
        public static MessageBoxResult? Show(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, EFontAwesomeIcon? glyph, MessageBoxResult? defaultResult) =>
            ShowInternal(owner, messageBoxText, caption, button, glyph, defaultResult);

        // ReSharper disable once FunctionRecursiveOnAllPaths
        private static MessageBoxResult ShowInternal(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, EFontAwesomeIcon? glyph, MessageBoxResult defaultResult) =>
            ShowInternal(owner, messageBoxText, caption, button, glyph, defaultResult);
        private static MessageBoxResult? ShowInternal(Window? owner, string messageBoxText, string? caption, MessageBoxButton? button, EFontAwesomeIcon? glyph, MessageBoxResult? defaultResult) {
            var window = new MessageBoxWindow(messageBoxText, caption ?? string.Empty, button ?? MessageBoxButton.OK, glyph);
            window.Owner = owner ?? GetActiveWindow();
            window.ShowDialog();
            return window.Result ?? defaultResult;
        }

        private static Window GetActiveWindow() {
            return Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window.IsActive);
        }

    }
}