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
    /// Default strategy to examine whether <c>GameObject</c> is reachable from the user.
    /// </summary>
    public class DefaultReachableStrategy : IReachableStrategy
    {
        [Obsolete]
        private static Func<GameObject, Vector2> GetScreenPoint => DefaultScreenPointStrategy.GetScreenPoint;

        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly ILogger _verboseLogger;
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        private PointerEventData _cachedPointerEventData;
        private int _cachedFrameCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        public DefaultReachableStrategy(Func<GameObject, Vector2> getScreenPoint = null, ILogger verboseLogger = null)
        {
            _getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _verboseLogger = verboseLogger;
        }

        /// <summary>
        /// Returns whether the <c>GameObject</c> is reachable from the user or not and screen position.
        /// Default implementation uses <c>DefaultScreenPointStrategy</c>, checks whether a raycast from <c>Camera.main</c> to the pivot position passes through.
        /// </summary>
        /// <param name="gameObject">Target <c>GameObject</c></param>
        /// <param name="raycastResult">Returns the front-most raycast hit result, even if it can not handle the press event</param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <returns>True if <c>GameObject</c> is reachable from user, Raycast screen position</returns>
        public bool IsReachable(GameObject gameObject, out RaycastResult raycastResult, ILogger verboseLogger = null)
        {
            verboseLogger = verboseLogger ?? _verboseLogger; // If null, use the specified in the constructor.

            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem is not found.");
                raycastResult = default;
                return false;
            }

            var pointerEventData = GetCachedPointerEventData();
            pointerEventData.position = _getScreenPoint.Invoke(gameObject);

            _results.Clear();
            EventSystem.current.RaycastAll(pointerEventData, _results);
            if (_results.Count == 0)
            {
                if (verboseLogger != null)
                {
                    var message = new StringBuilder(CreateMessage(gameObject, pointerEventData.position));
                    message.Append(" Raycast is not hit.");
                    verboseLogger.Log(message.ToString());
                }

                raycastResult = default;
                return false;
            }

            var isSameOrChildObject = IsSameOrChildObject(gameObject, _results[0].gameObject.transform);
            if (!isSameOrChildObject && verboseLogger != null)
            {
                var message = new StringBuilder(CreateMessage(gameObject, pointerEventData.position));
                message.Append(" Raycast hit other objects: [");
                foreach (var result in _results)
                {
                    message.Append($"{result.gameObject.name}({result.gameObject.GetInstanceID()})");
                    message.Append(", ");
                }

                message.Remove(message.Length - 2, 2);
                message.Append("]");
                verboseLogger.Log(message.ToString());
            }

            raycastResult = _results[0];
            return isSameOrChildObject;
        }

        private PointerEventData GetCachedPointerEventData()
        {
            if (_cachedPointerEventData == null || _cachedFrameCount != Time.frameCount)
            {
                _cachedPointerEventData = new PointerEventData(EventSystem.current);
                _cachedFrameCount = Time.frameCount;
            }

            return _cachedPointerEventData;
        }

        /// <summary>
        /// Make sure the <c>GameObject</c> is reachable from user.
        /// Hit test using raycaster
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="pointerEventData">Specify this if you want to avoid GC memory allocation. It contains an <c>EventSystem</c> instance, so don't cache it carelessly.</param>
        /// <param name="results">Specify this if you want to avoid GC memory allocation</param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <returns>True if this GameObject is reachable from user</returns>
        [Obsolete("Use instance method instead")]
        public static bool IsReachable(GameObject gameObject,
            PointerEventData pointerEventData = null,
            List<RaycastResult> results = null,
            ILogger verboseLogger = null)
        {
            pointerEventData = pointerEventData ?? new PointerEventData(EventSystem.current);
            pointerEventData.position = GetScreenPoint.Invoke(gameObject);

            results = results ?? new List<RaycastResult>();
            results.Clear();

            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem is not found.");
                return false;
            }

            EventSystem.current.RaycastAll(pointerEventData, results);
            if (results.Count == 0)
            {
                if (verboseLogger != null)
                {
                    var message = new StringBuilder(CreateMessage(gameObject, pointerEventData.position));
                    message.Append(" Raycast is not hit.");
                    verboseLogger.Log(message.ToString());
                }

                return false;
            }

            var isSameOrChildObject = IsSameOrChildObject(gameObject, results[0].gameObject.transform);
            if (!isSameOrChildObject && verboseLogger != null)
            {
                var message = new StringBuilder(CreateMessage(gameObject, pointerEventData.position));
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
