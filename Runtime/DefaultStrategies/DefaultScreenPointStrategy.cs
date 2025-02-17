// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default screen point strategy.
    /// </summary>
    public static class DefaultScreenPointStrategy
    {
        /// <summary>
        /// Default screen point strategy that cares about four position annotations in the order (upper one has higher priority):
        /// <list type="number">
        ///   <item><c cref="ScreenPositionAnnotation">ScreenPositionAnnotation</c></item>
        ///   <item><c cref="WorldPositionAnnotation">WorldPositionAnnotation</c></item>
        ///   <item><c cref="ScreenOffsetAnnotation">ScreenOffsetAnnotation</c></item>
        ///   <item><c cref="WorldOffsetAnnotation">WorldOffsetAnnotation</c></item>
        /// </list>
        /// Returns the transform position if no annotations specified.
        ///
        /// <example>
        /// You can extend the strategy like the following example:
        /// <code>
        /// () => HasYourAnnotation(gameObject)
        ///     ? PositionYouNeed(gameObject)
        ///     : DefaultScreenPointStrategy.GetScreenPoint(gameObject)
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="gameObject">GameObject that monkey operators operate</param>
        /// <returns>The screen point where monkey operators operate on</returns>
        public static Vector2 GetScreenPoint(GameObject gameObject)
        {
            if (gameObject.TryGetEnabledComponent<ScreenPositionAnnotation>(out var screenPositionAnnotation))
            {
                return screenPositionAnnotation.position;
            }

            if (gameObject.TryGetEnabledComponent<WorldPositionAnnotation>(out var worldPositionAnnotation))
            {
                return TransformPositionStrategy.GetScreenPointByWorldPosition(
                    gameObject,
                    worldPositionAnnotation.position
                );
            }

            if (gameObject.TryGetEnabledComponent<ScreenOffsetAnnotation>(out var screenOffsetAnnotation))
            {
                return TransformPositionStrategy.GetScreenPoint(gameObject) + screenOffsetAnnotation.offset;
            }

            if (gameObject.TryGetEnabledComponent<WorldOffsetAnnotation>(out var worldOffsetAnnotation))
            {
                return TransformPositionStrategy.GetScreenPointByWorldPosition(
                    gameObject,
                    gameObject.transform.position + worldOffsetAnnotation.offset
                );
            }

            return TransformPositionStrategy.GetScreenPoint(gameObject);
        }
    }
}
