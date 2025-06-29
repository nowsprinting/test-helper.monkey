// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Scroll wheel operator for Unity UI (uGUI) components that implements IScrollHandler.
    /// </summary>
    public class UguiScrollWheelOperator : IScrollWheelOperator
    {
        private readonly float _scrollDistance;
        private readonly ILogger _logger;
        private readonly ScreenshotOptions _screenshotOptions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scrollDistance">Scroll distance per frame (must be positive)</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        /// <exception cref="ArgumentException">Thrown when scrollDistance is zero or negative</exception>
        public UguiScrollWheelOperator(float scrollDistance, ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            if (scrollDistance <= 0)
            {
                throw new ArgumentException("scrollDistance must be positive", nameof(scrollDistance));
            }

            _scrollDistance = scrollDistance;
            _logger = logger ?? Debug.unityLogger;
            _screenshotOptions = screenshotOptions;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
            // TODO: Implement actual logic
            return false;
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: Generate random destination and call the overload method
            await UniTask.CompletedTask;
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult, Vector2 destination,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement actual scroll operation
            await UniTask.CompletedTask;
        }
    }
}