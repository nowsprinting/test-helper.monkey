// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators.Utils;
using TestHelper.Random;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Scroll wheel operator for Unity UI (uGUI) components that implements IScrollHandler.
    /// </summary>
    public class UguiScrollWheelOperator : IScrollWheelOperator
    {
        private readonly float _scrollPerFrame;
        private readonly ILogger _logger;
        private readonly IRandom _random;
        private readonly ScreenshotOptions _screenshotOptions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scrollPerFrame">Scroll distance per frame (must be positive)</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger</param>
        /// <param name="random">PRNG instance</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        /// <exception cref="ArgumentException">Thrown when scrollDistance is zero or negative</exception>
        public UguiScrollWheelOperator(float scrollPerFrame, ILogger logger = null, IRandom random = null,
            ScreenshotOptions screenshotOptions = null)
        {
            if (scrollPerFrame <= 0)
            {
                throw new ArgumentException("scrollPerFrame must be positive", nameof(scrollPerFrame));
            }

            _scrollPerFrame = scrollPerFrame;
            _logger = logger ?? Debug.unityLogger;
            _random = random ?? new RandomWrapper();
            _screenshotOptions = screenshotOptions;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            return gameObject.TryGetEnabledComponent<IScrollHandler>(out _);
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            var distance = CalcMaxScrollDistance(gameObject);
            var destination = new Vector2(
                _random.Next(-distance, distance),
                _random.Next(-distance, distance)
            );
            await OperateAsync(gameObject, raycastResult, destination, logger, screenshotOptions, cancellationToken);
        }

        private static int CalcMaxScrollDistance(GameObject gameObject)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return 200;
            }

            return (int)Math.Max(rectTransform.rect.width, rectTransform.rect.height);
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult, Vector2 destination,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            logger = logger ?? _logger;
            screenshotOptions = screenshotOptions ?? _screenshotOptions;

            // Output log before the operation
            var operationLogger = new OperationLogger(gameObject, this, logger, screenshotOptions);
            operationLogger.Properties.Add("destination", destination);
            await operationLogger.Log();

            // Send pointer enter event
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = raycastResult.screenPosition
            };
            ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);

            // Perform scroll operation
            if (destination.magnitude > 0)
            {
                var remainingDistance = destination.magnitude;
                var direction = destination.normalized;

                while (remainingDistance > 0 && !cancellationToken.IsCancellationRequested)
                {
                    var scrollDelta = direction * Mathf.Min(_scrollPerFrame, remainingDistance);
                    pointerEventData.scrollDelta = scrollDelta;

                    ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.scrollHandler);

                    remainingDistance -= _scrollPerFrame;

                    await UniTask.Yield(cancellationToken);
                }
            }

            // Send pointer exit event
            ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
        }
    }
}
