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
            // TODO: Implement reset functionality
            await UniTask.Yield(cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Implement next page functionality
            await UniTask.Yield(cancellationToken);
            return false;
        }

        /// <inheritdoc />
        public bool HasNextPage()
        {
            // TODO: Implement has next page logic
            return false;
        }
    }
}