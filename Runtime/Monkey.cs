﻿// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
#endif

namespace TestHelper.Monkey
{
    /// <summary>
    /// Reference implementation of the monkey testing.
    ///
    /// - Can specific lifetime and delay time
    /// - Can specific random number generator
    /// - Can lottery and operation only clickable objects
    /// - Can throw assert exception if Interactive component not found in 5 sec
    /// </summary>
    public static class Monkey
    {
        /// <summary>
        /// Run monkey testing by repeating to call <c cref="RunStep" /> and wait.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async UniTask Run(MonkeyConfig config, CancellationToken cancellationToken = default,
            // ReSharper disable once InvalidXmlDocComment
            [CallerMemberName] string callerMemberName = null)
        {
            var endTime = config.Lifetime == TimeSpan.MaxValue
                ? TimeSpan.MaxValue.TotalSeconds
                : config.Lifetime.Add(TimeSpan.FromSeconds(Time.time)).TotalSeconds;
            var lastOperationTime = Time.time;
            var stepCount = 0;

            var beforeGizmos = false;
            if (config.Gizmos)
            {
                beforeGizmos = GameViewControlHelper.GetGizmos();
                GameViewControlHelper.SetGizmos(true);
            }

            if (config.TakeScreenshots)
            {
                ApplyScreenshotConfig(config, callerMemberName);
            }

            config.Logger.Log($"Using {config.Random}");

            try
            {
                while (Time.time < endTime)
                {
                    var didAct = await RunStep(config, ++stepCount, cancellationToken);
                    if (didAct)
                    {
                        lastOperationTime = Time.time;
                    }
                    else if (config.SecondsToErrorForNoInteractiveComponent > 0)
                    {
                        UnityEngine.Assertions.Assert.IsTrue(
                            (Time.time - lastOperationTime) < config.SecondsToErrorForNoInteractiveComponent,
                            $"Interactive component not found in {config.SecondsToErrorForNoInteractiveComponent} seconds");
                    }

                    await UniTask.Delay(config.DelayMillis, DelayType.DeltaTime, cancellationToken: cancellationToken);
                }
            }
            finally
            {
                if (config.Gizmos)
                {
                    GameViewControlHelper.SetGizmos(beforeGizmos);
                }
            }
        }

        private static void ApplyScreenshotConfig(MonkeyConfig config, string callerMemberName)
        {
            if (config.ScreenshotsDirectory == null)
            {
                config.ScreenshotsDirectory =
                    Path.Combine(Application.persistentDataPath, "TestHelper.Monkey", "Screenshots");
            }

            if (config.ScreenshotsFilenamePrefix == null)
            {
#if UNITY_INCLUDE_TESTS
                config.ScreenshotsFilenamePrefix = TestContext.CurrentTestExecutionContext.CurrentTest.Name
                    .Replace('(', '_')
                    .Replace(')', '_')
                    .Replace(',', '-');
                // Note: Same as the file name created under ActualImages of the Graphics Tests Framework package.
#else
                config.ScreenshotsFilenamePrefix = callerMemberName;
#endif
            }
        }

        private class CoroutineRunner : MonoBehaviour
        {
        }

        /// <summary>
        /// Run a step of monkey testing.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="stepCount">Counter for screenshot filename</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public static async UniTask<bool> RunStep(MonkeyConfig config, int stepCount,
            CancellationToken cancellationToken = default)
        {
            var components = InteractiveComponentCollector
                .FindInteractiveComponents()
                .ToList();
            var component = Lottery(ref components, config.Random, config.ScreenPointStrategy);
            if (component == null)
            {
                return false;
            }

            if (config.TakeScreenshots)
            {
                var coroutineRunner = new GameObject().AddComponent<CoroutineRunner>();
                await ScreenshotHelper.TakeScreenshot(
                        directory: config.ScreenshotsDirectory,
                        filename: $"{config.ScreenshotsFilenamePrefix}_{stepCount:D4}.png")
                    .ToUniTask(coroutineRunner);
            }

            await DoOperation(component, config, cancellationToken);
            return true;
        }

        internal static InteractiveComponent Lottery(ref List<InteractiveComponent> components, IRandom random,
            Func<GameObject, Vector2> screenPointStrategy)
        {
            if (components == null || components.Count == 0)
            {
                return null;
            }

            while (true)
            {
                if (components.Count == 0)
                {
                    return null;
                }

                var next = components[random.Next(components.Count)];
                if (next.IsReallyInteractiveFromUser(screenPointStrategy) && GetCanOperations(next).Any())
                {
                    return next;
                }

                components.Remove(next);
            }
        }

        private enum SupportOperation
        {
            Click,
            TouchAndHold,
        }

        private static IEnumerable<IOperator> GetCanOperations(InteractiveComponent component,
            List<MatcherOperatorPair> pairs)
        {
            foreach (var pair in pairs)
            {
                if (pair._matcher.IsMatch(component))
                {
                    yield return pair._operator;
                }
            }
        }

        internal static async UniTask DoOperation(InteractiveComponent component, MonkeyConfig config,
            CancellationToken cancellationToken = default)
        {
            var operations = GetCanOperations(component, config.PrimaryOperators).ToArray();
            if (operations.Length == 0)
            {
                operations = GetCanOperations(component, config.SecondaryOperators).ToArray();
            }

            var operation = operations[config.Random.Next(operations.Length)];
            config.Logger.Log($"Do operation {component.gameObject.name} {operation.ToString()}");
            operation.DoOperation(component, config.ScreenPointStrategy, cancellationToken);
        }
    }
}
