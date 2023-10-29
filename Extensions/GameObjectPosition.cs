// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey.Extensions
{
    public static class GameObjectPosition
    {
        public static Vector2 GetScreenPoint(GameObject gameObject, Vector3? optPos = null)
        {
            var pos = optPos.HasValue ? optPos.Value : gameObject.transform.position;
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
