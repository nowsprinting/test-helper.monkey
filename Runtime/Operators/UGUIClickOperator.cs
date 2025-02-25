// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Click (tap) operator for Unity UI (uGUI) components.
    /// </summary>
    public class UGUIClickOperator : IClickOperator
    {
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
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerClickHandler handler))
            {
                throw new ArgumentException("Component must implement IPointerClickHandler.");
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
