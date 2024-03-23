// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Collect <c>InteractiveComponent</c>s in the scene.
    /// </summary>
    public static class InteractiveComponentCollector // TODO: Rename to InteractableComponentFinder
    {
        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene.
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        ///
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <returns>Interactive components</returns>
        public static IEnumerable<InteractiveComponent> FindInteractableComponents()
        {
            foreach (var component in FindMonoBehaviours())
            {
                if (component.IsInteractable())
                {
                    yield return new InteractiveComponent(component);
                }
            }
        }

        [Obsolete("Use FindInteractableComponents() instead")]
        public static IEnumerable<InteractiveComponent> FindInteractiveComponents()
        {
            return FindInteractableComponents();
        }

        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene.
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        ///
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <remarks>
        /// This method does not give correct results about UI elements when run on batchmode.
        /// Because GraphicRaycaster does not work in batchmode.
        /// </remarks>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        /// <returns>Really interactive components</returns>
        public static IEnumerable<InteractiveComponent> FindReachableInteractableComponents(
            Func<GameObject, Vector2> screenPointStrategy)
        {
            var data = new PointerEventData(EventSystem.current);
            var results = new List<RaycastResult>();

            foreach (var interactiveComponent in FindInteractableComponents())
            {
                if (interactiveComponent.gameObject.IsReachable(screenPointStrategy, data, results))
                {
                    yield return interactiveComponent;
                }
            }
        }

        [Obsolete("Use FindReachableInteractableComponents() instead")]
        public static IEnumerable<InteractiveComponent> FindReallyInteractiveComponents(
            Func<GameObject, Vector2> screenPointStrategy)
        {
            return FindReachableInteractableComponents(screenPointStrategy);
        }

        private static IEnumerable<MonoBehaviour> FindMonoBehaviours()
        {
#if UNITY_2022_3_OR_NEWER
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return Object.FindObjectsOfType<MonoBehaviour>();
#endif
        }
    }
}
