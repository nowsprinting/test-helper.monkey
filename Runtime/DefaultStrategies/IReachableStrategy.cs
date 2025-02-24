// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Strategy to examine whether <c>GameObject</c> is reachable from the user.
    /// You should implement this when you want to customize the raycast point (e.g., randomize position, specify camera).
    /// </summary>
    public interface IReachableStrategy
    {
        /// <summary>
        /// Returns whether the <c>GameObject</c> is reachable from the user or not and screen position.
        /// </summary>
        /// <param name="gameObject">Target <c>GameObject</c></param>
        /// <param name="position">Returns raycast screen position</param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <returns>True if <c>GameObject</c> is reachable from user, and set <c>position</c></returns>
        bool IsReachable(GameObject gameObject, out Vector2 position, ILogger verboseLogger = null);
    }
}
