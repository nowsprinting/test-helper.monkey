// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Wrapped component that provide interaction for user.
    /// </summary>
    // TODO: Rename to InteractableComponent
    public class InteractiveComponent
    {
        /// <summary>
        /// Inner component (EventTrigger or implements IEventSystemHandler)
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public readonly MonoBehaviour component;

        /// <summary>
        /// Transform via inner component
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public Transform transform => component.transform;

        /// <summary>
        /// GameObject via inner component
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public GameObject gameObject => component.gameObject;

        private readonly Func<GameObject, Vector2> _getScreenPoint;

        private readonly Func<GameObject, Func<GameObject, Vector2>, PointerEventData, List<RaycastResult>, bool>
            _isReachable;

        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        private InteractiveComponent(MonoBehaviour component,
            Func<GameObject, Vector2> getScreenPoint = null,
            Func<GameObject, Func<GameObject, Vector2>, PointerEventData, List<RaycastResult>, bool> isReachable = null)
        {
            this.component = component;
            _getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
        }

        /// <summary>
        /// Create <c>InteractableComponent</c> instance from MonoBehaviour.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="getScreenPoint">The function returns the screen position where raycast for the found <c>GameObject</c>.
        /// Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <returns>Returns new InteractableComponent instance from MonoBehaviour. If MonoBehaviour is not interactable so, return null.</returns>
        public static InteractiveComponent CreateInteractableComponent(MonoBehaviour component,
            Func<GameObject, Vector2> getScreenPoint = null,
            Func<GameObject, Func<GameObject, Vector2>, PointerEventData, List<RaycastResult>, bool> isReachable = null,
            Func<Component, bool> isComponentInteractable = null)
        {
            getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            if (isComponentInteractable.Invoke(component))
            {
                return new InteractiveComponent(component, getScreenPoint, isReachable);
            }

            throw new ArgumentException("Component is not interactable.");
        }

        /// <summary>
        /// Create <c>InteractableComponent</c> instance from GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="getScreenPoint">The function returns the screen position where raycast for the found <c>GameObject</c>.
        /// Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <returns>Returns new InteractableComponent instance from GameObject. If GameObject is not interactable so, return null.</returns>
        [Obsolete("Obsolete due to non-deterministic behavior when GameObject has multiple interactable components.")]
        public static InteractiveComponent CreateInteractableComponent(GameObject gameObject,
            Func<GameObject, Vector2> getScreenPoint = null,
            Func<GameObject, Func<GameObject, Vector2>, PointerEventData, List<RaycastResult>, bool> isReachable = null,
            Func<Component, bool> isComponentInteractable = null)
        {
            getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            foreach (var component in gameObject.GetComponents<MonoBehaviour>())
            {
                if (isComponentInteractable.Invoke(component))
                {
                    return new InteractiveComponent(component, getScreenPoint, isReachable);
                }
            }

            throw new ArgumentException("GameObject has not interactable component.");
        }

        /// <summary>
        /// Hit test using raycaster
        /// </summary>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        /// <param name="eventData">Specify if avoid GC memory allocation</param>
        /// <param name="results">Specify if avoid GC memory allocation</param>
        /// <returns>true: this object can control by user</returns>
        [Obsolete("Use IsReachable() instead")]
        public bool IsReallyInteractiveFromUser(Func<GameObject, Vector2> screenPointStrategy,
            PointerEventData eventData = null, List<RaycastResult> results = null)
        {
            return gameObject.IsReachable(screenPointStrategy, eventData, results);
        }

        /// <summary>
        /// Hit test using raycaster
        /// </summary>
        /// <returns>true: this object can control by user</returns>
        public bool IsReachable()
        {
            return _isReachable.Invoke(gameObject, _getScreenPoint, _eventData, _results);
        }

        /// <summary>
        /// Returns only available operators myself.
        /// </summary>
        /// <param name="operators"></param>
        /// <returns>Available operators myself</returns>
        public IEnumerable<IOperator> FilterAvailableOperators(IEnumerable<IOperator> operators)
        {
            return operators.Where(iOperator => iOperator.IsMatch(component));
        }
    }
}
