// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Operators;
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
        private readonly Func<GameObject, PointerEventData, List<RaycastResult>, bool> _isReachable;
        private readonly Func<Component, bool> _isInteractable;
        private readonly IEnumerable<IOperator> _operators;
        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="isInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        public InteractiveComponentCollector(
            Func<GameObject, PointerEventData, List<RaycastResult>, bool> isReachable = null,
            Func<Component, bool> isInteractable = null,
            IEnumerable<IOperator> operators = null)
        {
            _isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            _isInteractable = isInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
            _operators = operators;
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
                if (_isInteractable.Invoke(component))
                {
                    yield return InteractiveComponent.CreateInteractableComponent(component,
                        _isReachable,
                        _isInteractable,
                        _operators);
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
            var instance = new InteractiveComponentCollector();
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
