// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default strategy to examine whether GameObject is reachable.
    /// </summary>
    public static class DefaultReachableStrategy
    {
        private static Func<GameObject, Vector2> GetScreenPoint => DefaultScreenPointStrategy.GetScreenPoint;

        /// <summary>
        /// Make sure the <c>GameObject</c> is reachable from user.
        /// Hit test using raycaster
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="eventData">Specify if avoid GC memory allocation</param>
        /// <param name="results">Specify if avoid GC memory allocation</param>
        /// <param name="verboseLogger">Output verbose log if need</param>
        /// <returns>True if this GameObject is reachable from user</returns>
        public static bool IsReachable(GameObject gameObject,
            PointerEventData eventData = null,
            List<RaycastResult> results = null,
            ILogger verboseLogger = null)
        {
            eventData = eventData ?? new PointerEventData(EventSystem.current);
            eventData.position = GetScreenPoint.Invoke(gameObject);

            results = results ?? new List<RaycastResult>();
            results.Clear();

            EventSystem.current.RaycastAll(eventData, results);
            if (results.Count == 0)
            {
                if (verboseLogger != null)
                {
                    var message = new StringBuilder(CreateMessage(gameObject, eventData.position));
                    message.Append(" Raycast is not hit.");
                    verboseLogger.Log(message.ToString());
                }

                return false;
            }

            var isSameOrChildObject = IsSameOrChildObject(gameObject, results[0].gameObject.transform);
            if (!isSameOrChildObject && verboseLogger != null)
            {
                var message = new StringBuilder(CreateMessage(gameObject, eventData.position));
                message.Append(" Raycast hit other objects: ");
                foreach (var result in results)
                {
                    message.Append(result.gameObject.name);
                    message.Append(", ");
                }

                verboseLogger.Log(message.ToString(0, message.Length - 2));
            }

            return isSameOrChildObject;
        }

        private static bool IsSameOrChildObject(GameObject target, Transform hitObjectTransform)
        {
            while (hitObjectTransform)
            {
                if (hitObjectTransform == target.transform)
                {
                    return true;
                }

                hitObjectTransform = hitObjectTransform.transform.parent;
            }

            return false;
        }

        private static string CreateMessage(Object gameObject, Vector2 position)
        {
            var x = (int)position.x;
            var y = (int)position.y;
            return $"Not reachable to {gameObject.name}({gameObject.GetInstanceID()}), position=({x},{y}).";
        }
    }
}
