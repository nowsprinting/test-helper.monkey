// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.Monkey.Operators;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Extensions
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Returns the operators available to this component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Available operators</returns>
        public static IEnumerable<IOperator> SelectOperators(this Component component,
            IEnumerable<IOperator> operators)
        {
            return operators.Where(iOperator => iOperator.CanOperate(component));
        }

        /// <summary>
        /// Returns the operators that specify types and are available to this component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <param name="type">Operator type</param>
        /// <returns>Available operators</returns>
        public static IEnumerable<IOperator> SelectOperatorsOfType(this Component component,
            IEnumerable<IOperator> operators, OperatorType type)
        {
            return operators.Where(iOperator => iOperator.Type == type && iOperator.CanOperate(component));
        }

        /// <summary>
        /// Make sure the <c>Component</c> is interactable.
        /// If any of the following is true:
        /// 1. Type is <c>Selectable</c> and <c>interactable</c> property is true.
        /// 2. Type is <c>EventTrigger</c> component.
        /// 3. Implements <c>IEventSystemHandler</c> interface.
        /// </summary>
        /// <param name="component"></param>
        /// <returns>True if this Component is interactable</returns>
        [Obsolete("Use DefaultComponentInteractableStrategy instead.")]
        public static bool IsInteractable(this Component component)
        {
            // UI element
            var selectable = component as Selectable;
            if (selectable != null)
            {
                return selectable.interactable;
            }

            // 2D/3D object
            return component.GetType() == typeof(EventTrigger) ||
                   component.GetType().GetInterfaces().Contains(typeof(IEventSystemHandler));
        }
    }
}
