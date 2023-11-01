// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    public class ClickOperator : IOperator
    {
        public async UniTask DoOperation(MonoBehaviour component, Func<GameObject, Vector2> screenPointStrategy,
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerClickHandler handler))
            {
                return;
            }

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPointStrategy(component.gameObject)
            };

            handler.OnPointerClick(eventData);
            await Task.Yield();
        }
    }
}
