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

    public class ConsoleMenuOption {
        // Cached anchor data avoids recomputing offsets unless layout inputs change.
        private bool _anchorInitialized;
        private int _initialLeft;
        private int _initialTop;
        private int _initialRight;
        private int _initialBottom;

        private Point _location;
        private Size _dimensions;
        private ConsoleAnchorStyles _anchorStyles;

        public Point Location {
            get => _location;
            set {
                if (_location != value) {
                    _location = value;
                    ResetAnchorReference();
                }
            }
        }

        public Size Dimensions {
            get => _dimensions;
            set {
                if (_dimensions != value) {
                    _dimensions = value;
                    ResetAnchorReference();
                }
            }
        }
        public string Text { get; set; }

        public ConsoleColor? Color { get; set; }
        public ConsoleColor? BackgroundColor { get; set; }
        public ConsoleColor? BorderColor { get; set; }

        public ConsoleAnchorStyles Anchor {
            get => _anchorStyles;
            set {
                if (_anchorStyles != value) {
                    _anchorStyles = value;
                    ResetAnchorReference();
                }
            }
        }

        public Action Callback { get; set; }

        public ConsoleMenuOption(
            Point location,
            string text,
            Action callback,
            ConsoleColor? color = null,
            ConsoleColor? bgColor = null
        ) {
            _location = location;
            _dimensions = new Size(30, 1);
            Text = text;
            Color = color;
            BackgroundColor = bgColor;
            Callback = callback;
            BorderColor = ConsoleColor.White;
            _anchorStyles = ConsoleAnchorStyles.Top | ConsoleAnchorStyles.Left;
        }

        public ConsoleMenuOption(
            Rectangle bounds,
            string text,
            Action callback,
            ConsoleColor? color = null,
            ConsoleColor? bgColor = null
        ) : this(bounds.Location, text, callback, color, bgColor) {
            _dimensions = bounds.Size;
        }

        public void ResetAnchorReference() {
            _anchorInitialized = false;
        }

        public virtual void Render(ConsoleMenu menu, bool selected, double elapsed) {
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

        public virtual void ReceiveKey(ConsoleKey key) {
        }

        protected Rectangle GetAnchoredBounds(ConsoleMenu menu) {
            EnsureAnchorInitialized(menu);

            int containerWidth = menu.Graphics.Width;
            int containerHeight = menu.Graphics.Height;

            int width = Math.Max(1, Dimensions.Width);
            int height = Math.Max(1, Dimensions.Height);

            bool anchorLeft = Anchor.HasFlag(ConsoleAnchorStyles.Left);
            bool anchorRight = Anchor.HasFlag(ConsoleAnchorStyles.Right);
            bool anchorTop = Anchor.HasFlag(ConsoleAnchorStyles.Top);
            bool anchorBottom = Anchor.HasFlag(ConsoleAnchorStyles.Bottom);

            if (!anchorLeft && !anchorRight) {
                anchorLeft = true;
            }

            if (!anchorTop && !anchorBottom) {
                anchorTop = true;
            }

            int x;
            if (anchorLeft && anchorRight) {
                width = containerWidth - _initialLeft - _initialRight;
                x = _initialLeft;
            } else if (anchorRight) {
                x = containerWidth - _initialRight - width;
            } else {
                x = _initialLeft;
            }

            int y;
            if (anchorTop && anchorBottom) {
                height = containerHeight - _initialTop - _initialBottom;
                y = _initialTop;
            } else if (anchorBottom) {
                y = containerHeight - _initialBottom - height;
            } else {
                y = _initialTop;
            }

            width = Math.Max(1, Math.Min(width, containerWidth));
            height = Math.Max(1, Math.Min(height, containerHeight));

            if (x < 0) {
                x = 0;
            }
            if (x + width > containerWidth) {
                x = Math.Max(0, containerWidth - width);
            }

            if (y < 0) {
                y = 0;
            }
            if (y + height > containerHeight) {
                y = Math.Max(0, containerHeight - height);
            }

            return new Rectangle(x, y, width, height);
        }

        private void EnsureAnchorInitialized(ConsoleMenu menu) {
            if (_anchorInitialized) {
                return;
            }

            int containerWidth = Math.Max(1, menu.Graphics.Width);
            int containerHeight = Math.Max(1, menu.Graphics.Height);

            bool anchorLeft = Anchor.HasFlag(ConsoleAnchorStyles.Left);
            bool anchorRight = Anchor.HasFlag(ConsoleAnchorStyles.Right);
            bool anchorTop = Anchor.HasFlag(ConsoleAnchorStyles.Top);
            bool anchorBottom = Anchor.HasFlag(ConsoleAnchorStyles.Bottom);

            if (!anchorLeft && !anchorRight) {
                anchorLeft = true;
            }

            if (!anchorTop && !anchorBottom) {
                anchorTop = true;
            }

            // Record margins from the appropriate edges so future resizes can honour the chosen anchors.
            if (anchorLeft && !anchorRight) {
                _initialLeft = Location.X;
                _initialRight = Math.Max(0, containerWidth - (Location.X + Dimensions.Width));
            } else if (!anchorLeft && anchorRight) {
                _initialRight = Location.X;
                _initialLeft = Math.Max(0, containerWidth - Dimensions.Width - _initialRight);
            } else {
                _initialLeft = Location.X;
                _initialRight = Math.Max(0, containerWidth - (Location.X + Dimensions.Width));
            }

            if (anchorTop && !anchorBottom) {
                _initialTop = Location.Y;
                _initialBottom = Math.Max(0, containerHeight - (Location.Y + Dimensions.Height));
            } else if (!anchorTop && anchorBottom) {
                _initialBottom = Location.Y;
                _initialTop = Math.Max(0, containerHeight - (Location.Y + Dimensions.Height));
            } else {
                _initialTop = Location.Y;
                _initialBottom = Math.Max(0, containerHeight - (Location.Y + Dimensions.Height));
            }

            _anchorInitialized = true;
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
                if (remaining > 0) {
                    text += StringUtil.Repeat(" ", remaining);
                }
            }

            return text;
        }
    }
}
