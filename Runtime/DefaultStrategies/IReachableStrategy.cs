// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Strategy to examine whether <c>GameObject</c> is reachable from the user.
    /// You should replace this when you want to customize the raycast point (e.g., randomize position, specify camera).
    /// </summary>
    public interface IReachableStrategy
    {
        /// <summary>
        /// Logger set if you need verbose output.
        /// </summary>
        ILogger VerboseLogger { set; }

        /// <summary>
        /// Resets the <c>PointerEventData</c> instance it holds, including the <c>EventSystem</c> instance.
        /// </summary>
        void ResetPointerEventData();

        /// <summary>
        /// Returns whether the <c>GameObject</c> is reachable from the user or not and screen position.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>True if <c>GameObject</c> is reachable from user, Raycast screen position</returns>
        (bool, Vector2) IsReachableScreenPosition(GameObject gameObject);

        /// <summary>
        /// Returns whether the <c>GameObject</c> is reachable from the user or not.
        /// (without screen position)
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>True if <c>GameObject</c> is reachable from user</returns>
        bool IsReachable(GameObject gameObject);
    }
}
