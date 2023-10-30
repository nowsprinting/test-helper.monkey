// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the screen position that where monkey operators operate on
    /// </summary>
    public sealed class ScreenPositionAnnotation : MonoBehaviour
    {
        /// <summary>
        /// A screen position that where monkey operators operate on
        /// </summary>
        [SerializeField]
        public Vector2 position;
    }
}
