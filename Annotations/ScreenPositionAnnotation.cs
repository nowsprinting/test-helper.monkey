// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the screen position that where monkey operators operate on
    /// </summary>
    public class ScreenPositionAnnotation : PositionAnnotation
    {
        /// <summary>
        /// A screen position that where monkey operators operate on
        /// </summary>
        [SerializeField]
        public Vector2 position;

        /// <inheritdoc />
        public override Vector2 GetScreenPoint()
        {
            return position;
        }
    }
}
