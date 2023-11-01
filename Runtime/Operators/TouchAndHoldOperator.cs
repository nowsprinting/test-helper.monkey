// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    public class TouchAndHoldOperator : IOperator
    {
        private readonly int _holdMillis;

        public TouchAndHoldOperator(int holdMillis = 1000)
        {
            _holdMillis = holdMillis;
        }

        public async UniTask DoOperation(MonoBehaviour component, Func<GameObject, Vector2> screenPointStrategy,
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerDownHandler downHandler) || !(component is IPointerUpHandler upHandler))
            {
                return;
            }

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPointStrategy(component.gameObject)
            };

            downHandler.OnPointerDown(eventData);
            await UniTask.Delay(TimeSpan.FromMilliseconds(_holdMillis), cancellationToken: cancellationToken);

            if (component == null)
            {
                return;
            }

            upHandler.OnPointerUp(eventData);
        }
    }
}
