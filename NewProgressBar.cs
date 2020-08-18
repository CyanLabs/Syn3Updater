using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sync3Updater.Controls
{
    public class NewProgressBar : ProgressBar
    {
        public NewProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs prevent)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            const int inset = 2; // A single inset value to control teh sizing of the inner rect.

            using (Image offscreenImage = new Bitmap(Width, Height))
            {
                using (Graphics offscreen = Graphics.FromImage(offscreenImage))
                {
                    Rectangle rect = new Rectangle(0, 0, Width, Height);
                    if (ProgressBarRenderer.IsSupported)
                        ProgressBarRenderer.DrawHorizontalBar(offscreen, rect);

                    rect.Inflate(new Size(-inset, -inset)); // Deflate inner rect.
                    rect.Width = (int)(rect.Width * ((Value - (double)Minimum) / (Maximum - (double)Minimum)));
                    if (rect.Width == 0) rect.Width = 1; // Can't draw rec with width of 0.

                    LinearGradientBrush brush = new LinearGradientBrush(rect, BackColor, ForeColor, LinearGradientMode.Vertical);
                    offscreen.FillRectangle(brush, inset, inset, rect.Width, rect.Height);

                    e.Graphics.DrawImage(offscreenImage, 0, 0);
                }
            }
        }
    }

    public partial class TabControlWithoutHeader : TabControl
    {
        public TabControlWithoutHeader()
        {
            this.Multiline = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328)
                m.Result = new IntPtr(1);
            else
                base.WndProc(ref m);
        }
    }

}