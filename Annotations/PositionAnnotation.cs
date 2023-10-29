// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using TestHelper.Monkey.Extensions;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation base class that indicate the screen position that where monkey operators control on
    /// </summary>
    public class PositionAnnotation : MonoBehaviour, IPositionAnnotation
    {
        /// <inheritdoc />
        public virtual Vector2 GetScreenPoint()
        {
            return GameObjectPosition.GetScreenPoint(gameObject);
        }
    }
}
