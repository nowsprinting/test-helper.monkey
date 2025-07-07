// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Toggle operator for <see cref="Toggle"/> component.
    /// You can click to turn it on/off, or you can specify the on/off state.
    /// </summary>
    public class UguiToggleOperator : UGUIClickOperator, IToggleOperator
    {
        /// <inheritdoc />
        /// <remarks>The <c>new</c> keyword is specified because we want it to work with the casted type.</remarks>
        public new bool CanOperate(GameObject gameObject)
        {
            return gameObject.TryGetEnabledComponent<Toggle>(out _);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Does not click if the toggle state is already specified.
        /// <p/>
        /// This operator receives <c>RaycastResult</c>, but passing <c>default</c> may be OK, depending on the component being operated on.
        /// </remarks>
        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult, bool isOn,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            var toggle = gameObject.GetComponent<Toggle>();
            if (toggle.isOn == isOn)
            {
                return UniTask.CompletedTask;
            }

            return base.OperateAsync(gameObject, raycastResult, logger, screenshotOptions, cancellationToken);
        }
    }
}
