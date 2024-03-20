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
        /// <summary>
        /// Find <c>GameObject</c> type.
        /// </summary>
        public GameObjectType FindGameObjectType { get; set; } = GameObjectType.Active;

        /// <summary>
        /// The function returns the screen position where raycast for the found <c>GameObject</c>.
        /// Need for if <c>FindGameObjectType</c> is <c>Reachable</c>.
        /// </summary>
        public Func<GameObject, Vector2> ScreenPointStrategy { get; set; }

        /// <summary>
        /// Seconds to wait until <c>GameObject</c> appear.
        /// </summary>
        public double SecondsToWait { get; set; } = DefaultSecondsToWait;

        private const double DefaultSecondsToWait = 1.0d;

        /// <summary>
        /// Create standard finder for GameObject that reachable from user.
        /// </summary>
        /// <returns></returns>
        public static GameObjectFinder CreateReachableGameObjectFinder(
            double secondsToWait = DefaultSecondsToWait)
        {
            return new GameObjectFinder
            {
                FindGameObjectType = GameObjectType.Reachable,
                ScreenPointStrategy = DefaultScreenPointStrategy.GetScreenPoint,
                SecondsToWait = secondsToWait,
            };
        }

        /// <summary>
        /// Create standard finder for interactive GameObject.
        /// </summary>
        /// <returns></returns>
        public static GameObjectFinder CreateInteractiveGameObjectFinder(double secondsToWait = DefaultSecondsToWait)
        {
            return new GameObjectFinder
            {
                FindGameObjectType = GameObjectType.Interactive, //
                SecondsToWait = secondsToWait,
            };
        }

        private GameObject FindByName(string name)
        {
            var foundObject = GameObject.Find(name);
            if (foundObject == null)
            {
                return null;
            }

            if (FindGameObjectType == GameObjectType.Reachable)
            {
                if (!foundObject.IsReachable(ScreenPointStrategy))
                {
                    return null;
                }
            }

            if (FindGameObjectType == GameObjectType.Interactive)
            {
                if (!foundObject.IsInteractable())
                {
                    return null;
                }
            }

            return foundObject;
        }

        /// <summary>
        /// Find GameObject by name (wait until they appear).
        /// </summary>
        /// <param name="name">Find GameObject name</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Found GameObject</returns>
        /// <exception cref="TimeoutException"></exception>
        public async UniTask<GameObject> FindByNameAsync(string name, CancellationToken token = default)
        {
            var delaySeconds = 0.02d;
            GameObject foundObject;
            while ((foundObject = FindByName(name)) == null)
            {
                if (delaySeconds > SecondsToWait)
                {
                    throw new TimeoutException($"GameObject `{name}` not found.");
                }

                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds *= 2), cancellationToken: token);
            }

            return foundObject;
        }

        /// <summary>
        /// Find GameObject by name (wait until they appear).
        /// </summary>
        /// <param name="path">Find GameObject hierarchy path</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Found GameObject</returns>
        /// <exception cref="TimeoutException"></exception>
        public async UniTask<GameObject> FindByPathAsync(string path, CancellationToken token = default)
        {
            var name = path.Split('/').Last();
            var foundObject = await FindByNameAsync(name, token);

            if (foundObject.transform.MatchPath(path))
            {
                return foundObject;
            }

            throw new TimeoutException($"GameObject `{name}` is found, but it does not match path `{path}`.");
        }
    }
}
