// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default strategy to examine whether GameObject is interactable.
    /// </summary>
    public static class DefaultGameObjectInteractableStrategy
    {
        /// <summary>
        /// Make sure the <c>GameObject</c> is interactable.
        /// If any of the following is true:
        /// 1. Attached <c>Selectable</c> component and <c>interactable</c> property is true.
        /// 2. Attached <c>EventTrigger</c> component.
        /// 3. Attached component  implements <c>IEventSystemHandler</c> interface.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentInteractableStrategy"></param>
        /// <returns>True if this GameObject is interactable</returns>
        public static bool IsInteractable(GameObject gameObject, Func<Component, bool> componentInteractableStrategy)
        {
            return gameObject.GetComponents<Component>().Any(componentInteractableStrategy);
        }
    }
}
