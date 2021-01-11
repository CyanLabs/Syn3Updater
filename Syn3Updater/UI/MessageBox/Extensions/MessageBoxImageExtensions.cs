using System;
using System.Windows;
using FontAwesome5;

namespace Syn3Updater.UI.MessageBox.Extensions {
    internal static class MessageBoxImageExtensions {
        public static EFontAwesomeIcon ToSymbol(this MessageBoxImage image) {
            switch (image) {
                case MessageBoxImage.Error:
                    return EFontAwesomeIcon.Solid_TimesCircle;
                case MessageBoxImage.Information:
                    return EFontAwesomeIcon.Solid_InfoCircle;
                case MessageBoxImage.Warning:
                    return EFontAwesomeIcon.Solid_ExclamationCircle;
                case MessageBoxImage.Question:
                    return EFontAwesomeIcon.Solid_QuestionCircle;
                case MessageBoxImage.None:
                    return EFontAwesomeIcon.Solid_InfoCircle;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
