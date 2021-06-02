using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using HTMLConverter;

namespace Cyanlabs.Syn3Updater.Converter
{
    public class HtmlToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (value != null)
            {
                FlowDocument flowDocument = new();
                string xaml = HtmlToXamlConverter.ConvertHtmlToXaml(value.ToString(), false);
                using (MemoryStream stream = new(new ASCIIEncoding().GetBytes(xaml)))
                {
                    TextRange text = new(flowDocument.ContentStart, flowDocument.ContentEnd);
                    text.Load(stream, DataFormats.Xaml);
                }

                return flowDocument;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}