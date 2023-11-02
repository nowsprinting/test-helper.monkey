// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Collect <c>InteractiveComponent</c>s in the scene.
    /// </summary>
    public static class InteractiveComponentCollector
    {
        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene.
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        ///
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <returns>Interactive components</returns>
        public static IEnumerable<InteractiveComponent> FindInteractiveComponents()
        {
            foreach (var component in FindMonoBehaviours())
            {
                if (component.GetType() == typeof(EventTrigger) ||
                    component.GetType().GetInterfaces().Contains(typeof(IEventSystemHandler)))
                {
                    var interactiveComponent = component.gameObject.GetComponent<InteractiveComponent>();
                    if (interactiveComponent == null)
                    {
                        interactiveComponent = component.gameObject.AddComponent<InteractiveComponent>();
                    }

                    yield return interactiveComponent;
                }
            }
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
        public static IEnumerable<InteractiveComponent> FindReallyInteractiveComponents(
            Func<GameObject, Vector2> screenPointStrategy
        )
        {
            var data = new PointerEventData(EventSystem.current);
            var results = new List<RaycastResult>();

            foreach (var component in FindMonoBehaviours())
            {
                if (component.GetType() == typeof(EventTrigger) ||
                    component.GetType().GetInterfaces().Contains(typeof(IEventSystemHandler)))
                {
                    var interactiveComponent = component.gameObject.GetComponent<InteractiveComponent>();
                    if (interactiveComponent == null)
                    {
                        interactiveComponent = component.gameObject.AddComponent<InteractiveComponent>();
                    }

                    if (interactiveComponent.IsReallyInteractiveFromUser(screenPointStrategy, data, results))
                    {
                        yield return interactiveComponent;
                    }
                }
            }
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
