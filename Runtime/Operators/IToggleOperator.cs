// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Interface for the toggle UI component operator.
    /// You can click to turn it on/off, or you can specify the on/off state.
    /// </summary>
    /// <remarks>
    /// In monkey testing, this operator behavior is identical to that of <see cref="IClickOperator"/>, so its use is not recommended.
    /// </remarks>
    public interface IToggleOperator : IClickOperator
    {
        /// <summary>
        /// Set the toggle state.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="isOn">Set the specify on/off to the toggle state</param>
        /// <param name="logger">Logger set if you need</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult, bool isOn, ILogger logger = null,
            ScreenshotOptions screenshotOptions = null, CancellationToken cancellationToken = default);
    }
}
