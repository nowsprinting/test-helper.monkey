// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find <c>GameObject</c> by name or path (grob). Wait until they appear.
    /// </summary>
    public class GameObjectFinder
    {
        private readonly double _timeoutSeconds;
        private readonly Func<GameObject, Vector2> _screenPointStrategy;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appear.</param>
        /// <param name="screenPointStrategy">The function returns the screen position where raycast for the found <c>GameObject</c>.
        /// Default is <c>DefaultScreenPointStrategy.GetScreenPoint</c>.</param>
        public GameObjectFinder(double timeoutSeconds = 1.0d, Func<GameObject, Vector2> screenPointStrategy = null)
        {
            _timeoutSeconds = timeoutSeconds;
            _screenPointStrategy = screenPointStrategy ?? DefaultScreenPointStrategy.GetScreenPoint;
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

            if (reachable && !foundObject.IsReachable(_screenPointStrategy))
            {
                return (null, Reason.NotReachable);
            }

            if (interactable && !foundObject.IsInteractable())
            {
                return (null, Reason.NotInteractable);
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
        /// Find GameObject by name (wait until they appear).
        /// </summary>
        /// <param name="path">Find GameObject hierarchy path</param>
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
