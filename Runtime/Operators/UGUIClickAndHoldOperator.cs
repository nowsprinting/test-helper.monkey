// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
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
        private readonly Func<GameObject, Vector2> _getScreenPoint;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="holdMillis">Hold time in milliseconds</param>
        /// <param name="getScreenPoint">The function returns the screen click position. Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        public UGUIClickAndHoldOperator(int holdMillis = 1000, Func<GameObject, Vector2> getScreenPoint = null)
        {
            this._holdMillis = holdMillis;
            this._getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
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
        public async UniTask OperateAsync(Component component, CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerDownHandler downHandler) || !(component is IPointerUpHandler upHandler))
            {
                throw new ArgumentException("Component must implement IPointerDownHandler and IPointerUpHandler.");
            }

            EventSystem.current.SetSelectedGameObject(component.gameObject);

            var position = _getScreenPoint(component.gameObject);
            var eventData = new PointerEventData(EventSystem.current)
            {
                pointerEnter = component.gameObject,
                pointerPress = component.gameObject,
                position = position,
                pressPosition = position,
                clickCount = 0,
                // Note: Strictly, set rawPointerPress, pointerCurrentRaycast, and pointerPressRaycast to raycastResults[0]
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
            eventData.button = PointerEventData.InputButton.Left;
            upHandler.OnPointerUp(eventData);
        }
    }
}
