// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.DefaultStrategies;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find <c>InteractableComponent</c>s in the scene.
    /// </summary>
    // TODO: Rename to InteractableComponentsFinder
    public static class InteractiveComponentCollector
    {
        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene.
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        ///
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <param name="isInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <returns>Interactive components</returns>
        // TODO: Change to instance method
        public static IEnumerable<InteractiveComponent> FindInteractableComponents(
            Func<Component, bool> isInteractable = null)
        {
            isInteractable = isInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            foreach (var component in FindMonoBehaviours())
            {
                if (isInteractable(component))
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
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene, and reachable from user (pass hit test using raycaster).
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        /// 
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        /// <returns>Really interactive components</returns>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        // TODO: Change to instance method
        public static IEnumerable<InteractiveComponent> FindReachableInteractableComponents(
            Func<GameObject, Vector2> screenPointStrategy,
            Func<GameObject, PointerEventData, List<RaycastResult>, bool> isReachable = null)
        {
            isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            var data = new PointerEventData(EventSystem.current);
            var results = new List<RaycastResult>();

            foreach (var interactiveComponent in FindInteractableComponents())
            {
                data.position = screenPointStrategy(interactiveComponent.gameObject);
                if (isReachable(interactiveComponent.gameObject, data, results))
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
