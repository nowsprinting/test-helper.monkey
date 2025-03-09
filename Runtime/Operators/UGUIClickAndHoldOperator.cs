// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Click and hold operator for Unity UI (uGUI) components.
    /// a.k.a. touch and hold, long press.
    /// <p/>
    /// This operator receives <c>RaycastResult</c>, but passing <c>default</c> may be OK, depending on the component being operated on.
    /// </summary>
    public class UGUIClickAndHoldOperator : IClickAndHoldOperator
    {
        private readonly int _holdMillis;

        private readonly ScreenshotOptions _screenshotOptions;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="holdMillis">Hold time in milliseconds</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UGUIClickAndHoldOperator(int holdMillis = 1000,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            _holdMillis = holdMillis;
            _screenshotOptions = screenshotOptions;
            _logger = logger ?? Debug.unityLogger;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject.TryGetEnabledComponent<EventTrigger>(out var eventTrigger))
            {
                return eventTrigger.triggers.Any(x => x.eventID == EventTriggerType.PointerDown) &&
                       eventTrigger.triggers.Any(x => x.eventID == EventTriggerType.PointerUp);
            }

            return gameObject.TryGetEnabledComponent<IPointerDownHandler>(out _) &&
                   gameObject.TryGetEnabledComponent<IPointerUpHandler>(out _);
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            logger = logger ?? _logger;
            screenshotOptions = screenshotOptions ?? _screenshotOptions;

            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(gameObject, this, logger, screenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            await operationLogger.Log();

            // Do operation
            using (var pointerClickSimulator = new PointerEventSimulator(gameObject, raycastResult, logger))
            {
                await pointerClickSimulator.PointerClickAsync(_holdMillis, cancellationToken);
            }
        }
    }
}
