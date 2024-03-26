// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find <c>GameObject</c> by name or path (glob). Wait until they appear.
    /// </summary>
    public class GameObjectFinder
    {
        private readonly double _timeoutSeconds;
        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly Func<Component, bool> _isComponentInteractable;
        private readonly Func<GameObject, Func<Component, bool>, bool> _isGameObjectInteractable;
        private readonly Func<GameObject, PointerEventData, List<RaycastResult>, bool> _isReachable;
        private readonly PointerEventData _eventData = new PointerEventData(EventSystem.current);
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appear.</param>
        /// <param name="getScreenPoint">The function returns the screen position where raycast for the found <c>GameObject</c>.
        /// Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="isGameObjectInteractable">The function returns the <c>GameObject</c> is interactable or not.
        /// Default is <c>DefaultGameObjectInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        public GameObjectFinder(double timeoutSeconds = 1.0d,
            Func<GameObject, Vector2> getScreenPoint = null,
            Func<Component, bool> isComponentInteractable = null,
            Func<GameObject, Func<Component, bool>, bool> isGameObjectInteractable = null,
            Func<GameObject, PointerEventData, List<RaycastResult>, bool> isReachable = null)
        {
            _timeoutSeconds = timeoutSeconds;
            _getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
            _isGameObjectInteractable =
                isGameObjectInteractable ?? DefaultGameObjectInteractableStrategy.IsInteractable;
            _isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
        }

        private enum Reason
        {
            NotFound,
            NotReachable,
            NotInteractable,
            None
        }

        private (GameObject, Reason) FindByName(string name, bool reachable, bool interactable)
        {
            var foundObject = GameObject.Find(name);
            // Note: Cases where there are multiple GameObjects with the same name are not considered.

            if (foundObject == null)
            {
                return (null, Reason.NotFound);
            }

            if (reachable)
            {
                _eventData.position = _getScreenPoint.Invoke(foundObject);
                if (!_isReachable.Invoke(foundObject, _eventData, _results))
                {
                    return (null, Reason.NotReachable);
                }
            }

            if (interactable)
            {
                if (!_isGameObjectInteractable.Invoke(foundObject, _isComponentInteractable))
                {
                    return (null, Reason.NotInteractable);
                }
            }

            return (foundObject, Reason.None);
        }

        /// <summary>
        /// Find GameObject by name (wait until they appear).
        /// </summary>
        /// <param name="name">Find GameObject name</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Found GameObject</returns>
        /// <exception cref="TimeoutException">Throws if GameObject is not found</exception>
        public async UniTask<GameObject> FindByNameAsync(string name, bool reachable = true, bool interactable = false,
            CancellationToken token = default)
        {
            var delaySeconds = 0.01d;
            var reason = Reason.None;

            while (delaySeconds < _timeoutSeconds)
            {
                GameObject foundObject;
                (foundObject, reason) = FindByName(name, reachable, interactable);
                if (foundObject != null)
                {
                    return foundObject;
                }

                delaySeconds = Math.Min(delaySeconds * 2, _timeoutSeconds);
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: token);
            }

            switch (reason)
            {
                case Reason.NotFound:
                    throw new TimeoutException($"GameObject `{name}` is not found.");
                case Reason.NotReachable:
                    throw new TimeoutException($"GameObject `{name}` is found, but not reachable.");
                case Reason.NotInteractable:
                    throw new TimeoutException($"GameObject `{name}` is found, but not interactable.");
                case Reason.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Find GameObject by path (wait until they appear).
        /// </summary>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        /// <param name="path">Find GameObject hierarchy path separated by `/`. Can specify glob pattern</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Found GameObject</returns>
        /// <exception cref="TimeoutException">Throws if GameObject is not found</exception>
        public async UniTask<GameObject> FindByPathAsync(string path, bool reachable = true, bool interactable = false,
            CancellationToken token = default)
        {
            var name = path.Split('/').Last();
            var foundObject = await FindByNameAsync(name, reachable, interactable, token);
            if (foundObject.transform.MatchPath(path))
            {
                return foundObject;
            }

            throw new TimeoutException($"GameObject `{name}` is found, but it does not match path `{path}`.");
        }
    }
}
