using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nucleus.ConsoleEngine {
    public class ConsoleMenu {
        public List<ConsoleMenuOption> Options { get; set; }

        private ConsoleMenuOption? _selectedOption;
        private int _lastMeasuredWidth;
        private int _lastMeasuredHeight;
        private readonly List<ConsoleMenuOption> _orderedBuffer = new();

        public ConsoleColor SelectedColor { get; set; }
        public ConsoleColor SelectedBgColor { get; set; }

        public bool FillText { get; set; } = true;
        public bool Offset { get; set; } = true;

        public ConsoleGraphics Graphics { get; private set; }

        public ConsoleMenu(ConsoleGraphics graphics) {
            // Menus share a single graphics surface provided by the caller.
            this.Graphics = graphics;

            Options = new List<ConsoleMenuOption>();

            SelectedColor = ConsoleColor.White;
            SelectedBgColor = ConsoleColor.Magenta;

            _lastMeasuredWidth = graphics.Width;
            _lastMeasuredHeight = graphics.Height;
        }

        public virtual void OnShow() {

        }

        public void Render(double elapsed) {
            EnsureAnchorsUpToDate();
            EnsureSelectionValid();

            for (int i = 0; i < Options.Count; i++) {
                ConsoleMenuOption option = Options[i];
                bool selected = ReferenceEquals(option, _selectedOption);
                option.Render(this, selected, elapsed);
            }
        }

        public void ReceiveInput(ConsoleKey key) {
            if (Options.Count == 0) {
                _selectedOption = null;
                return;
            }

            EnsureSelectionValid();

            // Map arrow navigation into ordered traversals so focus follows the layout.
            switch (key) {
                case ConsoleKey.UpArrow:
                    SelectRelative(OrderedByVertical(), -1);
                    break;
                case ConsoleKey.DownArrow:
                    SelectRelative(OrderedByVertical(), 1);
                    break;
                case ConsoleKey.LeftArrow:
                    SelectRelative(OrderedByHorizontal(), -1);
                    break;
                case ConsoleKey.RightArrow:
                    SelectRelative(OrderedByHorizontal(), 1);
                    break;
                case ConsoleKey.Enter:
                    _selectedOption?.Callback?.Invoke();
                    break;
                default:
                    _selectedOption?.ReceiveKey(key);
                    break;
            }
        }

        public ConsoleMenuOption? SelectedOption {
            get => _selectedOption;
            set {
                if (value != null && !Options.Contains(value)) {
                    _selectedOption = null;
                    EnsureSelectionValid();
                } else {
                    _selectedOption = value;
                }
            }
        }

        public void ClearSelection() {
            _selectedOption = null;
        }

        private void SelectRelative(List<ConsoleMenuOption> ordered, int delta) {
            if (ordered.Count == 0) {
                return;
            }

            int currentIndex = _selectedOption != null ? ordered.IndexOf(_selectedOption) : -1;
            if (currentIndex == -1) {
                _selectedOption = ordered[0];
                return;
            }

            int nextIndex = currentIndex + delta;
            if (nextIndex < 0 || nextIndex >= ordered.Count) {
                return;
            }

            _selectedOption = ordered[nextIndex];
        }

        private List<ConsoleMenuOption> OrderedByVertical() {
            _orderedBuffer.Clear();
            _orderedBuffer.AddRange(Options);
            int containerHeight = Graphics.Height;
            int containerWidth = Graphics.Width;
            _orderedBuffer.Sort((a, b) => {
                int yCompare = ComputeEffectiveY(a, containerHeight).CompareTo(ComputeEffectiveY(b, containerHeight));
                if (yCompare != 0) {
                    return yCompare;
                }

                return ComputeEffectiveX(a, containerWidth).CompareTo(ComputeEffectiveX(b, containerWidth));
            });
            return _orderedBuffer;
        }

        private List<ConsoleMenuOption> OrderedByHorizontal() {
            _orderedBuffer.Clear();
            _orderedBuffer.AddRange(Options);
            int containerHeight = Graphics.Height;
            int containerWidth = Graphics.Width;
            _orderedBuffer.Sort((a, b) => {
                int xCompare = ComputeEffectiveX(a, containerWidth).CompareTo(ComputeEffectiveX(b, containerWidth));
                if (xCompare != 0) {
                    return xCompare;
                }

                return ComputeEffectiveY(a, containerHeight).CompareTo(ComputeEffectiveY(b, containerHeight));
            });
            return _orderedBuffer;
        }

        private static int ComputeEffectiveY(ConsoleMenuOption option, int containerHeight) {
            ConsoleAnchorStyles anchor = option.Anchor;
            bool anchorTop = anchor.HasFlag(ConsoleAnchorStyles.Top);
            bool anchorBottom = anchor.HasFlag(ConsoleAnchorStyles.Bottom);

            if (!anchorTop && anchorBottom) {
                return containerHeight - option.Location.Y - option.Dimensions.Height;
            }

            return option.Location.Y;
        }

        private static int ComputeEffectiveX(ConsoleMenuOption option, int containerWidth) {
            ConsoleAnchorStyles anchor = option.Anchor;
            bool anchorLeft = anchor.HasFlag(ConsoleAnchorStyles.Left);
            bool anchorRight = anchor.HasFlag(ConsoleAnchorStyles.Right);

            if (!anchorLeft && anchorRight) {
                return containerWidth - option.Location.X - option.Dimensions.Width;
            }

            return option.Location.X;
        }

        private void EnsureSelectionValid() {
            if (_selectedOption != null && Options.Contains(_selectedOption)) {
                return;
            }

            _selectedOption = Options.FirstOrDefault();
        }

        private void EnsureAnchorsUpToDate() {
            int width = Graphics.Width;
            int height = Graphics.Height;

            if (width == _lastMeasuredWidth && height == _lastMeasuredHeight) {
                return;
            }

            // When the console size changes we recalc anchor offsets so options stay aligned.
            for (int i = 0; i < Options.Count; i++) {
                Options[i].ResetAnchorReference();
            }

            _lastMeasuredWidth = width;
            _lastMeasuredHeight = height;
        }
    }
}
