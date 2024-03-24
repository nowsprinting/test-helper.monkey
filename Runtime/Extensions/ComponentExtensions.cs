// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Extensions
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Make sure the <c>Component</c> is interactable.
        /// If any of the following is true:
        /// 1. Type is <c>Selectable</c> and <c>interactable</c> property is true.
        /// 2. Type is <c>EventTrigger</c> component.
        /// 3. Implements <c>IEventSystemHandler</c> interface.
        /// </summary>
        /// <param name="component"></param>
        /// <returns>True if this GameObject is interactable</returns>
        public static bool IsInteractable(this Component component)
        {
            // UI element
            var selectable = component as Selectable;
            if (selectable != null)
            {
                return selectable.interactable;
            }

            // 2D/3D object
            return component.GetType() == typeof(EventTrigger) ||
                   component.GetType().GetInterfaces().Contains(typeof(IEventSystemHandler));
        }
    }
}
