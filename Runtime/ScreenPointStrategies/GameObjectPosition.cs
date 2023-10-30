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
        public static Vector2 GetScreenPoint(GameObject gameObject)
        {
            return GetScreenPointByWorldPosition(gameObject, gameObject.transform.position);
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="gameObject">GameObject that monkey operators operate</param>
        /// <param name="pos">The world position where monkey operators operate on</param>
        /// <returns>The screen point of the <paramref name="pos"/></returns>
        public static Vector2 GetScreenPointByWorldPosition(GameObject gameObject, Vector3 pos)
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (!canvas.isRootCanvas)
                {
                    canvas = canvas.rootCanvas;
                }

                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return RectTransformUtility.WorldToScreenPoint(null, pos);
                }

                return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, pos);
            }

            return RectTransformUtility.WorldToScreenPoint(GetMainCamera(), pos);
        }


        private static Camera s_cachedMainCamera;
        private static int s_cachedFrame;


        private static Camera GetMainCamera()
        {
            if (Time.frameCount == s_cachedFrame)
            {
                return s_cachedMainCamera;
            }

            s_cachedFrame = Time.frameCount;
            return s_cachedMainCamera = Camera.main;
        }
    }
}
