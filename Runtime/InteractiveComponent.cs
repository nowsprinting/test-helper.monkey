// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.Random;
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

        internal InteractiveComponent(MonoBehaviour component,
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

            Debug.LogWarning($"Component `{component}` is not interactable.");
            return null;
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

            Debug.LogWarning($"GameObject `{gameObject}` has not interactable component.");
            return null;
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
        /// Check inner component can receive click event
        /// </summary>
        /// <returns>true: Can click</returns>
        public bool CanClick() => ClickOperator.CanClick(component);

        /// <summary>
        /// Click inner component
        /// </summary>
        public void Click() => ClickOperator.Click(component, _getScreenPoint);

        /// <summary>
        /// Check inner component can receive tap (click) event
        /// </summary>
        /// <returns>true: Can tap</returns>
        public bool CanTap() => ClickOperator.CanClick(component);

        /// <summary>
        /// Tap (click) inner component
        /// </summary>
        public void Tap() => ClickOperator.Click(component, _getScreenPoint);

        /// <summary>
        /// Check inner component can receive touch-and-hold event
        /// </summary>
        /// <returns>true: Can touch-and-hold</returns>
        public bool CanTouchAndHold() => TouchAndHoldOperator.CanTouchAndHold(component);

        /// <summary>
        /// Touch-and-hold inner component
        /// </summary>
        /// <param name="delayMillis">Delay time between down to up</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        public async UniTask TouchAndHold(int delayMillis = 1000, CancellationToken cancellationToken = default)
            => await TouchAndHoldOperator.TouchAndHold(component, _getScreenPoint, delayMillis, cancellationToken);

        /// <summary>
        /// Check inner component can input text
        /// </summary>
        /// <returns>true: Can click</returns>
        public bool CanTextInput() => TextInputOperator.CanTextInput(component);

        /// <summary>
        /// Input random text that is randomly generated by <paramref name="randomStringParams"/>
        /// </summary>
        /// <param name="randomStringParams">Random string generation parameters</param>
        /// <param name="randomString">Random string generator</param>
        public void TextInput(Func<GameObject, RandomStringParameters> randomStringParams,
            IRandomString randomString) =>
            TextInputOperator.Input(component, randomStringParams, randomString);

        // TODO: drag, swipe, flick, etc...
    }
}
