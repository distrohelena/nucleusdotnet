using System;
using System.Drawing;

namespace Nucleus.ConsoleEngine {
    /// <summary>
    /// Base visual element for the console UI surface.
    /// </summary>
    /// <remarks>
    /// Extracted so non-interactive controls such as labels no longer need to inherit <see cref="ConsoleMenuOption"/>.
    /// This keeps anchor and rendering infrastructure shared while letting passive elements stay lightweight, per the console navigation refactor.
    /// </remarks>
    public abstract class ConsoleControl {
        private bool _anchorInitialized;
        private int _initialLeft;
        private int _initialTop;
        private int _initialRight;
        private int _initialBottom;

        private Point _location;
        private Size _dimensions;
        private ConsoleAnchorStyles _anchorStyles;

        protected ConsoleControl(Point location) {
            _location = location;
            _dimensions = new Size(30, 1);
            _anchorStyles = ConsoleAnchorStyles.Top | ConsoleAnchorStyles.Left;
        }

        protected ConsoleControl(Rectangle bounds) : this(bounds.Location) {
            _dimensions = bounds.Size;
        }

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

        public ConsoleAnchorStyles Anchor {
            get => _anchorStyles;
            set {
                if (_anchorStyles != value) {
                    _anchorStyles = value;
                    ResetAnchorReference();
                }
            }
        }

        public virtual bool IsSelectable => false;

        public virtual void Render(ConsoleMenu menu, bool selected, double elapsed) {
        }

        public virtual void ReceiveKey(ConsoleKey key) {
        }

        public void ResetAnchorReference() {
            _anchorInitialized = false;
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
    }
}
