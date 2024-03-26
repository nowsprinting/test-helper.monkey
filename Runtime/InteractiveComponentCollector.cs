// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find <c>InteractableComponent</c>s in the scene.
    /// </summary>
    // TODO: Rename to InteractableComponentsFinder
    public class InteractiveComponentCollector
    {
        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly Func<Component, bool> _isComponentInteractable;
        private readonly Func<GameObject, PointerEventData, List<RaycastResult>, bool> _isReachable;
        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="getScreenPoint">The function returns the screen position where raycast for the found <c>GameObject</c>.
        /// Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        public InteractiveComponentCollector(
            Func<GameObject, Vector2> getScreenPoint = null,
            Func<Component, bool> isComponentInteractable = null,
            Func<GameObject, PointerEventData, List<RaycastResult>, bool> isReachable = null)
        {
            _getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
            _isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
        }

        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene.
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        ///
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <returns>Interactive components</returns>
        public IEnumerable<InteractiveComponent> FindInteractableComponents()
        {
            foreach (var component in FindMonoBehaviours())
            {
                if (_isComponentInteractable.Invoke(component))
                {
                    yield return new InteractiveComponent(component,
                        _getScreenPoint,
                        _isReachable);
                }
            }
        }

        [Obsolete("Use FindInteractableComponents() instead")]
        public static IEnumerable<InteractiveComponent> FindInteractiveComponents()
        {
            var instance = new InteractiveComponentCollector();
            return instance.FindInteractableComponents();
        }

        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene, and reachable from user (pass hit test using raycaster).
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        /// 
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <returns>Really interactive components</returns>
        public IEnumerable<InteractiveComponent> FindReachableInteractableComponents()
        {
            foreach (var interactiveComponent in FindInteractableComponents())
            {
                _eventData.position = _getScreenPoint.Invoke(interactiveComponent.gameObject);
                if (_isReachable.Invoke(interactiveComponent.gameObject, _eventData, _results))
                {
                    yield return interactiveComponent;
                }
            }
        }

        [Obsolete("Use FindReachableInteractableComponents() instead")]
        public static IEnumerable<InteractiveComponent> FindReallyInteractiveComponents(
            Func<GameObject, Vector2> screenPointStrategy)
        {
            var instance = new InteractiveComponentCollector(getScreenPoint: screenPointStrategy);
            return instance.FindReachableInteractableComponents();
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
