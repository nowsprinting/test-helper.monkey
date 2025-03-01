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
        public bool CanOperate(Component component)
        {
            if (component as EventTrigger)
            {
                return ((EventTrigger)component).triggers.Any(x => x.eventID == EventTriggerType.PointerClick);
            }

            return component.GetType().GetInterfaces().Contains(typeof(IPointerClickHandler));
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(Component component, RaycastResult raycastResult,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerClickHandler handler))
            {
                throw new ArgumentException("Component must implement IPointerClickHandler.");
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

            // Pointer click
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
#if UNITY_2020_3_OR_NEWER
                pointerClick = component.gameObject,
#endif
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
                pointerId = 0, // Touchscreen touches go from 0
#else
                pointerId = -1, // Mouse left button
#endif
                button = PointerEventData.InputButton.Left,
                clickCount = 1,
            };
            handler.OnPointerClick(eventData);
        }
    }
}
