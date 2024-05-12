// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Wrapped component that provide interaction for user.
    /// </summary>
    [Obsolete]
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

        private readonly Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> _isReachable;
        private readonly IEnumerable<IOperator> _operators;
        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        private InteractiveComponent(MonoBehaviour component,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable = null,
            IEnumerable<IOperator> operators = null)
        {
            this.component = component;
            _isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            _operators = operators;
        }

        /// <summary>
        /// Create <c>InteractableComponent</c> instance from MonoBehaviour.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Returns new InteractableComponent instance from MonoBehaviour. If MonoBehaviour is not interactable so, return null.</returns>
        public static InteractiveComponent CreateInteractableComponent(MonoBehaviour component,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable = null,
            Func<Component, bool> isComponentInteractable = null,
            IEnumerable<IOperator> operators = null)
        {
            isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            if (isComponentInteractable.Invoke(component))
            {
                return new InteractiveComponent(component, isReachable, operators);
            }

            throw new ArgumentException("Component is not interactable.");
        }

        /// <summary>
        /// Create <c>InteractableComponent</c> instance from GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Returns new InteractableComponent instance from GameObject. If GameObject is not interactable so, return null.</returns>
        [Obsolete("Obsolete due to non-deterministic behavior when GameObject has multiple interactable components.")]
        public static InteractiveComponent CreateInteractableComponent(GameObject gameObject,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable = null,
            Func<Component, bool> isComponentInteractable = null,
            IEnumerable<IOperator> operators = null)
        {
            isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            foreach (var component in gameObject.GetComponents<MonoBehaviour>())
            {
                if (isComponentInteractable.Invoke(component))
                {
                    return new InteractiveComponent(component, isReachable, operators);
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
        [Obsolete("Use GameObjectExtensions.IsReachable() instead")]
        public bool IsReachable()
        {
            return _isReachable(gameObject, _eventData, _results, null);
        }

        /// <summary>
        /// Returns the operators available to this component.
        /// </summary>
        /// <returns>Available operators</returns>
        [Obsolete("Use ComponentExtensions.SelectOperators() instead.")]
        public IEnumerable<IOperator> GetOperators()
        {
            return component.SelectOperators(_operators);
        }

        /// <summary>
        /// Returns the operators that specify types and are available to this component.
        /// </summary>
        /// <returns>Available operators</returns>
        [Obsolete("Use ComponentExtensions.SelectOperators<T>() instead.")]
        public IEnumerable<T> GetOperatorsByType<T>() where T : IOperator
        {
            return component.SelectOperators<T>(_operators);
        }

        /// <summary>
        /// Check component can receive click (tap) event.
        /// </summary>
        [Obsolete]
        public bool CanClick() => GetOperatorsByType<IClickOperator>().Any();

        /// <summary>
        /// Click component.
        /// </summary>
        [Obsolete]
        public void Click() => GetOperatorsByType<IClickOperator>().First().OperateAsync(component);

        [Obsolete]
        public bool CanTap() => CanClick();

        [Obsolete]
        public void Tap() => Click();

        /// <summary>
        /// Check component can receive click (tap) and hold event.
        /// </summary>
        [Obsolete]
        public bool CanClickAndHold() => GetOperatorsByType<IClickAndHoldOperator>().Any();

        /// <summary>
        /// Click (touch) and hold component.
        /// </summary>
        [Obsolete]
        public async UniTask ClickAndHold(CancellationToken cancellationToken = default)
        {
            var clickAndHoldOperator = GetOperatorsByType<IClickAndHoldOperator>().First();
            await clickAndHoldOperator.OperateAsync(component, cancellationToken);
        }

        [Obsolete]
        public bool CanTouchAndHold() => CanClickAndHold();

        [Obsolete]
        public async UniTask TouchAndHold(CancellationToken cancellationToken = default) =>
            await ClickAndHold(cancellationToken);

        /// <summary>
        /// Check component can input text.
        /// </summary>
        [Obsolete]
        public bool CanTextInput() => GetOperatorsByType<ITextInputOperator>().Any();

        /// <summary>
        /// Input random text.
        /// </summary>
        [Obsolete]
        public void TextInput() => GetOperatorsByType<ITextInputOperator>().First().OperateAsync(component);

        /// <summary>
        /// Input specified text.
        /// </summary>
        [Obsolete]
        public void TextInput(string text)
        {
            var textInputOperator = GetOperatorsByType<ITextInputOperator>().First();
            textInputOperator.OperateAsync(component, text);
        }
    }
}
