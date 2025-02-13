// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

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

            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem is not found.");
                return false;
            }

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
                message.Append(" Raycast hit other objects: {");
                foreach (var result in results)
                {
                    message.Append(result.gameObject.name);
                    message.Append(", ");
                }

                message.Remove(message.Length - 2, 2);
                message.Append("}");
                verboseLogger.Log(message.ToString());
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

        private static string CreateMessage(GameObject gameObject, Vector2 position)
        {
            var x = (int)position.x;
            var y = (int)position.y;
            var builder = new StringBuilder();
            builder.Append($"Not reachable to {gameObject.name}({gameObject.GetInstanceID()}), position=({x},{y})");

            var camera = gameObject.GetAssociatedCamera();
            if (camera != null)
            {
                builder.Append($", camera={camera.name}({camera.GetInstanceID()})");
            }

            builder.Append(".");
            return builder.ToString();
        }
    }
}
