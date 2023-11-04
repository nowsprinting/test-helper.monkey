// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Utility for calculating position for interactive component hints
    /// </summary>
    public static class InteractiveComponentHintPosition
    {
        /// <summary>
        /// Get operation point in world space where monkey operators operate on
        /// the <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera">Camera used by EventSystems</param>
        /// <param name="screenPoint">Screen point got by screen position strategies</param>
        /// <param name="targetPos">World space position for the hint target</param>
        /// <returns></returns>
        public static Vector3 GetWorldPoint(Camera camera, Vector2 screenPoint, Vector3 targetPos)
        {
            var screenSpaceOverlay = camera == null;
            if (screenSpaceOverlay)
            {
                return screenPoint;
            }

            var camTransform = camera.transform;

            if (camera.orthographic)
            {
                var originalScreenPoint = (Vector2)camera.WorldToScreenPoint(targetPos);
                var unitPerPx = camera.orthographicSize * 2 / Screen.height;
                var d = (screenPoint - originalScreenPoint) * unitPerPx;
                return targetPos + camTransform.right * d.x + camTransform.up * d.y;
            }

            var camBack = camTransform.forward * -1;
            var camPos = camTransform.position;

            // Ray is a direction vector from the camera position to the operation point in world space
            var anyPointInRay = camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 1));
            var rayDirection = (anyPointInRay - camPos).normalized;
            var x = Vector3.Dot(camBack, targetPos);
            return camPos + (x - Vector3.Dot(camBack, camPos)) / Vector3.Dot(camBack, rayDirection) * rayDirection;
        }
    }
}
