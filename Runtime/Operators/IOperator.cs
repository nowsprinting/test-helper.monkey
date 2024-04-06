// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Matcher and Operator pair for monkey testing.
    /// Implement the <c>IsMatch</c> method to determine whether an operation such as click is possible, and the <c>Operate</c> method to execute the operation.
    /// Please keep the required for the operation, such as the input text strategy, in the instance field of the implementation class.
    /// </summary>
    public interface IOperator
    {
        /// <summary>
        /// Returns if can operate target component this Operator.
        /// </summary>
        /// <param name="component">Target component</param>
        /// <returns>True if can operate component this Operator.</returns>
        bool IsMatch(Component component);

        /// <summary>
        /// Do operate.
        /// </summary>
        /// <param name="component">Target component</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask Operate(Component component, CancellationToken cancellationToken = default);
    }
}
