using System;
using System.Drawing;

namespace Nucleus.ConsoleEngine {
    public class ConsoleMenuTextBox : ConsoleMenuOption {
        public bool IsPassword { get; set; }
        public string Input { get; set; } = "";

        // Simple cursor timer so focused inputs blink at a steady cadence.
        private double timer;
        private bool tick;

        public ConsoleColor? InputColor { get; set; }
        public ConsoleColor? InputBgColor { get; set; }

        public ConsoleMenuTextBox(
            Point location,
            string text,
            Action callback,
            ConsoleColor? color = null,
            ConsoleColor? bgColor = null
        ) : base(location, text, callback, color, bgColor) {

        }

        public override void Render(ConsoleMenu menu, bool selected, double elapsed) {
            timer += elapsed;
            if (timer > 300) {
                timer = 0;
                tick = !tick;
            }

            Rectangle bounds = GetAnchoredBounds(menu);
            bool hasBorder = BorderColor != null;

            int offsetX = hasBorder ? 1 : 0;
            int offsetY = hasBorder ? 1 : 0;
            int interiorWidth = Math.Max(1, bounds.Width - offsetX - (hasBorder ? 1 : 0));

            if (selected) {
                menu.Graphics.FillRectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    bounds.Height,
                    ' ',
                    menu.SelectedColor,
                    menu.SelectedBgColor
                );
            } else if (BackgroundColor != null) {
                menu.Graphics.FillRectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    bounds.Height,
                    ' ',
                    null,
                    BackgroundColor
                );
            }

            if (hasBorder) {
                menu.Graphics.DrawRectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width,
                    bounds.Height,
                    BorderColor.Value,
                    selected ? menu.SelectedBgColor : BackgroundColor
                );
            }

            string inputLine = BuildInputLine(menu, interiorWidth, selected && tick);
            menu.Graphics.DrawString(
                inputLine,
                bounds.X + offsetX,
                bounds.Y + offsetY,
                selected ? menu.SelectedColor : InputColor,
                selected ? menu.SelectedBgColor : InputBgColor
            );

            if (bounds.Height - offsetY > 1) {
                string label = BuildDisplayText(menu, interiorWidth);
                menu.Graphics.DrawString(
                    label,
                    bounds.X + offsetX,
                    bounds.Y + offsetY + 1,
                    selected ? menu.SelectedColor : Color,
                    selected ? menu.SelectedBgColor : BackgroundColor
                );
            }
        }

        public override void ReceiveKey(ConsoleKey key) {
            base.ReceiveKey(key);

            if (key == ConsoleKey.Backspace && Input.Length > 0) {
                Input = Input.Remove(Input.Length - 1);
                return;
            } else if (key == ConsoleKey.Spacebar) {
                Input += " ";
            }

            // Convert single character keys to lowercase for simplicity; ignore modifiers.
            string value = key.ToString().ToLowerInvariant();
            if (value.StartsWith("d") && value.Length == 2) {
                value = value.Substring(1);
            } else if (value.Length > 1) {
                return;
            }
            Input += value;
        }

        private string BuildInputLine(ConsoleMenu menu, int availableWidth, bool appendCursor) {
            if (availableWidth <= 0) {
                return string.Empty;
            }

            string rendered = Input ?? string.Empty;
            if (IsPassword) {
                rendered = StringUtil.Repeat("*", rendered.Length);
            }

            if (menu.Offset) {
                rendered = " " + rendered;
            }

            // Append the blinking cursor without exceeding the visual bounds.
            if (appendCursor) {
                if (rendered.Length >= availableWidth) {
                    int cursorWidth = Math.Max(availableWidth - 1, 0);
                    rendered = cursorWidth > 0 ? rendered.Substring(0, cursorWidth) : string.Empty;
                    if (availableWidth > 0) {
                        rendered += "_";
                    }
                } else {
                    rendered += "_";
                }
            }

            if (rendered.Length > availableWidth) {
                rendered = rendered.Substring(0, availableWidth);
            } else if (menu.FillText) {
                int pad = availableWidth - rendered.Length;
                if (pad > 0) {
                    //rendered += StringUtil.Repeat(" ", pad);
                }
            }

            return rendered;
        }
    }
}
