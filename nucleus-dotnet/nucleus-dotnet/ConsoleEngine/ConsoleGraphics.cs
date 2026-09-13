using System;
using System.Drawing;
using System.Text;

namespace Nucleus.ConsoleEngine {
    public class ConsoleGraphics {
        private struct ConsolePixel {
            public char Character;
            public ConsoleColor? ForegroundColor;
            public ConsoleColor? BackgroundColor;

            public ConsolePixel(char character, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
                Character = character;
                ForegroundColor = foregroundColor;
                BackgroundColor = backgroundColor;
            }
        }

        private ConsolePixel[,] canvas;
        private Size size;
        private Point offset;
        private readonly bool useAnsiSequences;
        private readonly ConsoleColor defaultForeground;
        private readonly ConsoleColor defaultBackground;
        private bool cursorPositioningFailed;
        /// <summary>
        /// Tracks whether Unicode box drawing characters should be used for borders.
        /// </summary>
        readonly bool useUnicodeBorders;

        /// <summary>
        /// Horizontal border character for rectangles.
        /// </summary>
        readonly char borderHorizontal;

        /// <summary>
        /// Vertical border character for rectangles.
        /// </summary>
        readonly char borderVertical;

        /// <summary>
        /// Top-left corner border character for rectangles.
        /// </summary>
        readonly char borderTopLeft;

        /// <summary>
        /// Top-right corner border character for rectangles.
        /// </summary>
        readonly char borderTopRight;

        /// <summary>
        /// Bottom-left corner border character for rectangles.
        /// </summary>
        readonly char borderBottomLeft;

        /// <summary>
        /// Bottom-right corner border character for rectangles.
        /// </summary>
        readonly char borderBottomRight;

        public ConsoleColor? BackgroundColor { get; set; }

        public int Width => size.Width;
        public int Height => size.Height;
        public Size Size => size;

        public ConsoleGraphics(int width, int height, bool enableAnsiSequences = true) {
            Console.OutputEncoding = Encoding.UTF8;
            useAnsiSequences = enableAnsiSequences;
            defaultForeground = SafeGetColor(() => Console.ForegroundColor, ConsoleColor.Gray);
            defaultBackground = SafeGetColor(() => Console.BackgroundColor, ConsoleColor.Black);
            useUnicodeBorders = ResolveUnicodeBorderPreference(enableAnsiSequences);
            borderHorizontal = useUnicodeBorders ? '─' : '-';
            borderVertical = useUnicodeBorders ? '│' : '|';
            borderTopLeft = useUnicodeBorders ? '┌' : '+';
            borderTopRight = useUnicodeBorders ? '┐' : '+';
            borderBottomLeft = useUnicodeBorders ? '└' : '+';
            borderBottomRight = useUnicodeBorders ? '┘' : '+';
            Resize(width, height);
        }

        public void Resize(int width, int height) {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            if (canvas != null && canvas.GetLength(0) == width && canvas.GetLength(1) == height) {
                return;
            }

            canvas = new ConsolePixel[width, height];
            size = new Size(width, height);
        }

        public void SetOffset(int x, int y) {
            offset = new Point(x, y);
        }

        public void DrawString(string text, int startX, int startY, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
            if (string.IsNullOrEmpty(text) || size.Width == 0 || size.Height == 0) {
                return;
            }

            int targetY = startY + offset.Y;
            if (targetY < 0 || targetY >= size.Height) {
                return;
            }

            int targetX = startX + offset.X;
            int textIndex = 0;

            if (targetX < 0) {
                textIndex = -targetX;
                if (textIndex >= text.Length) {
                    return;
                }
                targetX = 0;
            }

            for (int x = targetX; x < size.Width && textIndex < text.Length; x++) {
                canvas[x, targetY] = new ConsolePixel(text[textIndex++], foregroundColor, backgroundColor);
            }
        }

        public void DrawStringPad(string text, int startX, int startY, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
            if (string.IsNullOrEmpty(text) || size.Width == 0 || size.Height == 0) {
                return;
            }

            int targetY = startY + offset.Y;
            if (targetY < 0 || targetY >= size.Height) {
                return;
            }

            int targetX = startX + offset.X - text.Length;
            int textIndex = 0;

            if (targetX < 0) {
                textIndex = -targetX;
                if (textIndex >= text.Length) {
                    return;
                }
                targetX = 0;
            }

            for (int x = targetX; x < size.Width && textIndex < text.Length; x++) {
                canvas[x, targetY] = new ConsolePixel(text[textIndex++], foregroundColor, backgroundColor);
            }
        }

