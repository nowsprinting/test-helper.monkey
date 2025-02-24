// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.Assertions;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Find <c>GameObject</c> by name or path (glob). Wait until they appear.
    /// </summary>
    public class GameObjectFinder
    {
        private readonly double _timeoutSeconds;
        private readonly IReachableStrategy _reachableStrategy;
        private readonly Func<Component, bool> _isComponentInteractable;

        private const double MinTimeoutSeconds = 0.01d;
        private const double MaxPollingIntervalSeconds = 1.0d;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appear.</param>
        /// <param name="reachableStrategy">Strategy to examine whether <c>GameObject</c> is reachable from the user. Default is <c>DefaultReachableStrategy</c>.</param>
        /// <param name="isComponentInteractable">The function returns the <c>Component</c> is interactable or not. Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        public GameObjectFinder(double timeoutSeconds = 1.0d,
            IReachableStrategy reachableStrategy = null,
            Func<Component, bool> isComponentInteractable = null)
        {
            Assert.IsTrue(timeoutSeconds > MinTimeoutSeconds,
                $"TimeoutSeconds must be greater than {MinTimeoutSeconds}.");

            _timeoutSeconds = timeoutSeconds;
            _reachableStrategy = reachableStrategy ?? new DefaultReachableStrategy();
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

            if (reachable && !_reachableStrategy.IsReachable(foundObject, out _))
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
        /// Find <c>GameObject</c> by name (wait until they appear).
        /// </summary>
        /// <param name="name">Find <c>GameObject</c> name</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found <c>GameObject</c></returns>
        /// <exception cref="TimeoutException">Throws if <c>GameObject</c> is not found</exception>
        public async UniTask<GameObject> FindByNameAsync(string name, bool reachable = true, bool interactable = false,
            CancellationToken cancellationToken = default)
        {
            return await FindByNameAsync(name, null, reachable, interactable, cancellationToken);
        }

        /// <summary>
        /// Find <c>GameObject</c> by path (wait until they appear).
        /// </summary>
        /// <param name="path">Find <c>GameObject</c> hierarchy path separated by `/`. Can specify glob pattern</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found <c>GameObject</c></returns>
        /// <exception cref="TimeoutException">Throws if <c>GameObject</c> is not found</exception>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public async UniTask<GameObject> FindByPathAsync(string path, bool reachable = true, bool interactable = false,
            CancellationToken cancellationToken = default)
        {
            var name = path.Split('/').Last();
            return await FindByNameAsync(name, path, reachable, interactable, cancellationToken);
        }
    }
}
