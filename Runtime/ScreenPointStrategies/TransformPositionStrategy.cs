﻿// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.ScreenPointStrategies
{
    /// <summary>
    /// Screen point strategy that dont care any annotations
    /// </summary>
    [Obsolete("Use DefaultStrategies.TransformPositionStrategy instead.")]
    public static class TransformPositionStrategy
    {
        /// <summary>
        /// Screen point strategy that dont care any annotations
        /// </summary>
        /// <param name="gameObject">GameObject that monkey operators operate</param>
        /// <returns>The screen point of the <paramref name="gameObject"/> transform position</returns>
        [Obsolete("Use DefaultStrategies.TransformPositionStrategy.GetScreenPoint instead.")]
        public static Vector2 GetScreenPoint(GameObject gameObject) =>
            GetScreenPointByWorldPosition(gameObject, gameObject.transform.position);

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="gameObject">GameObject that monkey operators operate</param>
        /// <param name="pos">The world position where monkey operators operate on</param>
        /// <returns>The screen point of the <paramref name="pos"/></returns>
        [Obsolete("Use DefaultStrategies.TransformPositionStrategy.GetScreenPointByWorldPosition instead.")]
        public static Vector2 GetScreenPointByWorldPosition(GameObject gameObject, Vector3 pos) =>
            RectTransformUtility.WorldToScreenPoint(gameObject.GetAssociatedCamera(), pos);
    }
}