        public void DrawHorizontalLine(char c, int x, int y, int width, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
            if (width <= 0 || size.Width == 0 || size.Height == 0) {
                return;
            }

            int targetY = y + offset.Y;
            if (targetY < 0 || targetY >= size.Height) {
                return;
            }

            int startX = x + offset.X;
            int endX = startX + width;

            if (endX <= 0 || startX >= size.Width) {
                return;
            }

            int clampedStart = Math.Max(0, startX);
            int clampedEnd = Math.Min(size.Width, endX);

            for (int drawX = clampedStart; drawX < clampedEnd; drawX++) {
                canvas[drawX, targetY] = new ConsolePixel(c, foregroundColor, backgroundColor);
            }
        }

        public void DrawVerticalLine(char c, int x, int y, int height, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
            if (height <= 0 || size.Width == 0 || size.Height == 0) {
                return;
            }

            int targetX = x + offset.X;
            if (targetX < 0 || targetX >= size.Width) {
                return;
            }

            int startY = y + offset.Y;
            int endY = startY + height;

            if (endY <= 0 || startY >= size.Height) {
                return;
            }

            int clampedStart = Math.Max(0, startY);
            int clampedEnd = Math.Min(size.Height, endY);

            for (int drawY = clampedStart; drawY < clampedEnd; drawY++) {
                canvas[targetX, drawY] = new ConsolePixel(c, foregroundColor, backgroundColor);
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
            if (width < 0 || height <= 0 || size.Width == 0 || size.Height == 0) {
                return;
            }

            int left = x + offset.X;
            int top = y + offset.Y;
            int right = left + width;
            int bottom = top + height - 1;

            if (top > size.Height - 1 || bottom < 0 || right < 0 || left > size.Width - 1) {
                return;
            }

            int clampedLeft = Math.Max(0, left);
            int clampedRight = Math.Min(size.Width - 1, right);
            int clampedTop = Math.Max(0, top);
            int clampedBottom = Math.Min(size.Height - 1, bottom);

            if (clampedLeft > clampedRight || clampedTop > clampedBottom) {
                return;
            }

            DrawHorizontalLine(borderHorizontal, clampedLeft - offset.X, clampedTop - offset.Y, clampedRight - clampedLeft + 1, foregroundColor, backgroundColor);
            DrawHorizontalLine(borderHorizontal, clampedLeft - offset.X, clampedBottom - offset.Y, clampedRight - clampedLeft + 1, foregroundColor, backgroundColor);
            DrawVerticalLine(borderVertical, clampedLeft - offset.X, clampedTop - offset.Y, clampedBottom - clampedTop + 1, foregroundColor, backgroundColor);
            DrawVerticalLine(borderVertical, clampedRight - offset.X, clampedTop - offset.Y, clampedBottom - clampedTop + 1, foregroundColor, backgroundColor);

            SetPixelIfVisible(clampedLeft, clampedTop, new ConsolePixel(borderTopLeft, foregroundColor, backgroundColor));
            SetPixelIfVisible(clampedRight, clampedTop, new ConsolePixel(borderTopRight, foregroundColor, backgroundColor));
            SetPixelIfVisible(clampedLeft, clampedBottom, new ConsolePixel(borderBottomLeft, foregroundColor, backgroundColor));
            SetPixelIfVisible(clampedRight, clampedBottom, new ConsolePixel(borderBottomRight, foregroundColor, backgroundColor));
        }

        public void FillRectangle(int x, int y, int width, int height, char c, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) {
            if (width <= 0 || height <= 0 || size.Width == 0 || size.Height == 0) {
                return;
            }

            int startX = x + offset.X;
            int startY = y + offset.Y;
            int endX = startX + width;
            int endY = startY + height;

            int clampedStartX = Math.Max(0, startX);
            int clampedEndX = Math.Min(size.Width, endX);
            int clampedStartY = Math.Max(0, startY);
            int clampedEndY = Math.Min(size.Height, endY);

            if (clampedStartX >= clampedEndX || clampedStartY >= clampedEndY) {
                return;
            }

            for (int px = clampedStartX; px < clampedEndX; px++) {
                for (int py = clampedStartY; py < clampedEndY; py++) {
                    canvas[px, py] = new ConsolePixel(c, foregroundColor, backgroundColor);
                }
            }
        }

        public void Render() {
            if (size.Width == 0 || size.Height == 0) {
                return;
            }

            if (useAnsiSequences) {
                RenderAnsi();
            } else {
                RenderBasic();
            }
        }

        void RenderAnsi() {
            TrySetCursorVisibility(false);
            SetCursorPositionSafe(0, 0);

            StringBuilder buffer = new StringBuilder(size.Width * size.Height + size.Height);

            ConsoleColor? setColor = null;
            ConsoleColor? setBgColor = null;

            buffer.Append(BackgroundColor == null ? RESET_BACKGROUND : ConsoleBackgroundColorToAnsi(BackgroundColor.Value));

            for (int y = 0; y < size.Height; y++) {
                for (int x = 0; x < size.Width; x++) {
                    ConsolePixel pixel = canvas[x, y];

                    if (pixel.ForegroundColor != setColor) {
                        if (pixel.ForegroundColor == null) {
                            setColor = null;
                            buffer.Append("\x1b[39m");
                        } else {
                            buffer.Append(ConsoleColorToAnsi(pixel.ForegroundColor.Value));
                            setColor = pixel.ForegroundColor;
                        }
                    }

                    if (pixel.BackgroundColor != setBgColor) {
                        if (pixel.BackgroundColor == null) {
                            setBgColor = null;
                            buffer.Append(BackgroundColor == null ? RESET_BACKGROUND : ConsoleBackgroundColorToAnsi(BackgroundColor.Value));
                        } else {
                            buffer.Append(ConsoleBackgroundColorToAnsi(pixel.BackgroundColor.Value));
                            setBgColor = pixel.BackgroundColor;
                        }
                    }

                    char outputChar = pixel.Character == '\0' ? ' ' : pixel.Character;
                    buffer.Append(outputChar);
                    canvas[x, y] = new ConsolePixel(' ');
                }
            }

            SafeConsoleWrite(buffer.ToString());
            SetCursorPositionSafe(0, 0);
            TryResetConsoleColors();
        }

        void RenderBasic() {
            TrySetCursorVisibility(false);

            bool repositionSupported = !cursorPositioningFailed && SetCursorPositionSafe(0, 0);
            if (!repositionSupported) {
                cursorPositioningFailed = true;
            }

            ConsoleColor currentForeground = defaultForeground;
            ConsoleColor currentBackground = defaultBackground;

            for (int y = 0; y < size.Height; y++) {
                if (repositionSupported) {
                    if (!SetCursorPositionSafe(0, y)) {
                        repositionSupported = false;
                        cursorPositioningFailed = true;
                    }
                }

                for (int x = 0; x < size.Width; x++) {
                    ConsolePixel pixel = canvas[x, y];

                    ConsoleColor targetForeground = pixel.ForegroundColor ?? defaultForeground;
                    ConsoleColor targetBackground = pixel.BackgroundColor ?? BackgroundColor ?? defaultBackground;

                    SetConsoleForeground(ref currentForeground, targetForeground);
                    SetConsoleBackground(ref currentBackground, targetBackground);

                    char outputChar = pixel.Character == '\0' ? ' ' : pixel.Character;
                    SafeConsoleWrite(outputChar);
                    canvas[x, y] = new ConsolePixel(' ');
                }

                if (!repositionSupported) {
                    SafeConsoleWrite(Environment.NewLine);
                }
            }

            if (repositionSupported) {
                SetCursorPositionSafe(0, 0);
            }

            ResetConsoleColors(currentForeground, currentBackground);
        }

        private void SetPixelIfVisible(int x, int y, ConsolePixel pixel) {
            if (x >= 0 && x < size.Width && y >= 0 && y < size.Height) {
                canvas[x, y] = pixel;
            }
        }

        private void TrySetCursorVisibility(bool visible) {
            try {
                Console.CursorVisible = visible;
            } catch {
                // ignored
            }
        }

        private bool SetCursorPositionSafe(int x, int y) {
            try {
                Console.SetCursorPosition(x, y);
                return true;
            } catch {
                return false;
            }
        }

        private void SetConsoleForeground(ref ConsoleColor currentColor, ConsoleColor target) {
            if (currentColor == target) {
                return;
            }

            try {
                Console.ForegroundColor = target;
                currentColor = target;
            } catch {
                // ignore consoles that forbid color changes
            }
        }

        private void SetConsoleBackground(ref ConsoleColor currentColor, ConsoleColor target) {
            if (currentColor == target) {
                return;
            }

            try {
                Console.BackgroundColor = target;
                currentColor = target;
            } catch {
                // ignore consoles that forbid color changes
            }
        }

        private void ResetConsoleColors(ConsoleColor currentForeground, ConsoleColor currentBackground) {
            try {
                if (currentForeground != defaultForeground) {
                    Console.ForegroundColor = defaultForeground;
                }

                if (currentBackground != defaultBackground) {
                    Console.BackgroundColor = defaultBackground;
                }
            } catch {
                // ignore best-effort reset failures
            }
        }

        private void TryResetConsoleColors() {
            try {
                Console.ResetColor();
            } catch {
                // ignore
            }
        }

        private void SafeConsoleWrite(char value) {
            try {
                Console.Write(value);
            } catch {
                // ignore write failures
            }
        }

        private void SafeConsoleWrite(string value) {
            try {
                Console.Write(value);
            } catch {
                // ignore write failures
            }
        }

        private static ConsoleColor SafeGetColor(Func<ConsoleColor> getter, ConsoleColor fallback) {
            try {
                return getter();
            } catch {
                return fallback;
            }
        }

        private static bool IsOutputRedirectedSafe() {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Unity doesn’t have a console redirect concept, fake it
        return false;
#else
            try {
                return Console.IsOutputRedirected;
            } catch {
                return false;
            }
#endif
        }

        public static string ConsoleColorToAnsi(ConsoleColor color) {
            return color switch {
                ConsoleColor.Black => "\x1b[30m",
                ConsoleColor.DarkRed => "\x1b[31m",
                ConsoleColor.DarkGreen => "\x1b[32m",
                ConsoleColor.DarkYellow => "\x1b[33m",
                ConsoleColor.DarkBlue => "\x1b[34m",
                ConsoleColor.DarkMagenta => "\x1b[35m",
                ConsoleColor.DarkCyan => "\x1b[36m",
                ConsoleColor.Gray => "\x1b[37m",
                ConsoleColor.DarkGray => "\x1b[90m",
                ConsoleColor.Red => "\x1b[91m",
                ConsoleColor.Green => "\x1b[92m",
                ConsoleColor.Yellow => "\x1b[93m",
                ConsoleColor.Blue => "\x1b[94m",
                ConsoleColor.Magenta => "\x1b[95m",
                ConsoleColor.Cyan => "\x1b[96m",
                ConsoleColor.White => "\x1b[97m",
                _ => "\x1b[39m"
            };
        }

        public static readonly string RESET_BACKGROUND = IsOutputRedirectedSafe() ? "" : "\x1b[49m";

        public static string ConsoleBackgroundColorToAnsi(ConsoleColor color) {
            return color switch {
                ConsoleColor.Black => "\x1b[40m",
                ConsoleColor.DarkRed => "\x1b[41m",
                ConsoleColor.DarkGreen => "\x1b[42m",
                ConsoleColor.DarkYellow => "\x1b[43m",
                ConsoleColor.DarkBlue => "\x1b[44m",
                ConsoleColor.DarkMagenta => "\x1b[45m",
                ConsoleColor.DarkCyan => "\x1b[46m",
                ConsoleColor.Gray => "\x1b[47m",
                ConsoleColor.DarkGray => "\x1b[100m",
                ConsoleColor.Red => "\x1b[101m",
                ConsoleColor.Green => "\x1b[102m",
                ConsoleColor.Yellow => "\x1b[103m",
                ConsoleColor.Blue => "\x1b[104m",
                ConsoleColor.Magenta => "\x1b[105m",
                ConsoleColor.Cyan => "\x1b[106m",
                ConsoleColor.White => "\x1b[107m",
                _ => RESET_BACKGROUND
            };
        }

        /// <summary>
        /// Determines whether Unicode box drawing characters should be used for borders.
        /// </summary>
        /// <param name="enableAnsiSequences">True when ANSI rendering is enabled.</param>
        /// <returns>True when Unicode borders should be used; otherwise false.</returns>
        static bool ResolveUnicodeBorderPreference(bool enableAnsiSequences) {
            string forceAscii = Environment.GetEnvironmentVariable("NUCLEUS_CONSOLE_ASCII");
            if (!string.IsNullOrWhiteSpace(forceAscii)) {
                return false;
            }

            string forceUnicode = Environment.GetEnvironmentVariable("NUCLEUS_CONSOLE_UNICODE");
            if (!string.IsNullOrWhiteSpace(forceUnicode)) {
                return true;
            }

            if (!OperatingSystem.IsWindows()) {
                return true;
            } else if (!enableAnsiSequences) {
                return false;
            }

            string wtSession = Environment.GetEnvironmentVariable("WT_SESSION");
            if (!string.IsNullOrWhiteSpace(wtSession)) {
                return true;
            }

            string conEmu = Environment.GetEnvironmentVariable("ConEmuANSI");
            if (!string.IsNullOrWhiteSpace(conEmu) && string.Equals(conEmu, "ON", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            string termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
            if (!string.IsNullOrWhiteSpace(termProgram)) {
                return true;
            }

            return false;
        }
    }
}
