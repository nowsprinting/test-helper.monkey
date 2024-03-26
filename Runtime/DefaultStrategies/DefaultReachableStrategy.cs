// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default strategy to examine whether GameObject is reachable.
    /// </summary>
    public static class DefaultReachableStrategy
    {
        /// <summary>
        /// Make sure the <c>GameObject</c> is reachable from user.
        /// Hit test using raycaster
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="eventData">Specify if avoid GC memory allocation</param>
        /// <param name="results">Specify if avoid GC memory allocation</param>
        /// <returns>True if this GameObject is reachable from user</returns>
        public static bool IsReachable(GameObject gameObject,
            PointerEventData eventData,
            List<RaycastResult> results)
        {
            results.Clear();

            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0 && IsSameOrChildObject(gameObject, results[0].gameObject.transform);
        }

        private static bool IsSameOrChildObject(GameObject target, Transform hitObjectTransform)
        {
            while (hitObjectTransform != null)
            {
                if (hitObjectTransform == target.transform)
                {
                    return true;
                }

                hitObjectTransform = hitObjectTransform.transform.parent;
            }

            return false;
        }
    }
}
