// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using UnityEngine;

namespace TestHelper.Monkey.ScreenPointStrategies
// TODO: Move to TestHelper.Monkey.DefaultStrategies namespace
{
    /// <summary>
    /// Default screen point strategy.
    /// </summary>
    public static class DefaultScreenPointStrategy
    {
        /// <summary>
        /// Default screen point strategy that care 4 position annotations in the order (upper one has higher priority):
        /// <list type="number">
        /// <item><description><c cref="ScreenPositionAnnotation">ScreenPositionAnnotation</c></description></item>
        /// <item><description><c cref="WorldPositionAnnotation">WorldPositionAnnotation</c></description></item>
        /// <item><description><c cref="ScreenOffsetAnnotation">ScreenOffsetAnnotation</c></description></item>
        /// <item><description><c cref="WorldOffsetAnnotation">WorldOffsetAnnotation</c></description></item>
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
            if (gameObject.TryGetComponent<ScreenPositionAnnotation>(out var screenPositionAnnotation))
            {
                return screenPositionAnnotation.position;
            }

            if (gameObject.TryGetComponent<WorldPositionAnnotation>(out var worldPositionAnnotation))
            {
                return TransformPositionStrategy.GetScreenPointByWorldPosition(
                    gameObject,
                    worldPositionAnnotation.position
                );
            }

            if (gameObject.TryGetComponent<ScreenOffsetAnnotation>(out var screenOffsetAnnotation))
            {
                return TransformPositionStrategy.GetScreenPoint(gameObject) + screenOffsetAnnotation.offset;
            }

            if (gameObject.TryGetComponent<WorldOffsetAnnotation>(out var worldOffsetAnnotation))
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
