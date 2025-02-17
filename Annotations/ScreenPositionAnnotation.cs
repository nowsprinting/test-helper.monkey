// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicates the screen position where monkey operators operate.
    /// </summary>
    public sealed class ScreenPositionAnnotation : MonoBehaviour
    {
        /// <summary>
        /// A screen position where monkey operators operate.
        /// It respects <c>CanvasScaler</c> but does not calculate the aspect ratio.
        /// </summary>
        [SerializeField]
        public Vector2 position;
    }
}
