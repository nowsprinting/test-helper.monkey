// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
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
        /// Constructor overload.
        /// </summary>
        /// <param name="config">The configuration for autopilot/tests</param>
        public InteractiveComponentCollector(MonkeyConfig config)
            : this(config.IsReachable, config.IsInteractable, config.Operators)
        {
        }

        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene.
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        ///
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <returns>Interactable components</returns>
        public IEnumerable<Component> FindInteractableComponents()
        {
            foreach (var component in FindMonoBehaviours())
            {
                if (_isInteractable.Invoke(component))
                {
                    yield return component;
                }
            }
        }

        [Obsolete("Use FindInteractableComponents() instead")]
        public static IEnumerable<InteractiveComponent> FindInteractiveComponents()
        {
            throw new NotImplementedException("Use FindInteractableComponents() instead");
        }

        /// <summary>
        /// Find components attached EventTrigger or implements IEventSystemHandler in scene, and reachable from user (pass hit test using raycaster).
        /// Includes UI elements that inherit from the Selectable class, such as Button.
        /// 
        /// Note: If you only need UI elements, using UnityEngine.UI.Selectable.allSelectablesArray is faster.
        /// </summary>
        /// <returns>Reachable and Interactable components</returns>
        public IEnumerable<Component> FindReachableInteractableComponents()
        {
            foreach (var interactableComponent in FindInteractableComponents())
            {
                if (_isReachable.Invoke(interactableComponent.gameObject, _eventData, _results))
                {
                    yield return interactableComponent;
                }
            }
        }

        /// <summary>
        /// Returns tuple of interactable component and operator.
        /// Note: Not check reachable from user.
        /// </summary>
        /// <returns>Tuple of interactable component and operator</returns>
        public IEnumerable<(Component, IOperator)> FindInteractableComponentsAndOperators()
        {
            return FindInteractableComponents().SelectMany(x => x.SelectOperators(_operators), (x, o) => (x, o));
        }

        [Obsolete("Use FindReachableInteractableComponents() instead")]
        public static IEnumerable<InteractiveComponent> FindReallyInteractiveComponents(
            Func<GameObject, Vector2> screenPointStrategy)
        {
            throw new NotImplementedException("Use FindReachableInteractableComponents() instead");
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
