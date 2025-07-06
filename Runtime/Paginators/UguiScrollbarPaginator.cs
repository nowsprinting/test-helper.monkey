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
            _scrollbar.value = 0f;
            await UniTask.Yield(cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default)
        {
            if (!HasNextPage())
            {
                return false;
            }

            var currentValue = _scrollbar.value;
            var scrollAmount = CalculateNormalizedScrollAmount();
            var newValue = Mathf.Min(currentValue + scrollAmount, 1f);
            
            _scrollbar.value = newValue;
            await UniTask.Yield(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public bool HasNextPage()
        {
            // Scrollbarの場合、valueが1.0未満であれば次のページが存在
            return _scrollbar.value < 1.0f - float.Epsilon;
        }

        private float CalculateNormalizedScrollAmount()
        {
            // Scrollbarのsizeプロパティを使用（表示領域の比率を表す）
            return _scrollbar.size;
        }
    }
}
