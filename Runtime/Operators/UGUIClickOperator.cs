// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Click (tap) operator for Unity UI (uGUI) components.
    /// This operator receives <c>RaycastResult</c>, but passing <c>default</c> may be OK, depending on the component being operated on.
    /// </summary>
    public class UGUIClickOperator : IClickOperator
    {
        private readonly ScreenshotOptions _screenshotOptions;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UGUIClickOperator(ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            _screenshotOptions = screenshotOptions;
            _logger = logger ?? Debug.unityLogger;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject.TryGetEnabledComponent<EventTrigger>(out var eventTrigger))
            {
                return eventTrigger.triggers.Any(x => x.eventID == EventTriggerType.PointerClick);
            }

            return gameObject.TryGetEnabledComponent<IPointerClickHandler>(out _);
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
                await pointerClickSimulator.PointerClickAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
