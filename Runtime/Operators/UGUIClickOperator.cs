// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
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
        private readonly Func<GameObject, Vector2> _getScreenPoint;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="getScreenPoint">The function returns the screen click position. Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        public UGUIClickOperator(Func<GameObject, Vector2> getScreenPoint = null)
        {
            this._getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
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
        public async UniTask OperateAsync(Component component, CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerClickHandler handler))
            {
                throw new ArgumentException("Component must implement IPointerClickHandler.");
            }

            EventSystem.current.SetSelectedGameObject(component.gameObject);

            var position = _getScreenPoint(component.gameObject);
            var eventData = new PointerEventData(EventSystem.current)
            {
                pointerEnter = component.gameObject,
                pointerPress = component.gameObject,
                pointerClick = component.gameObject,
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
