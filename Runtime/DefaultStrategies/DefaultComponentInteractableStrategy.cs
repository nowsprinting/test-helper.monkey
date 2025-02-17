// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default strategy to examine whether Component is interactable.
    /// </summary>
    public static class DefaultComponentInteractableStrategy
    {
        /// <summary>
        /// Make sure the <c>Component</c> is interactable.
        ///
        /// If any of the following is true:
        /// <list type="number">
        ///   <item>Type is <c>Selectable</c> and <c>interactable</c> property is true</item>
        ///   <item>Type is <c>EventTrigger</c> component</item>
        ///   <item>Implements <c>IEventSystemHandler</c> interface</item>
        /// </list>
        /// </summary>
        /// <param name="component"></param>
        /// <returns>True if this Component is interactable</returns>
        public static bool IsInteractable(Component component)
        {
            if (component == null || (component is Behaviour behaviour && !behaviour.isActiveAndEnabled))
            {
                return false;
            }

            // UI element
            var selectable = component as Selectable;
            if (selectable)
            {
                return selectable.interactable;
            }

            // 2D/3D object
            return component.GetType() == typeof(EventTrigger) ||
                   component.GetType().GetInterfaces().Contains(typeof(IEventSystemHandler));
        }
    }
}
