// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using TestHelper.Monkey.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    internal static class ClickOperator
    {
        internal static bool CanClick(MonoBehaviour component)
        {
            if (component.gameObject.TryGetComponent(typeof(IgnoreAnnotation), out _))
            {
                return false;
            }

            if (component as EventTrigger)
            {
                return ((EventTrigger)component).triggers.Any(x => x.eventID == EventTriggerType.PointerClick);
            }

            return component.GetType().GetInterfaces().Contains(typeof(IPointerClickHandler));
        }

        internal static void Click(MonoBehaviour component, Func<GameObject, Vector2> screenPointStrategy)
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
        }
    }
}
