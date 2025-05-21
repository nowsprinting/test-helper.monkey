// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// Interface for providing custom logic to determine whether a <see cref="GameObject"/> matches specific criteria.
    /// Used by <see cref="GameObjectFinder"/> to allow external comparison logic for <see cref="GameObject"/> instances.
    /// </summary>
    public interface IGameObjectMatcher
    {
        /// <summary>
        /// Determines whether the specified <see cref="GameObject"/> matches the implemented criteria.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to evaluate.</param>
        /// <returns><c>true</c> if the <see cref="GameObject"/> matches the criteria; otherwise, <c>false</c>.</returns>
        bool IsMatch(GameObject gameObject);
    }
}
