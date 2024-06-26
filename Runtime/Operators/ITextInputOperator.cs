// Copyright (c) 2023-2024 Koji Hasegawa.
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
        /// <param name="component">Target component</param>
        /// <param name="text">text to input</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask OperateAsync(Component component, string text, CancellationToken cancellationToken = default);
    }
}
