using System.Drawing;

namespace Nucleus.ConsoleEngine {
    public class ConsoleMenuLabel : ConsoleControl {
        public ConsoleMenuLabel(Point location, string text, ConsoleColor? color = null, ConsoleColor? bgColor = null)
            : base(location) {
            Text = text;
            Color = color;
            BackgroundColor = bgColor;
        }

        public ConsoleMenuLabel(Rectangle bounds, string text, ConsoleColor? color = null, ConsoleColor? bgColor = null)
            : base(bounds) {
            Text = text;
            Color = color;
            BackgroundColor = bgColor;
        }

        public string Text { get; set; }
        public ConsoleColor? Color { get; set; }
        public ConsoleColor? BackgroundColor { get; set; }

        public override void Render(ConsoleMenu menu, bool selected, double elapsed) {
            Rectangle bounds = GetAnchoredBounds(menu);

            if (BackgroundColor != null) {
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

            int availableWidth = Math.Max(0, bounds.Width);
            int availableHeight = Math.Max(0, bounds.Height);
            if (availableWidth == 0 || availableHeight == 0) {
                return;
            }

            string raw = Text ?? string.Empty;
            string[] lines = raw
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.None);

            int maxLines = Math.Min(availableHeight, lines.Length);
            for (int i = 0; i < maxLines; i++) {
                string line = lines[i] ?? string.Empty;
                if (menu.Offset) {
                    line = " " + line;
                }

                if (line.Length > availableWidth) {
                    line = line.Substring(0, availableWidth);
                }

                if (!string.IsNullOrEmpty(line)) {
                    menu.Graphics.DrawString(
                        line,
                        bounds.X,
                        bounds.Y + i,
                        Color,
                        BackgroundColor
                    );
                }
            }
        }
    }
}
