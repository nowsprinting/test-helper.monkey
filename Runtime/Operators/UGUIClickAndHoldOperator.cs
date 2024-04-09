// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Click and hold operator for Unity UI (uGUI) components.
    /// a.k.a. touch and hold, long press.
    /// </summary>
    public class UGUIClickAndHoldOperator : IOperator
    {
        private readonly int _holdMillis;
        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);

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
        public OperatorType Type => OperatorType.ClickAndHold;

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

            _eventData.position = _getScreenPoint(component.gameObject);
            downHandler.OnPointerDown(_eventData);
            await UniTask.Delay(TimeSpan.FromMilliseconds(_holdMillis), cancellationToken: cancellationToken);

            if (component == null || component.gameObject == null)
            {
                return;
            }

            upHandler.OnPointerUp(_eventData);
        }
    }
}
