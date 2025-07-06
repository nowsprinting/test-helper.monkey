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
    /// Scrollbar用のページネーター実装
    /// </summary>
    public class UguiScrollbarPaginator : IPaginator
    {
        private readonly Scrollbar _scrollbar;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scrollbar">制御対象のScrollbar</param>
        /// <exception cref="ArgumentNullException">scrollbarがnullの場合</exception>
        public UguiScrollbarPaginator(Scrollbar scrollbar)
        {
            _scrollbar = scrollbar ?? throw new ArgumentNullException(nameof(scrollbar));
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