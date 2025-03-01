// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Return value of <see cref="GameObjectFinder.FindByNameAsync"/> and <see cref="GameObjectFinder.FindByPathAsync"/>.
    /// </summary>
    public struct GameObjectFinderResult
    {
        /// <summary>
        /// Found <c>GameObject</c>.
        /// </summary>
        [NotNull]
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// The frontmost raycast hit result will be set regardless of whether the event can be processed.
        /// Set <c>default</c> if <c>reachable</c> is false.
        /// </summary>
        public RaycastResult RaycastResult { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObject">Found <c>GameObject</c></param>
        /// <param name="raycastResult">The frontmost raycast hit result will be set regardless of whether the event can be processed</param>
        public GameObjectFinderResult(GameObject gameObject, RaycastResult raycastResult)
        {
            GameObject = gameObject;
            RaycastResult = raycastResult;
        }
    }
}
