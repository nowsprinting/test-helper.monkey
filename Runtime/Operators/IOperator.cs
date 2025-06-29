// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

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
        /// Returns if can operate target <c>GameObject</c> this Operator.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <returns>True if can operate <c>GameObject</c> this Operator.</returns>
        bool CanOperate(GameObject gameObject);

        /// <summary>
        /// Execute this operator in monkey testing.
        /// </summary>
        /// <remarks>
        /// If required parameters for the operation, such as hold time, input text strategy, etc., keep them in instance fields of the implementation class.
        /// If you want to add parameters for execution outside of monkey tests, define a method in the sub-interface (e.g., <c>ITextInputOperator</c>).
        /// </remarks>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="logger">Logger set if you need</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default);
    }
}
