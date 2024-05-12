// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Operators;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
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

            var interactableComponentCollector = new InteractiveComponentCollector(config);

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
                        config.Verbose,
                        cancellationToken);
                    if (didAct)
                    {
                        lastOperationTime = Time.time;
                    }
                    else if (0 < config.SecondsToErrorForNoInteractiveComponent &&
                             config.SecondsToErrorForNoInteractiveComponent < (Time.time - lastOperationTime))
                    {
                        var message = new StringBuilder(
                            $"Interactive component not found in {config.SecondsToErrorForNoInteractiveComponent} seconds");
                        if (config.Screenshots != null)
                        {
                            var filename = config.Screenshots.FilenameStrategy.GetFilename();
                            await TakeScreenshotAsync(config.Screenshots, filename);
                            message.Append($" ({filename})");
                        }

                        throw new TimeoutException(message.ToString());
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

        /// <summary>
        /// Run a step of monkey testing.
        /// </summary>
        /// <param name="random">Random number generator from <c>MonkeyConfig</c></param>
        /// <param name="logger">Logger from <c>MonkeyConfig</c></param>
        /// <param name="screenshotOptions">Take screenshots options from <c>MonkeyConfig</c></param>
        /// <param name="isReachable">Function returns the <c>GameObject</c> is reachable from user or not. from <c>MonkeyConfig</c></param>
        /// <param name="interactableComponentCollector">InteractableComponentCollector instance includes isReachable, isInteractable, and operators</param>
        /// <param name="verbose">Output verbose logs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any operator was executed</returns>
        public static async UniTask<bool> RunStep(
            IRandom random,
            ILogger logger,
            ScreenshotOptions screenshotOptions,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable,
            InteractiveComponentCollector interactableComponentCollector,
            bool verbose = false,
            CancellationToken cancellationToken = default)
        {
            var lotteryEntries = GetLotteryEntries(interactableComponentCollector, verbose ? logger : null);
            var (selectedComponent, selectedOperator) = LotteryOperator(lotteryEntries.ToList(), random, isReachable,
                verbose ? logger : null);
            if (selectedComponent == null || selectedOperator == null)
            {
                return false;
            }

            var message = new StringBuilder($"{selectedOperator} operates to {selectedComponent.gameObject.name}");
            if (screenshotOptions != null)
            {
                var filename = screenshotOptions.FilenameStrategy.GetFilename();
                await TakeScreenshotAsync(screenshotOptions, filename);
                message.Append($" ({filename})");
            }

            logger.Log(message.ToString());

            await selectedOperator.OperateAsync(selectedComponent, cancellationToken);
            return true;
        }

        internal static IEnumerable<(Component, IOperator)> GetLotteryEntries(InteractiveComponentCollector collector,
            ILogger verboseLogger = null)
        {
            var dictionary = verboseLogger != null ? new Dictionary<GameObject, string>() : null;

            foreach (var (component, iOperator) in collector.FindInteractableComponentsAndOperators())
            {
                if (component.gameObject.TryGetComponent(typeof(IgnoreAnnotation), out _))
                {
                    if (dictionary != null && !dictionary.Keys.Contains(component.gameObject))
                    {
                        dictionary.Add(component.gameObject, "Ignored");
                    }

                    continue;
                }

                if (dictionary != null && !dictionary.Keys.Contains(component.gameObject))
                {
                    dictionary.Add(component.gameObject, null);
                }

                yield return (component, iOperator);
            }

            if (verboseLogger != null)
            {
                if (dictionary.Count == 0)
                {
                    verboseLogger.Log("No lottery entries.");
                }
                else
                {
                    var builder = new StringBuilder("Lottery entries: ");
                    foreach (var gameObject in dictionary.Keys)
                    {
                        var value = dictionary[gameObject];
                        if (value != null)
                        {
                            builder.Append($"[{value}]");
                        }

                        builder.Append($"{gameObject.name}({gameObject.GetInstanceID()}), ");
                    }

                    verboseLogger.Log(builder.ToString(0, builder.Length - 2));
                }
            }
        }

        internal static (Component, IOperator) LotteryOperator(
            List<(Component, IOperator)> operators,
            IRandom random,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable,
            ILogger verboseLogger = null)
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            var raycastResults = new List<RaycastResult>();

            while (operators.Count > 0)
            {
                var (selectedComponent, selectedOperator) = operators[random.Next(operators.Count)];
                if (isReachable(selectedComponent.gameObject, pointerEventData, raycastResults, verboseLogger))
                {
                    return (selectedComponent, selectedOperator);
                }

                operators.Remove((selectedComponent, selectedOperator));
            }

            verboseLogger?.Log("Lottery entries are empty or all of not reachable.");
            return (null, null);
        }

        private static async UniTask TakeScreenshotAsync(ScreenshotOptions screenshotOptions, string filename)
        {
            if (!s_coroutineRunner)
            {
                s_coroutineRunner = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
            }

            await ScreenshotHelper.TakeScreenshot(
                    directory: screenshotOptions.Directory,
                    filename: filename,
                    superSize: screenshotOptions.SuperSize,
                    stereoCaptureMode: screenshotOptions.StereoCaptureMode,
                    logFilepath: false
                )
                .ToUniTask(s_coroutineRunner);
        }

        private class CoroutineRunner : MonoBehaviour
        {
        }
    }
}
