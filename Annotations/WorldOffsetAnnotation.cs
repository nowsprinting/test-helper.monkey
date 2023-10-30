// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the screen position offset on world space that where monkey operators operate
    /// on
    /// </summary>
    public sealed class WorldOffsetAnnotation : MonoBehaviour
    {
        /// <summary>
        /// Offset from a world point of the GameObject that the annotation attached to
        /// </summary>
        [SerializeField]
        public Vector3 offset;
    }
}
