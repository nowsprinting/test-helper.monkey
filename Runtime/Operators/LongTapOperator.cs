// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    internal static class LongTapOperator
    {
        internal static bool CanLongTap(MonoBehaviour component)
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

        internal static async Task LongTap(MonoBehaviour component, int delayMillis = 1000,
            CancellationToken cancellationToken = default)
        {
            if (!(component is IPointerDownHandler downHandler) || !(component is IPointerUpHandler upHandler))
            {
                return;
            }

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = component.gameObject.GetScreenPoint()
            };

            downHandler.OnPointerDown(eventData);
            await Task.Delay(delayMillis, cancellationToken);
            upHandler.OnPointerUp(eventData);
        }
    }
}
