// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find interactable components on the scene.
    /// </summary>
    public class InteractableComponentsFinder
    {
        private readonly Func<Component, bool> _isInteractable;
        private readonly IEnumerable<IOperator> _operators;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        public InteractableComponentsFinder(
            Func<Component, bool> isInteractable = null,
            IEnumerable<IOperator> operators = null)
        {
            _isInteractable = isInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
            _operators = operators;
        }

        /// <summary>
        /// Constructor overload.
        /// </summary>
        /// <param name="config">The configuration for autopilot/tests</param>
        public InteractableComponentsFinder(MonkeyConfig config)
            : this(config.IsInteractable, config.Operators)
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

        /// <summary>
        /// Returns tuple of interactable component and operator.
        /// Note: Not check reachable from user.
        /// </summary>
        /// <returns>Tuple of interactable component and operator</returns>
        public IEnumerable<(Component, IOperator)> FindInteractableComponentsAndOperators()
        {
            return FindInteractableComponents().SelectMany(x => x.SelectOperators(_operators), (x, o) => (x, o));
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
