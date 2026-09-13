using System;
using System.Drawing;

namespace Nucleus.ConsoleEngine {
    [Flags]
    public enum ConsoleAnchorStyles {
        None = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
    }

    public class ConsoleMenuOption : ConsoleControl {
        public string Text { get; set; }

        public ConsoleColor? Color { get; set; }
        public ConsoleColor? BackgroundColor { get; set; }
        public ConsoleColor? BorderColor { get; set; }

        public override bool IsSelectable => true;

        public Action Callback { get; set; }

        public ConsoleMenuOption(
            Point location,
            string text,
            Action callback,
            ConsoleColor? color = null,
            ConsoleColor? bgColor = null
        ) : base(location) {
            Dimensions = new Size(30, 1);
            Text = text;
            Color = color;
            BackgroundColor = bgColor;
            Callback = callback;
            BorderColor = ConsoleColor.White;
            Anchor = ConsoleAnchorStyles.Top | ConsoleAnchorStyles.Left;
        }

        public ConsoleMenuOption(
            Rectangle bounds,
            string text,
            Action callback,
            ConsoleColor? color = null,
            ConsoleColor? bgColor = null
        ) : this(bounds.Location, text, callback, color, bgColor) {
            Dimensions = bounds.Size;
        }

        public override void Render(ConsoleMenu menu, bool selected, double elapsed) {
            Rectangle bounds = GetAnchoredBounds(menu);

            int offsetX = 0;
            int offsetY = 0;
            bool hasBorder = BorderColor != null;

            if (selected) {
                // Draw the highlight background first so the option text can sit on top.
                menu.Graphics.FillRectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    bounds.Height,
                    ' ',
                    menu.SelectedColor,
                    menu.SelectedBgColor
                );

                if (hasBorder) {
                    menu.Graphics.DrawRectangle(
                        bounds.X,
                        bounds.Y,
                        bounds.Width,
                        bounds.Height,
                        BorderColor.Value,
                        menu.SelectedBgColor
                    );
                    offsetX = 1;
                    offsetY = 1;
                }

                string text = BuildDisplayText(menu, bounds.Width - offsetX - (hasBorder ? 1 : 0));
                menu.Graphics.DrawString(
                    text,
                    bounds.X + offsetX,
                    bounds.Y + offsetY,
                    menu.SelectedColor,
                    menu.SelectedBgColor
                );
            } else {
                if (hasBorder) {
                    menu.Graphics.DrawRectangle(
                        bounds.X,
                        bounds.Y,
                        bounds.Width,
                        bounds.Height,
                        BorderColor.Value,
                        BackgroundColor
                    );
                    offsetX = 1;
                    offsetY = 1;
                }

                string text = BuildDisplayText(menu, bounds.Width - offsetX - (hasBorder ? 1 : 0));
                menu.Graphics.DrawString(
                    text,
                    bounds.X + offsetX,
                    bounds.Y + offsetY,
                    Color,
                    BackgroundColor
                );
            }
        }

        public override void ReceiveKey(ConsoleKey key) {
        }

        /// <summary>
        /// Gives interactive controls a chance to consume navigation keys before the menu moves focus.
        /// </summary>
        public virtual bool HandleNavigationKey(ConsoleKey key) {
            return false;
        }

        protected string BuildDisplayText(ConsoleMenu menu, int availableWidth) {
            if (availableWidth <= 0) {
                return string.Empty;
            }

            string text = Text ?? string.Empty;

            if (menu.Offset) {
                text = " " + text;
            }

            // Clip or pad to the requested width so borders align cleanly for every option.
            if (text.Length > availableWidth) {
                text = text.Substring(0, availableWidth);
            } else if (menu.FillText) {
                int remaining = availableWidth - text.Length;
                //if (remaining > 0) {
                //    text += StringUtil.Repeat(" ", remaining);
                //}
            }

            return text;
        }
    }
}
