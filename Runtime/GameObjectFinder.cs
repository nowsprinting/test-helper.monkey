// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find <c>GameObject</c> by name or path (glob). Wait until they appear.
    /// </summary>
    public class GameObjectFinder
    {
        private readonly double _timeoutSeconds;
        private readonly Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> _isReachable;
        private readonly Func<Component, bool> _isComponentInteractable;
        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        private const double MinTimeoutSeconds = 0.01d;
        private const double MaxPollingIntervalSeconds = 1.0d;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appear.</param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not.
        /// Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        public GameObjectFinder(double timeoutSeconds = 1.0d,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable = null,
            Func<Component, bool> isComponentInteractable = null)
        {
            Assert.IsTrue(timeoutSeconds > MinTimeoutSeconds,
                $"TimeoutSeconds must be greater than {MinTimeoutSeconds}.");

            _timeoutSeconds = timeoutSeconds;
            _isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            _isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
        }

        private enum Reason
        {
            NotFound,
            NotMatchPath,
            NotReachable,
            NotInteractable,
            None
        }

        private (GameObject, Reason) FindByName(string name, string path, bool reachable, bool interactable)
        {
            var foundObject = GameObject.Find(name);
            // Note: Cases where there are multiple GameObjects with the same name are not considered.

            if (foundObject == null)
            {
                return (null, Reason.NotFound);
            }

            if (path != null && !foundObject.transform.MatchPath(path))
            {
                return (null, Reason.NotMatchPath);
            }

            if (reachable && !_isReachable.Invoke(foundObject, null, _results, null))
            {
                return (null, Reason.NotReachable);
            }

            if (interactable && !foundObject.GetComponents<Component>().Any(_isComponentInteractable))
            {
                return (null, Reason.NotInteractable);
            }

            return (foundObject, Reason.None);
        }

        private async UniTask<GameObject> FindByNameAsync(string name, string path, bool reachable, bool interactable,
            CancellationToken cancellationToken)
        {
            var timeoutTime = Time.realtimeSinceStartup + (float)_timeoutSeconds;
            var delaySeconds = MinTimeoutSeconds;
            var reason = Reason.None;

            while (Time.realtimeSinceStartup < timeoutTime)
            {
                GameObject foundObject;
                (foundObject, reason) = FindByName(name, path, reachable, interactable);
                if (foundObject != null)
                {
                    return foundObject;
                }

                delaySeconds = Math.Min(delaySeconds * 2, MaxPollingIntervalSeconds);
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), ignoreTimeScale: true,
                    cancellationToken: cancellationToken);
            }

            switch (reason)
            {
                case Reason.NotFound:
                    throw new TimeoutException($"GameObject `{name}` is not found.");
                case Reason.NotMatchPath:
                    throw new TimeoutException($"GameObject `{name}` is found, but it does not match path `{path}`.");
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
        /// <param name="name">Find GameObject name</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found GameObject</returns>
        /// <exception cref="TimeoutException">Throws if GameObject is not found</exception>
        public async UniTask<GameObject> FindByNameAsync(string name, bool reachable = true, bool interactable = false,
            CancellationToken cancellationToken = default)
        {
            return await FindByNameAsync(name, null, reachable, interactable, cancellationToken);
        }

        /// <summary>
        /// Find GameObject by path (wait until they appear).
        /// </summary>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        /// <param name="path">Find GameObject hierarchy path separated by `/`. Can specify glob pattern</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found GameObject</returns>
        /// <exception cref="TimeoutException">Throws if GameObject is not found</exception>
        public async UniTask<GameObject> FindByPathAsync(string path, bool reachable = true, bool interactable = false,
            CancellationToken cancellationToken = default)
        {
            var name = path.Split('/').Last();
            return await FindByNameAsync(name, path, reachable, interactable, cancellationToken);
        }
    }
}
