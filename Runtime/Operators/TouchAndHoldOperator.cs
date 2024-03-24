// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    internal static class TouchAndHoldOperator
    {
        internal static bool CanTouchAndHold(MonoBehaviour component)
        {
            if (component.gameObject.TryGetComponent(typeof(IgnoreAnnotation), out _))
            {
                return false;
            }

            if (component as EventTrigger)
            {
                return ((EventTrigger)component).triggers.Any(x => x.eventID == EventTriggerType.PointerDown) &&
                       ((EventTrigger)component).triggers.Any(x => x.eventID == EventTriggerType.PointerUp);
            }

            var interfaces = component.GetType().GetInterfaces();
            return interfaces.Contains(typeof(IPointerDownHandler)) && interfaces.Contains(typeof(IPointerUpHandler));
        }

        internal static async UniTask TouchAndHold(
            MonoBehaviour component,
            Func<GameObject, Vector2> screenPointStrategy = null,
            int delayMillis = 1000,
            CancellationToken cancellationToken = default
        )
        {
            if (!(component is IPointerDownHandler downHandler) || !(component is IPointerUpHandler upHandler))
            {
                return;
            }

            screenPointStrategy = screenPointStrategy ?? DefaultScreenPointStrategy.GetScreenPoint;
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPointStrategy(component.gameObject)
            };

            downHandler.OnPointerDown(eventData);
            await UniTask.Delay(TimeSpan.FromMilliseconds(delayMillis), cancellationToken: cancellationToken);

            if (component == null)
            {
                return;
            }

            upHandler.OnPointerUp(eventData);
        }
    }
}
