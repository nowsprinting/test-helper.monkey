// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation class that indicate the world position that where monkey operators operate on
    /// </summary>
    public class WorldPositionAnnotation : PositionAnnotation
    {
        /// <summary>
        /// A world position that where monkey operators operate on
        /// </summary>
        [SerializeField]
        public Vector3 position;

        /// <inheritdoc />
        public override Vector2 GetScreenPoint()
        {
            return GameObjectPosition.GetScreenPoint(gameObject, position);
        }
    }
}
