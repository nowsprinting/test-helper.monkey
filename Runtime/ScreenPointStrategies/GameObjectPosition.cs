// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.ScreenPointStrategies
{
    /// <summary>
    /// Screen point strategy that dont care any annotations
    /// </summary>
    public static class TransformPositionStrategy
    {
        /// <summary>
        /// Screen point strategy that dont care any annotations
        /// </summary>
        /// <param name="gameObject">GameObject that monkey operators operate</param>
        /// <returns>The screen point of the <paramref name="gameObject"/> transform position</returns>
        public static Vector2 GetScreenPoint(GameObject gameObject) =>
            GetScreenPointByWorldPosition(gameObject, gameObject.transform.position);

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="gameObject">GameObject that monkey operators operate</param>
        /// <param name="pos">The world position where monkey operators operate on</param>
        /// <returns>The screen point of the <paramref name="pos"/></returns>
        public static Vector2 GetScreenPointByWorldPosition(GameObject gameObject, Vector3 pos) =>
            RectTransformUtility.WorldToScreenPoint(CameraSelector.SelectBy(gameObject), pos);
    }
}
