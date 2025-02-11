// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.ScreenPointStrategies;
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
        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);

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

            _eventData.position = _getScreenPoint(component.gameObject);
            handler.OnPointerClick(_eventData);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return nameof(UGUIClickOperator);
        }
    }
}
