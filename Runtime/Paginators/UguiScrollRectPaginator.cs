// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Paginators
{
    /// <summary>
    /// Paginator implementation for <see cref="ScrollRect"/>.
    /// </summary>
    public class UguiScrollRectPaginator : IPaginator
    {
        private readonly ScrollRect _scrollRect;
        private bool _isHorizontalAtEnd;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scrollRect">ScrollRect to be controlled</param>
        /// <exception cref="ArgumentNullException">When scrollRect is null</exception>
        public UguiScrollRectPaginator(ScrollRect scrollRect)
        {
            _scrollRect = scrollRect ?? throw new ArgumentNullException(nameof(scrollRect));

            if (_scrollRect.content == null)
            {
                throw new ArgumentNullException(nameof(scrollRect.content), "ScrollRect.content is null");
            }
        }

        /// <inheritdoc />
        public async UniTask ResetAsync(CancellationToken cancellationToken = default)
        {
            _scrollRect.normalizedPosition = new Vector2(0f, 1f);
            _isHorizontalAtEnd = false;
            await UniTask.Yield(cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default)
        {
            if (!HasNextPage())
            {
                return false;
            }

            var currentPosition = _scrollRect.normalizedPosition;

            if (_scrollRect.horizontal && _scrollRect.vertical)
            {
                // For both scrolling
                if (!_isHorizontalAtEnd && !IsHorizontalAtEnd())
                {
                    // Move horizontally
                    var horizontalAmount = CalculateHorizontalScrollAmount();
                    var newX = Mathf.Min(currentPosition.x + horizontalAmount, 1f);
                    _scrollRect.normalizedPosition = new Vector2(newX, currentPosition.y);
                    _isHorizontalAtEnd = newX >= 1f - float.Epsilon;
                }
                else
                {
                    // Horizontal direction is at the end, so return to the left and move vertically
                    var verticalAmount = CalculateVerticalScrollAmount();
                    var newY = Mathf.Max(currentPosition.y - verticalAmount, 0f);
                    _scrollRect.normalizedPosition = new Vector2(0f, newY);
                    _isHorizontalAtEnd = false;
                }
            }
            else if (_scrollRect.horizontal)
            {
                // Horizontal direction only
                var horizontalAmount = CalculateHorizontalScrollAmount();
                var newX = Mathf.Min(currentPosition.x + horizontalAmount, 1f);
                _scrollRect.normalizedPosition = new Vector2(newX, currentPosition.y);
            }
            else if (_scrollRect.vertical)
            {
                // Vertical direction only
                var verticalAmount = CalculateVerticalScrollAmount();
                var newY = Mathf.Max(currentPosition.y - verticalAmount, 0f);
                _scrollRect.normalizedPosition = new Vector2(currentPosition.x, newY);
            }
            else
            {
                // Scrolling disabled
                return false;
            }

            await UniTask.Yield(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public bool HasNextPage()
        {
            // End determination for bidirectional scrolling
            if (_scrollRect.horizontal && _scrollRect.vertical)
            {
                // Return false only when both horizontal and vertical directions have reached the end
                return !(IsHorizontalAtEnd() && IsVerticalAtEnd());
            }

            // For unidirectional scrolling
            if (_scrollRect.horizontal)
            {
                return !IsHorizontalAtEnd();
            }

            if (_scrollRect.vertical)
            {
                return !IsVerticalAtEnd();
            }

            // When scrolling is disabled
            return false;
        }

        private Vector2 CalculateViewportSize()
        {
            var viewport = _scrollRect.viewport ?? _scrollRect.transform as RectTransform;
            return viewport?.rect.size ?? Vector2.zero;
        }

        private bool IsHorizontalAtEnd()
        {
            return _scrollRect.normalizedPosition.x >= 1.0f - float.Epsilon;
        }

        private bool IsVerticalAtEnd()
        {
            return _scrollRect.normalizedPosition.y <= 0.0f + float.Epsilon;
        }

        private float CalculateHorizontalScrollAmount()
        {
            if (!_scrollRect.horizontal)
            {
                return 0f;
            }

            var viewportSize = CalculateViewportSize();
            var contentSize = _scrollRect.content.rect.size;

            if (contentSize.x <= viewportSize.x)
            {
                return 0f; // When content is smaller than viewport
            }

            return viewportSize.x / (contentSize.x - viewportSize.x);
        }

        private float CalculateVerticalScrollAmount()
        {
            if (!_scrollRect.vertical)
            {
                return 0f;
            }

            var viewportSize = CalculateViewportSize();
            var contentSize = _scrollRect.content.rect.size;

            if (contentSize.y <= viewportSize.y)
            {
                return 0f; // When content is smaller than viewport
            }

            return viewportSize.y / (contentSize.y - viewportSize.y);
        }
    }
}
