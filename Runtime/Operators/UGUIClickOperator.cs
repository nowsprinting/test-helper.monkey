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
        public async UniTask OperateAsync(Component component, Vector2 position,
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerClickHandler handler))
            {
                throw new ArgumentException("Component must implement IPointerClickHandler.");
            }

            EventSystem.current.SetSelectedGameObject(component.gameObject);

            var eventData = new PointerEventData(EventSystem.current)
            {
                pointerEnter = component.gameObject,
                pointerPress = component.gameObject,
#if UNITY_2020_3_OR_NEWER
                pointerClick = component.gameObject,
#endif
                position = position,
                pressPosition = position,
                clickCount = 1,
                button = PointerEventData.InputButton.Left,
                // Note: Strictly, set rawPointerPress, pointerCurrentRaycast, and pointerPressRaycast to RaycastResults[0]
            };
            handler.OnPointerClick(eventData);
        }
    }
}
