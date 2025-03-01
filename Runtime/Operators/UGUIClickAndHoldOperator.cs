// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Operators.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Click and hold operator for Unity UI (uGUI) components.
    /// a.k.a. touch and hold, long press.
    ///
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
        public bool CanOperate(Component component)
        {
            if (component as EventTrigger)
            {
                return ((EventTrigger)component).triggers.Any(x => x.eventID == EventTriggerType.PointerDown) &&
                       ((EventTrigger)component).triggers.Any(x => x.eventID == EventTriggerType.PointerUp);
            }

            var interfaces = component.GetType().GetInterfaces();
            return interfaces.Contains(typeof(IPointerDownHandler)) && interfaces.Contains(typeof(IPointerUpHandler));
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(Component component, RaycastResult raycastResult,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerDownHandler downHandler) || !(component is IPointerUpHandler upHandler))
            {
                throw new ArgumentException("Component must implement IPointerDownHandler and IPointerUpHandler.");
            }

            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(component, this, logger ?? _logger,
                screenshotOptions ?? _screenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            await operationLogger.Log();

            // Selected before operation
            if (component is Selectable)
            {
                EventSystem.current.SetSelectedGameObject(component.gameObject);
            }

            // Pointer down
            var eventData = new PointerEventData(EventSystem.current)
            {
                pointerCurrentRaycast = raycastResult,
                pointerPressRaycast = raycastResult,
                rawPointerPress = raycastResult.gameObject,
#if UNITY_2022_3_OR_NEWER
                displayIndex = raycastResult.displayIndex,
#endif
                position = raycastResult.screenPosition,
                pressPosition = raycastResult.screenPosition,
                pointerPress = component.gameObject,
                // Note: pointerClick is not set here because it is not yet clicked
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
                pointerId = 0, // Touchscreen touches go from 0
#else
                pointerId = -1, // Mouse left button
#endif
                button = PointerEventData.InputButton.Left,
                clickCount = 0, // Note: not yet clicked
            };
            downHandler.OnPointerDown(eventData);

            await UniTask.Delay(TimeSpan.FromMilliseconds(_holdMillis), ignoreTimeScale: true,
                cancellationToken: cancellationToken);

            if (component == null || CanOperate(component) == false)
            {
                return;
            }

            // Pointer up
#if UNITY_2020_3_OR_NEWER
            eventData.pointerClick = component.gameObject;
#endif
            eventData.clickCount = 1;
            upHandler.OnPointerUp(eventData);
        }
    }
}
