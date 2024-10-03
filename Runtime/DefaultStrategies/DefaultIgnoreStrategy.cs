// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default strategy to examine whether GameObject should be ignored.
    /// </summary>
    public static class DefaultIgnoreStrategy
    {
        /// <summary>
        /// Ensure the <c>GameObject</c> is ignored or not.
        /// Default implementation is to check whether the GameObject has <c>IgnoreAnnotation</c> component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>True if GameObject is ignored</returns>
        public static bool IsIgnored(GameObject gameObject)
        {
            return gameObject.TryGetComponent(typeof(IgnoreAnnotation), out _);
        }
    }
}
