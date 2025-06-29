// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators.Utils;
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
            var randomDestination = new Vector2(
                UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-10f, 10f)
            );

            await OperateAsync(gameObject, raycastResult, randomDestination, logger, screenshotOptions, cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult, Vector2 destination,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            logger = logger ?? _logger;
            screenshotOptions = screenshotOptions ?? _screenshotOptions;

            if (!gameObject.TryGetEnabledComponent<IScrollHandler>(out var scrollHandler))
            {
                return;
            }

            // Output log before the operation
            var operationLogger = new OperationLogger(gameObject, this, logger, screenshotOptions);
            operationLogger.Properties.Add("destination", destination);
            await operationLogger.Log();

            // Send pointer enter event
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = raycastResult.screenPosition
            };
            
            if (gameObject.TryGetEnabledComponent<IPointerEnterHandler>(out var enterHandler))
            {
                enterHandler.OnPointerEnter(pointerEventData);
            }

            // Perform scroll operation
            if (destination.magnitude > 0)
            {
                var remainingDistance = destination.magnitude;
                var direction = destination.normalized;
                
                while (remainingDistance > 0 && !cancellationToken.IsCancellationRequested)
                {
                    var scrollDelta = direction * Mathf.Min(_scrollDistance, remainingDistance);
                    pointerEventData.scrollDelta = scrollDelta;
                    
                    scrollHandler.OnScroll(pointerEventData);
                    
                    remainingDistance -= _scrollDistance;
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }

            // Send pointer exit event
            if (gameObject.TryGetEnabledComponent<IPointerExitHandler>(out var exitHandler))
            {
                exitHandler.OnPointerExit(pointerEventData);
            }
        }
    }
}