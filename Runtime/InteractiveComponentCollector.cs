// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
        /// <remarks>
        /// `reallyInteractiveOnly` option does not give correct results about UI elements when run on batchmode.
        /// Because GraphicRaycaster does not work in batchmode.
        /// </remarks>
        /// <param name="reallyInteractiveOnly">With hit test using Raycaster. Recommended to set it to false if there are many objects.</param>
        /// <returns>Interactive components</returns>
        public static IEnumerable<InteractiveComponent> FindInteractiveComponents(bool reallyInteractiveOnly = true)
        {
            PointerEventData data = null;
            List<RaycastResult> results = null;
            if (reallyInteractiveOnly)
            {
                data = new PointerEventData(EventSystem.current);
                results = new List<RaycastResult>();
            }

            foreach (var component in FindMonoBehaviours())
            {
                if (component.GetType() == typeof(EventTrigger) ||
                    component.GetType().GetInterfaces().Contains(typeof(IEventSystemHandler)))
                {
                    var interactiveComponent = new InteractiveComponent(component);
                    if (!reallyInteractiveOnly || interactiveComponent.IsReallyInteractiveFromUser(data, results))
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
