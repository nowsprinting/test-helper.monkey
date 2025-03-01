// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Strategy to examine whether <c>GameObject</c> should be ignored.
    /// You should implement this when you want to ignore specific objects (e.g., by name and/or path) in your game title.
    /// </summary>
    public interface IIgnoreStrategy
    {
        /// <summary>
        /// Returns whether the <c>GameObject</c> is ignored or not.
        /// </summary>
        /// <param name="gameObject">Target <c>GameObject</c></param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <returns>True if <c>GameObject</c> is ignored</returns>
        bool IsIgnored(GameObject gameObject, ILogger verboseLogger = null);
    }
}
