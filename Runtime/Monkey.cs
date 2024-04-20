// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

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

            var interactableComponentCollector = new InteractiveComponentCollector(
                isReachable: config.IsReachable,
                isInteractable: config.IsInteractable,
                operators: config.Operators);

            config.Logger.Log($"Using {config.Random}");

            try
            {
                while (Time.time < endTime)
                {
                    var didAct = await RunStep(
                        config.Random,
                        config.Logger,
                        config.Screenshots,
                        config.IsReachable,
                        interactableComponentCollector,
                        cancellationToken);
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
        /// <param name="random">Random number generator from <c>MonkeyConfig</c></param>
        /// <param name="logger">Logger from <c>MonkeyConfig</c></param>
        /// <param name="screenshotOptions">Take screenshots options from <c>MonkeyConfig</c></param>
        /// <param name="isReachable">Function returns the <c>GameObject</c> is reachable from user or not. from <c>MonkeyConfig</c></param>
        /// <param name="interactableComponentCollector">InteractableComponentCollector instance includes isReachable, isInteractable, and operators</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any operator was executed</returns>
        public static async UniTask<bool> RunStep(
            IRandom random,
            ILogger logger,
            ScreenshotOptions screenshotOptions,
            Func<GameObject, PointerEventData, List<RaycastResult>, bool> isReachable,
            InteractiveComponentCollector interactableComponentCollector,
            CancellationToken cancellationToken = default)
        {
            var operators = GetOperators(interactableComponentCollector);
            var (selectedComponent, selectedOperator) = LotteryOperator(operators.ToList(), random, isReachable);
            if (selectedComponent == null || selectedOperator == null)
            {
                return false;
            }

            if (screenshotOptions != null)
            {
                if (s_coroutineRunner == null || (bool)s_coroutineRunner == false)
                {
                    s_coroutineRunner = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
                }

                await ScreenshotHelper.TakeScreenshot(
                        directory: screenshotOptions.Directory,
                        filename: screenshotOptions.FilenameStrategy.GetFilename(),
                        superSize: screenshotOptions.SuperSize,
                        stereoCaptureMode: screenshotOptions.StereoCaptureMode
                    )
                    .ToUniTask(s_coroutineRunner);
            }

            logger.Log($"{selectedOperator} operates to {selectedComponent.gameObject.name}");
            await selectedOperator.OperateAsync(selectedComponent, cancellationToken);
            return true;
        }

        internal static IEnumerable<(Component, IOperator)> GetOperators(InteractiveComponentCollector collector)
        {
            return collector.FindInteractableComponentsAndOperators()
                .Where(x => !x.Item1.gameObject.TryGetComponent(typeof(IgnoreAnnotation), out _));
        }

        internal static (Component, IOperator) LotteryOperator(List<(Component, IOperator)> operators, IRandom random,
            Func<GameObject, PointerEventData, List<RaycastResult>, bool> isReachable)
        {
            while (operators.Count > 0)
            {
                var (selectedComponent, selectedOperator) = operators[random.Next(operators.Count)];
                if (selectedComponent.gameObject.IsReachable(isReachable))
                {
                    return (selectedComponent, selectedOperator);
                }

                operators.Remove((selectedComponent, selectedOperator));
            }

            return (null, null);
        }
    }
}
