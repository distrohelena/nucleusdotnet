using System;
using System.Collections.Generic;
using System.Linq;

namespace Nucleus.ConsoleEngine {
    public class ConsoleMenu {
        public List<ConsoleControl> Controls { get; set; }

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

            Controls = new List<ConsoleControl>();

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

            for (int i = 0; i < Controls.Count; i++) {
                ConsoleControl control = Controls[i];
                ConsoleMenuOption? option = control as ConsoleMenuOption;
                bool selected = option != null && ReferenceEquals(option, _selectedOption);
                control.Render(this, selected, elapsed);
            }
        }

        public void ReceiveInput(ConsoleKey key) {
            if (!Controls.Any(option => option.IsSelectable)) {
                _selectedOption = null;
                return;
            }

            EnsureSelectionValid();

            // Map arrow navigation into ordered traversals so focus follows the layout.
            switch (key) {
                case ConsoleKey.UpArrow:
                    SelectVertical(-1);
                    break;
                case ConsoleKey.DownArrow:
                    SelectVertical(1);
                    break;
                case ConsoleKey.LeftArrow:
                    SelectHorizontal(-1);
                    break;
                case ConsoleKey.RightArrow:
                    SelectHorizontal(1);
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
                if (value != null && (!Controls.Contains(value) || !value.IsSelectable)) {
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

        private List<ConsoleMenuOption> OrderedByVertical() {
            _orderedBuffer.Clear();
            for (int i = 0; i < Controls.Count; i++) {
                if (Controls[i] is ConsoleMenuOption option && option.IsSelectable) {
                    _orderedBuffer.Add(option);
                }
            }
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

        private static int ComputeEffectiveY(ConsoleControl option, int containerHeight) {
            ConsoleAnchorStyles anchor = option.Anchor;
            bool anchorTop = anchor.HasFlag(ConsoleAnchorStyles.Top);
            bool anchorBottom = anchor.HasFlag(ConsoleAnchorStyles.Bottom);

            if (!anchorTop && anchorBottom) {
                return containerHeight - option.Location.Y - option.Dimensions.Height;
            }

            return option.Location.Y;
        }

        private static int ComputeEffectiveX(ConsoleControl option, int containerWidth) {
            ConsoleAnchorStyles anchor = option.Anchor;
            bool anchorLeft = anchor.HasFlag(ConsoleAnchorStyles.Left);
            bool anchorRight = anchor.HasFlag(ConsoleAnchorStyles.Right);

            if (!anchorLeft && anchorRight) {
                return containerWidth - option.Location.X - option.Dimensions.Width;
            }

            return option.Location.X;
        }

        private void SelectHorizontal(int direction) {
            if (_selectedOption == null) {
                EnsureSelectionValid();
                return;
            }

            int containerWidth = Graphics.Width;
            int containerHeight = Graphics.Height;
            ConsoleMenuOption current = _selectedOption;

            int currentX = ComputeEffectiveX(current, containerWidth);
            int currentTop = ComputeEffectiveY(current, containerHeight);
            int currentHeight = Math.Max(1, current.Dimensions.Height);
            int currentBottom = currentTop + currentHeight;
            int currentCenterY = currentTop + (currentHeight / 2);

            ConsoleMenuOption? bestAligned = null;
            int bestAlignedDeltaX = int.MaxValue;
            int bestAlignedDeltaY = int.MaxValue;
            ConsoleMenuOption? bestFallback = null;
            int bestFallbackDeltaX = int.MaxValue;
            int bestFallbackDeltaY = int.MaxValue;

            for (int i = 0; i < Controls.Count; i++) {
                if (Controls[i] is not ConsoleMenuOption candidate) {
                    continue;
                }
                if (ReferenceEquals(candidate, current)) {
                    continue;
                }
                if (!candidate.IsSelectable) {
                    continue;
                }

                int candidateX = ComputeEffectiveX(candidate, containerWidth);
                int candidateTop = ComputeEffectiveY(candidate, containerHeight);
                int candidateHeight = Math.Max(1, candidate.Dimensions.Height);
                int candidateBottom = candidateTop + candidateHeight;
                int candidateCenterY = candidateTop + (candidateHeight / 2);

                int deltaX = direction > 0 ? candidateX - currentX : currentX - candidateX;
                if (deltaX <= 0) {
                    continue;
                }

                int deltaY = Math.Abs(candidateCenterY - currentCenterY);
                bool verticallyAligned = candidateBottom > currentTop && candidateTop < currentBottom;

                if (verticallyAligned) {
                    if (deltaX < bestAlignedDeltaX || (deltaX == bestAlignedDeltaX && deltaY < bestAlignedDeltaY)) {
                        bestAligned = candidate;
                        bestAlignedDeltaX = deltaX;
                        bestAlignedDeltaY = deltaY;
                    }
                } else if (deltaX < bestFallbackDeltaX || (deltaX == bestFallbackDeltaX && deltaY < bestFallbackDeltaY)) {
                    bestFallback = candidate;
                    bestFallbackDeltaX = deltaX;
                    bestFallbackDeltaY = deltaY;
                }
            }

            // Prefer options that overlap vertically with the current focus before falling back.
            if (bestAligned != null) {
                _selectedOption = bestAligned;
            } else if (bestFallback != null) {
                _selectedOption = bestFallback;
            }
        }

        private void SelectVertical(int direction) {
            if (_selectedOption == null) {
                EnsureSelectionValid();
                return;
            }

            int containerWidth = Graphics.Width;
            int containerHeight = Graphics.Height;
            ConsoleMenuOption current = _selectedOption;

            int currentTop = ComputeEffectiveY(current, containerHeight);
            int currentHeight = Math.Max(1, current.Dimensions.Height);
            int currentBottom = currentTop + currentHeight;
            int currentCenterY = currentTop + (currentHeight / 2);

            int currentLeft = ComputeEffectiveX(current, containerWidth);
            int currentWidth = Math.Max(1, current.Dimensions.Width);
            int currentRight = currentLeft + currentWidth;
            int currentCenterX = currentLeft + (currentWidth / 2);

            ConsoleMenuOption? bestAligned = null;
            int bestAlignedDeltaY = int.MaxValue;
            int bestAlignedDeltaX = int.MaxValue;
            ConsoleMenuOption? bestFallback = null;
            int bestFallbackDeltaY = int.MaxValue;
            int bestFallbackDeltaX = int.MaxValue;

            for (int i = 0; i < Controls.Count; i++) {
                if (Controls[i] is not ConsoleMenuOption candidate) {
                    continue;
                }
                if (ReferenceEquals(candidate, current)) {
                    continue;
                }
                if (!candidate.IsSelectable) {
                    continue;
                }

                int candidateTop = ComputeEffectiveY(candidate, containerHeight);
                int candidateHeight = Math.Max(1, candidate.Dimensions.Height);
                int candidateBottom = candidateTop + candidateHeight;
                int candidateCenterY = candidateTop + (candidateHeight / 2);

                int candidateLeft = ComputeEffectiveX(candidate, containerWidth);
                int candidateWidth = Math.Max(1, candidate.Dimensions.Width);
                int candidateRight = candidateLeft + candidateWidth;
                int candidateCenterX = candidateLeft + (candidateWidth / 2);

                int deltaY = direction < 0
                    ? currentCenterY - candidateCenterY
                    : candidateCenterY - currentCenterY;

                if (deltaY <= 0) {
                    continue;
                }

                int deltaX = Math.Abs(candidateCenterX - currentCenterX);
                bool horizontallyAligned = candidateRight > currentLeft && candidateLeft < currentRight;

                if (horizontallyAligned) {
                    if (deltaY < bestAlignedDeltaY || (deltaY == bestAlignedDeltaY && deltaX < bestAlignedDeltaX)) {
                        bestAligned = candidate;
                        bestAlignedDeltaY = deltaY;
                        bestAlignedDeltaX = deltaX;
                    }
                } else if (deltaY < bestFallbackDeltaY || (deltaY == bestFallbackDeltaY && deltaX < bestFallbackDeltaX)) {
                    bestFallback = candidate;
                    bestFallbackDeltaY = deltaY;
                    bestFallbackDeltaX = deltaX;
                }
            }

            if (bestAligned != null) {
                _selectedOption = bestAligned;
            } else if (bestFallback != null) {
                _selectedOption = bestFallback;
            }
        }

        private void EnsureSelectionValid() {
            if (_selectedOption != null && Controls.Contains(_selectedOption) && _selectedOption.IsSelectable) {
                return;
            }

            _selectedOption = Controls
                .OfType<ConsoleMenuOption>()
                .FirstOrDefault(option => option.IsSelectable);
        }

        private void EnsureAnchorsUpToDate() {
            int width = Graphics.Width;
            int height = Graphics.Height;

            if (width == _lastMeasuredWidth && height == _lastMeasuredHeight) {
                return;
            }

            // When the console size changes we recalc anchor offsets so options stay aligned.
            for (int i = 0; i < Controls.Count; i++) {
                Controls[i].ResetAnchorReference();
            }

            _lastMeasuredWidth = width;
            _lastMeasuredHeight = height;
        }
    }
}
