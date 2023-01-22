// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Extensions
{
    internal static class GameObjectExtensions
    {
        internal static Vector2 GetScreenPoint(this GameObject gameObject)
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
                    return RectTransformUtility.WorldToScreenPoint(null, gameObject.transform.position);
                }

                return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, gameObject.transform.position);
            }

            return RectTransformUtility.WorldToScreenPoint(GetMainCamera(), gameObject.transform.position);
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
