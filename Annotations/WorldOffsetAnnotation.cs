// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the screen position offset on world space that where monkey operators operate
    /// on
    /// </summary>
    public class WorldOffsetAnnotation : PositionAnnotation
    {
        /// <summary>
        /// Offset from a world point of the GameObject that the annotation attached to
        /// </summary>
        [SerializeField]
        public Vector3 offset;

        /// <inheritdoc />
        public override Vector2 GetScreenPoint()
        {
            var go = gameObject;
            return GameObjectPosition.GetScreenPoint(go, go.transform.position + offset);
        }
    }
}
