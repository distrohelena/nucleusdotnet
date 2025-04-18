﻿#if WINDOWS
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Platform.Windows.Controls {
    public class ImageButton : UserControl {
        public Button Button { get; private set; }

        [Browsable(true)]
        public Image Image { get; set; }

        [Browsable(true)]
        public override string Text {
            get { return Button.Text; }
            set { Button.Text = value; }
        }

        public ImageButton() {
            Button = new Button();
            Button.FlatStyle = FlatStyle.Flat;
            Button.TextAlign = ContentAlignment.BottomCenter;

            Button.FlatAppearance.BorderColor = Color.FromArgb(255, 200, 200, 200);
            Button.FlatAppearance.BorderSize = 1;
            Button.Click += Button_Click;

            Button.FlatAppearance.CheckedBackColor = Color.FromArgb(0, 0, 0, 0);
            Button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 0, 0, 0);
            Button.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 0, 0, 0);

            Button.Dock = DockStyle.Fill;
            Button.BackColor = Color.FromArgb(0, 0, 0, 0);

            this.Controls.Add(Button);
        }

        private void Button_Click(object sender, EventArgs e) {
            this.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs pevent) {
            base.OnPaint(pevent);

            Graphics g = pevent.Graphics;
            if (Image != null) {
                int borderSize = Button.FlatAppearance.BorderSize * 2;
                int borderSize2 = borderSize * 2;
                g.DrawImage(Image, new Rectangle(borderSize, borderSize, this.Width - borderSize2, this.Height - borderSize2), new Rectangle(Point.Empty, Image.Size), GraphicsUnit.Pixel);
            }
        }
    }
}
#endif