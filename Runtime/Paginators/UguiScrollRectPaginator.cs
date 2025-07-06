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
    /// ScrollRect用のページネーター実装
    /// </summary>
    public class UguiScrollRectPaginator : IPaginator
    {
        private readonly ScrollRect _scrollRect;
        private bool _isHorizontalAtEnd;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scrollRect">制御対象のScrollRect</param>
        /// <exception cref="ArgumentNullException">scrollRectがnullの場合</exception>
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
            _scrollRect.normalizedPosition = new Vector2(0f, 0f);
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
                // 両方向スクロールの場合
                if (!_isHorizontalAtEnd && !IsHorizontalAtEnd())
                {
                    // 水平方向に移動
                    var horizontalAmount = CalculateHorizontalScrollAmount();
                    var newX = Mathf.Min(currentPosition.x + horizontalAmount, 1f);
                    _scrollRect.normalizedPosition = new Vector2(newX, currentPosition.y);
                    _isHorizontalAtEnd = newX >= 1f - float.Epsilon;
                }
                else
                {
                    // 水平方向が終端なので、左端に戻って垂直方向に移動
                    var verticalAmount = CalculateVerticalScrollAmount();
                    var newY = Mathf.Max(currentPosition.y - verticalAmount, 0f);
                    _scrollRect.normalizedPosition = new Vector2(0f, newY);
                    _isHorizontalAtEnd = false;
                }
            }
            else if (_scrollRect.horizontal)
            {
                // 水平方向のみ
                var horizontalAmount = CalculateHorizontalScrollAmount();
                var newX = Mathf.Min(currentPosition.x + horizontalAmount, 1f);
                _scrollRect.normalizedPosition = new Vector2(newX, currentPosition.y);
            }
            else if (_scrollRect.vertical)
            {
                // 垂直方向のみ
                var verticalAmount = CalculateVerticalScrollAmount();
                var newY = Mathf.Max(currentPosition.y - verticalAmount, 0f);
                _scrollRect.normalizedPosition = new Vector2(currentPosition.x, newY);
            }
            else
            {
                // スクロール無効
                return false;
            }

            await UniTask.Yield(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public bool HasNextPage()
        {
            // 両方向スクロールの場合の終端判定
            if (_scrollRect.horizontal && _scrollRect.vertical)
            {
                // 水平・垂直両方向とも終端に達している場合のみfalse
                return !(IsHorizontalAtEnd() && IsVerticalAtEnd());
            }
            
            // 単方向スクロールの場合
            if (_scrollRect.horizontal)
            {
                return !IsHorizontalAtEnd();
            }
            
            if (_scrollRect.vertical)
            {
                return !IsVerticalAtEnd();
            }
            
            // スクロールが無効の場合
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
                return 0f; // コンテンツがビューポートより小さい場合
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
                return 0f; // コンテンツがビューポートより小さい場合
            }

            return viewportSize.y / (contentSize.y - viewportSize.y);
        }
    }
}
