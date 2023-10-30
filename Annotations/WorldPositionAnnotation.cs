// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the world position that where monkey operators operate on
    /// </summary>
    public sealed class WorldPositionAnnotation : MonoBehaviour
    {
        /// <summary>
        /// A world position that where monkey operators operate on
        /// </summary>
        [SerializeField]
        public Vector3 position;
    }
}
