// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the screen position offset on screen space that where monkey operators operate
    /// on
    /// </summary>
    public class ScreenOffsetAnnotation : PositionAnnotation
    {
        /// <summary>
        /// Offset from a screen point of the GameObject that the annotation attached to
        /// </summary>
        [SerializeField]
        public Vector2 offset;
        
        /// <inheritdoc />
        public override Vector2 GetScreenPoint()
        {
            return base.GetScreenPoint() + offset;
        }
    }
}
