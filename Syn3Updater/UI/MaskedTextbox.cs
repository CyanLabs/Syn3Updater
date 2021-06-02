using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyanlabs.Syn3Updater.UI
{
    public class MaskedTextBox : TextBox
    {
        #region Properties & Fields

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask", typeof(string), typeof(MaskedTextBox), new UIPropertyMetadata(OnMaskPropertyChanged));

        private MaskedTextProvider _maskProvider;

        public string Mask
        {
            get => (string) GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            int position = SelectionStart;
            int selectionLength = SelectionLength;

            switch (e.Key)
            {
                case Key.Back:
                    if (selectionLength == 0)
                        RemoveChar(GetEditPositionTo(--position));
                    else
                        RemoveRange(position, selectionLength);

                    e.Handled = true;
                    break;

                case Key.Delete:
                    if (selectionLength == 0)
                        RemoveChar(GetEditPositionFrom(position));
                    else
                        RemoveRange(position, selectionLength);

                    e.Handled = true;
                    break;

                case Key.Space:
                    if (selectionLength != 0 && IsValidKey(e.Key, position))
                        RemoveRange(position, selectionLength);
                    else
                        UpdateText(" ", position);

                    e.Handled = true;
                    break;

                default:
                    if (selectionLength != 0 && IsValidKey(e.Key, position)) RemoveRange(position, selectionLength);

                    break;
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = true;

            if (!IsReadOnly)
            {
                int position = SelectionStart;
                UpdateText(e.Text, position);
                base.OnPreviewTextInput(e);
            }
        }

        private static void OnMaskPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MaskedTextBox control = (MaskedTextBox) d;
            control._maskProvider = new MaskedTextProvider(control.Mask) {ResetOnSpace = false};
            control._maskProvider.Set(control.Text);
            control.RefreshText(control.SelectionStart);
        }

        private int GetEditPositionFrom(int startPosition)
        {
            int position = _maskProvider.FindEditPositionFrom(startPosition, true);
            return position == -1 ? startPosition : position;
        }

        private int GetEditPositionTo(int endPosition)
        {
            while (endPosition >= 0 && !_maskProvider.IsEditPosition(endPosition)) endPosition--;

            return endPosition;
        }

        private bool IsValidKey(Key key, int position)
        {
            char virtualKey = (char) KeyInterop.VirtualKeyFromKey(key);
            return _maskProvider.VerifyChar(virtualKey, position, out MaskedTextResultHint _);
        }

        private void RefreshText(int position)
        {
            Text = !IsFocused ? _maskProvider.ToString(false, true) : _maskProvider.ToDisplayString();

            SelectionStart = position;
        }

        private void RemoveRange(int position, int selectionLength)
        {
            if (_maskProvider.RemoveAt(position, position + selectionLength - 1)) RefreshText(position);
        }

        private void RemoveChar(int position)
        {
            if (_maskProvider.RemoveAt(position)) RefreshText(position);
        }

        private void UpdateText(string text, int position)
        {
            if (position < Text.Length)
            {
                position = GetEditPositionFrom(position);

                if (Keyboard.IsKeyToggled(Key.Insert) && _maskProvider.Replace(text, position) || _maskProvider.InsertAt(text, position))
                    position++;

                position = GetEditPositionFrom(position);
            }

            RefreshText(position);
        }

        #endregion
    }
}