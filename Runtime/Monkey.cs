// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Operators;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.Assertions;

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
        private static MonoBehaviour s_coroutineRunner; // for take screenshots

        /// <summary>
        /// Run monkey testing by repeating to call <c cref="RunStep" /> and wait.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async UniTask Run(MonkeyConfig config, CancellationToken cancellationToken = default)
        {
            var endTime = config.Lifetime == TimeSpan.MaxValue
                ? TimeSpan.MaxValue.TotalSeconds
                : config.Lifetime.Add(TimeSpan.FromSeconds(Time.time)).TotalSeconds;
            var lastOperationTime = Time.time;

            var beforeGizmos = false;
            if (config.Gizmos)
            {
                beforeGizmos = GameViewControlHelper.GetGizmos();
                GameViewControlHelper.SetGizmos(true);
            }

            var interactiveComponentCollector = new InteractiveComponentCollector(
                isReachable: config.IsReachable,
                isInteractable: config.IsInteractable,
                operators: config.Operators);

            config.Logger.Log($"Using {config.Random}");

            try
            {
                while (Time.time < endTime)
                {
                    var didAct = await RunStep(config, interactiveComponentCollector, cancellationToken);
                    if (didAct)
                    {
                        lastOperationTime = Time.time;
                    }
                    else if (config.SecondsToErrorForNoInteractiveComponent > 0)
                    {
                        Assert.IsTrue(
                            (Time.time - lastOperationTime) < config.SecondsToErrorForNoInteractiveComponent,
                            $"Interactive component not found in {config.SecondsToErrorForNoInteractiveComponent} seconds"
                        );
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

        private class CoroutineRunner : MonoBehaviour
        {
        }

        /// <summary>
        /// Run a step of monkey testing.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="interactiveComponentCollector"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public static async UniTask<bool> RunStep(
            MonkeyConfig config,
            InteractiveComponentCollector interactiveComponentCollector,
            CancellationToken cancellationToken = default)
        {
            var operators = GetOperators(interactiveComponentCollector);
            var (selectedComponent, selectedOperator) = LotteryOperator(operators.ToList(), config.Random);
            if (selectedComponent == null || selectedOperator == null)
            {
                return false;
            }

            if (config.Screenshots != null)
            {
                if (s_coroutineRunner == null || (bool)s_coroutineRunner == false)
                {
                    s_coroutineRunner = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
                }

                await ScreenshotHelper.TakeScreenshot(
                        directory: config.Screenshots.Directory,
                        filename: config.Screenshots.FilenameStrategy.GetFilename(),
                        superSize: config.Screenshots.SuperSize,
                        stereoCaptureMode: config.Screenshots.StereoCaptureMode
                    )
                    .ToUniTask(s_coroutineRunner);
            }

            config.Logger.Log($"{selectedOperator} operates to {selectedComponent.gameObject.name}");
            await selectedOperator.OperateAsync(selectedComponent.component, cancellationToken);
            return true;
        }

        internal static IEnumerable<(InteractiveComponent, IOperator)> GetOperators(
            InteractiveComponentCollector interactiveComponentCollector)
        {
            var components = interactiveComponentCollector.FindInteractableComponents();
            return components.SelectMany(x => x.GetOperators(), (x, o) => (x, o));
        }

        internal static (InteractiveComponent, IOperator) LotteryOperator(
            List<(InteractiveComponent, IOperator)> operators,
            IRandom random)
        {
            while (operators.Count > 0)
            {
                var (selectedComponent, selectedOperator) = operators[random.Next(operators.Count)];
                if (selectedComponent.IsReachable())
                {
                    return (selectedComponent, selectedOperator);
                }

                operators.Remove((selectedComponent, selectedOperator));
            }

            return (null, null);
        }
    }
}
