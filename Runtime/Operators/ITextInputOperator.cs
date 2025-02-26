// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Matcher and Operator pair for text input component.
    /// Added specify input string method for scenario testing.
    /// </summary>
    public interface ITextInputOperator : IOperator
    {
        /// <summary>
        /// Text input with specified text.
        /// </summary>
        /// <param name="component">Operation target component</param>
        /// <param name="text">Text to input</param>
        /// <param name="logger">Logger set if you need</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask OperateAsync(Component component, string text,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default);
    }
}
