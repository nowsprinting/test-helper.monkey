// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using TestHelper.Monkey.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Matchers
{
    public class ClickableComponentMatcher : IComponentMatcher
    {
        public bool IsMatch(MonoBehaviour component)
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
    }
}
