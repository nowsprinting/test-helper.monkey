// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    /// <summary>
    /// An annotation interface that indicate where monkey operators control on
    /// </summary>
    public interface IPositionAnnotation
    {
        /// <summary>
        /// Returns the screen point of the game object.
        /// </summary>
        /// <returns></returns>
        Vector2 GetScreenPoint();
    }
}
