// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Matcher and Operator pair for monkey testing.
    /// Implement the <c>CanOperate</c> method to determine whether an operation such as click is possible, and the <c>OperateAsync</c> method to execute the operation.
    /// </summary>
    /// <remarks>
    /// Must be implements sub-interface (e.g., <c>IClickOperator</c>) to represent the type of operator.
    /// If required parameters for the operation, such as hold time, input text strategy, etc., keep them in instance fields of the implementation class.
    /// </remarks>
    public interface IOperator
    {
        /// <summary>
        /// Returns if can operate target component this Operator.
        /// </summary>
        /// <param name="component">Target component</param>
        /// <returns>True if can operate component this Operator.</returns>
        bool CanOperate(Component component);

        /// <summary>
        /// Execute this operator in monkey testing.
        /// </summary>
        /// <remarks>
        /// If required parameters for the operation, such as hold time, input text strategy, etc., keep them in instance fields of the implementation class.
        /// If you want to add parameters for execution outside of monkey tests, define a sub-interface (e.g., <c>ITextInputOperator</c>).
        /// </remarks>
        /// <param name="component">Target component</param>
        /// <param name="position">Screen position of starting operation. Usually, this is the position through which the raycast passes.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask OperateAsync(Component component, Vector2 position, CancellationToken cancellationToken = default);
    }
}
