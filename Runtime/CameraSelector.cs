// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// A utility to select Camera
    /// </summary>
    public static class CameraSelector
    {
        private static Camera s_cachedMainCamera;
        private static int s_cachedFrame;

        /// <summary>
        /// Returns the first enabled Camera component that is tagged "MainCamera"
        /// </summary>
        /// <returns>The first enabled Camera component that is tagged "MainCamera"</returns>
        public static Camera GetMainCamera()
        {
            if (Time.frameCount == s_cachedFrame)
            {
                return s_cachedMainCamera;
            }

            s_cachedFrame = Time.frameCount;
            return s_cachedMainCamera = Camera.main;
        }

        /// <summary>
        /// Returns an associated camera with <paramref name="gameObject"/>. Or return <c cref="Camera.main" /> if
        /// there are no camera associated with
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>
        /// Camera associated with <paramref name="gameObject"/>, or return <c cref="Camera.main" /> if there are
        /// no camera associated with
        /// </returns>
        public static Camera SelectBy(GameObject gameObject)
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return GetMainCamera();
            }

            if (!canvas.isRootCanvas)
            {
                canvas = canvas.rootCanvas;
            }

            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
    }
}
