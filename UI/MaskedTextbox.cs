using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Syn3Updater.UI
{
	public class MaskedTextBox : TextBox
	{
		public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
			"Mask",
			typeof(string),
			typeof(MaskedTextBox),
			new UIPropertyMetadata(OnMaskPropertyChanged));

		private MaskedTextProvider maskProvider;

		public string Mask
		{
			get { return (string)this.GetValue(MaskProperty); }
			set { this.SetValue(MaskProperty, value); }
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			var position = this.SelectionStart;
			var selectionLength = this.SelectionLength;

			switch (e.Key)
			{
				case Key.Back:
					if (selectionLength == 0)
					{
						this.RemoveChar(this.GetEditPositionTo(--position));
					}
					else
					{
						this.RemoveRange(position, selectionLength);
					}

					e.Handled = true;
					break;

				case Key.Delete:
					if (selectionLength == 0)
					{
						this.RemoveChar(this.GetEditPositionFrom(position));
					}
					else
					{
						this.RemoveRange(position, selectionLength);
					}

					e.Handled = true;
					break;

				case Key.Space:
					if (selectionLength != 0 && this.IsValidKey(e.Key, position))
					{
						this.RemoveRange(position, selectionLength);
					}
					else
					{
						this.UpdateText(" ", position);
					}

					e.Handled = true;
					break;

				default:
					if (selectionLength != 0 && this.IsValidKey(e.Key, position))
					{
						this.RemoveRange(position, selectionLength);
					}

					break;
			}
		}

		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			e.Handled = true;

			if (!this.IsReadOnly)
			{
				var position = this.SelectionStart;
				position = UpdateText(e.Text, position);
				base.OnPreviewTextInput(e);
			}
		}

		private static void OnMaskPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (MaskedTextBox)d;
			control.maskProvider = new MaskedTextProvider(control.Mask) { ResetOnSpace = false };
			control.maskProvider.Set(control.Text);
			control.RefreshText(control.SelectionStart);
		}

		private int GetEditPositionFrom(int startPosition)
		{
			var position = this.maskProvider.FindEditPositionFrom(startPosition, true);
			return position == -1 ? startPosition : position;
		}

		private int GetEditPositionTo(int endPosition)
		{
			while (endPosition >= 0 && !this.maskProvider.IsEditPosition(endPosition))
			{
				endPosition--;
			}

			return endPosition;
		}

		private bool IsValidKey(Key key, int position)
		{
			char virtualKey = (char)KeyInterop.VirtualKeyFromKey(key);
			MaskedTextResultHint resultHint;
			return this.maskProvider.VerifyChar(virtualKey, position, out resultHint);
		}

		private void RefreshText(int position)
		{
			if (!this.IsFocused)
			{
				this.Text = this.maskProvider.ToString(false, true);
			}
			else
			{
				this.Text = this.maskProvider.ToDisplayString();
			}

			this.SelectionStart = position;
		}

		private void RemoveRange(int position, int selectionLength)
		{
			if (this.maskProvider.RemoveAt(position, position + selectionLength - 1))
			{
				this.RefreshText(position);
			}
		}

		private void RemoveChar(int position)
		{
			if (this.maskProvider.RemoveAt(position))
			{
				this.RefreshText(position);
			}
		}

		private int UpdateText(string text, int position)
		{
			if (position < this.Text.Length)
			{
				position = this.GetEditPositionFrom(position);

				if ((Keyboard.IsKeyToggled(Key.Insert) && this.maskProvider.Replace(text, position)) ||
					this.maskProvider.InsertAt(text, position))
				{
					position++;
				}

				position = this.GetEditPositionFrom(position);
			}

			this.RefreshText(position);
			return position;
		}
	}
}