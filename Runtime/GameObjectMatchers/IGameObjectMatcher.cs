// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// An interface that provides a custom logic for determining whether a <see cref="GameObject"/> matches certain criteria.
    /// Used by <see cref="GameObjectFinder"/>.
    /// </summary>
    public interface IGameObjectMatcher
    {
        /// <summary>
        /// Determines if a <see cref="GameObject"/> matches the implemented criteria.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to evaluate.</param>
        /// <returns><c>true</c> if the <see cref="GameObject"/> matches the criteria; otherwise, <c>false</c>.</returns>
        bool IsMatch(GameObject gameObject);
    }
}
