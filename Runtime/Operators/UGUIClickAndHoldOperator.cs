// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Click and hold operator for Unity UI (uGUI) components.
    /// a.k.a. touch and hold, long press.
    /// </summary>
    public class UGUIClickAndHoldOperator : IClickAndHoldOperator
    {
        private readonly int _holdMillis;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="holdMillis">Hold time in milliseconds</param>
        public UGUIClickAndHoldOperator(int holdMillis = 1000)
        {
            this._holdMillis = holdMillis;
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
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerDownHandler downHandler) || !(component is IPointerUpHandler upHandler))
            {
                throw new ArgumentException("Component must implement IPointerDownHandler and IPointerUpHandler.");
            }

            EventSystem.current.SetSelectedGameObject(component.gameObject);

            var eventData = new PointerEventData(EventSystem.current)
            {
                pointerCurrentRaycast = raycastResult,
                pointerPressRaycast = raycastResult,
                rawPointerPress = raycastResult.gameObject,
                displayIndex = raycastResult.displayIndex,
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

#if UNITY_2020_3_OR_NEWER
            eventData.pointerClick = component.gameObject;
#endif
            eventData.clickCount = 1;
            upHandler.OnPointerUp(eventData);
        }
    }
}
