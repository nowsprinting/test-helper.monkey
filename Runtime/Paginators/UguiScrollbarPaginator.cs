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
    /// Paginator implementation for <see cref="Scrollbar"/>.
    /// </summary>
    public class UguiScrollbarPaginator : IPaginator
    {
        private readonly Scrollbar _scrollbar;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scrollbar">Scrollbar to be controlled</param>
        /// <exception cref="ArgumentNullException">When scrollbar is null</exception>
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
            // For Scrollbar, if value is less than 1.0, the next page exists
            return _scrollbar.value < 1.0f - float.Epsilon;
        }

        private float CalculateNormalizedScrollAmount()
        {
            // Use the size property of Scrollbar (represents the ratio of the display area)
            return _scrollbar.size;
        }
    }
}
