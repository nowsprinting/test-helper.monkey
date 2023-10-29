// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey
{
    public static class OperationPosition
    {
        public static Vector2 GetAsScreenPoint(GameObject gameObject)
        {
            return gameObject.TryGetComponent<IPositionAnnotation>(out var positionAnnotation)
                ? positionAnnotation.GetScreenPoint()
                : GameObjectPosition.GetScreenPoint(gameObject);
        }
    }
}
