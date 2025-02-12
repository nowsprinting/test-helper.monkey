// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    /// <list type="bullet">
    ///   <item>Can specify lifetime and delay time</item>
    ///   <item>Can specify random number generator</item>
    ///   <item>Only clickable objects can be selected or operated</item>
    ///   <item>Throw assert exception if Interactive component not found in 5 sec</item>
    /// </list>
    /// </summary>
    public static class Monkey
    {
        private static MonoBehaviour s_coroutineRunner; // for take screenshots

        /// <summary>
        /// Run monkey testing by repeating to call <c cref="RunStep">RunStep</c> and wait.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async UniTask Run(MonkeyConfig config, CancellationToken cancellationToken = default)
        {
            var endTime = config.Lifetime == TimeSpan.MaxValue
                ? TimeSpan.MaxValue.TotalSeconds
                : config.Lifetime.Add(TimeSpan.FromSeconds(Time.realtimeSinceStartup)).TotalSeconds;
            var lastOperationTime = Time.realtimeSinceStartup;

            var beforeGizmos = false;
            if (config.Gizmos)
            {
                beforeGizmos = GameViewControlHelper.GetGizmos();
                GameViewControlHelper.SetGizmos(true);
            }

            var interactableComponentsFinder = new InteractableComponentsFinder(config);

            config.Logger.Log($"Using {config.Random}");

            try
            {
                while (Time.realtimeSinceStartup < endTime)
                {
                    var didAction = await RunStep(
                        config.Random,
                        config.Logger,
                        config.Screenshots,
                        interactableComponentsFinder,
                        config.IsIgnored,
                        config.IsReachable,
                        config.Verbose,
                        cancellationToken);
                    if (didAction)
                    {
                        lastOperationTime = Time.realtimeSinceStartup;
                    }
                    else if (0 < config.SecondsToErrorForNoInteractiveComponent &&
                             config.SecondsToErrorForNoInteractiveComponent <
                             (Time.realtimeSinceStartup - lastOperationTime))
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

                    await UniTask.Delay(config.DelayMillis, ignoreTimeScale: true,
                        cancellationToken: cancellationToken);
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
        /// This method is internal by nature, called from <c cref="Run">Run</c> method.
        /// </summary>
        /// <param name="random">Random number generator from <c>MonkeyConfig</c></param>
        /// <param name="logger">Logger from <c>MonkeyConfig</c></param>
        /// <param name="screenshotOptions">Take screenshots options from <c>MonkeyConfig</c></param>
        /// <param name="interactableComponentsFinder">InteractableComponentsFinder instance includes isInteractable and operators</param>
        /// <param name="isIgnored">Function returns the <c>GameObject</c> is ignored or not. from <c>MonkeyConfig</c></param>
        /// <param name="isReachable">Function returns the <c>GameObject</c> is reachable from user or not. from <c>MonkeyConfig</c></param>
        /// <param name="verbose">Output verbose logs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any operator was executed</returns>
        public static async UniTask<bool> RunStep(
            IRandom random,
            ILogger logger,
            ScreenshotOptions screenshotOptions,
            InteractableComponentsFinder interactableComponentsFinder,
            Func<GameObject, ILogger, bool> isIgnored,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable,
            bool verbose = false,
            CancellationToken cancellationToken = default)
        {
            var lotteryEntries = GetLotteryEntries(interactableComponentsFinder, verbose ? logger : null);
            var (selectedComponent, selectedOperator) =
                LotteryOperator(lotteryEntries.ToList(), random, isIgnored, isReachable, verbose ? logger : null);
            if (selectedComponent == null || selectedOperator == null)
            {
                return false;
            }

            var message = new StringBuilder();
            message.Append($"{selectedOperator.GetType().Name} operates to {selectedComponent.gameObject.name}");
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

        internal static IEnumerable<(Component, IOperator)> GetLotteryEntries(
            InteractableComponentsFinder interactableComponentsFinder,
            ILogger verboseLogger = null)
        {
            var lotteryEntries = verboseLogger != null ? new StringBuilder() : null;

            foreach (var (component, iOperator) in
                     interactableComponentsFinder.FindInteractableComponentsAndOperators())
            {
                if (verboseLogger != null)
                {
                    lotteryEntries.Append(
                        $"{component.gameObject.name}({component.gameObject.GetInstanceID()}):{component.GetType().Name}:{iOperator.GetType().Name}, ");
                }

                yield return (component, iOperator);
            }

            if (verboseLogger == null)
            {
                yield break;
            }

            if (lotteryEntries.Length == 0)
            {
                verboseLogger.Log("No lottery entries.");
                yield break;
            }

            lotteryEntries.Insert(0, "Lottery entries: {");
            lotteryEntries.Remove(lotteryEntries.Length - 2, 2);
            lotteryEntries.Append("}");
            verboseLogger.Log(lotteryEntries.ToString());
        }

        internal static (Component, IOperator) LotteryOperator(
            List<(Component, IOperator)> operators,
            IRandom random,
            Func<GameObject, ILogger, bool> isIgnored,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable,
            ILogger verboseLogger = null)
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            var raycastResults = new List<RaycastResult>();

            while (operators.Count > 0)
            {
                var (selectedComponent, selectedOperator) = operators[random.Next(operators.Count)];
                if (!isIgnored(selectedComponent.gameObject, verboseLogger) &&
                    isReachable(selectedComponent.gameObject, pointerEventData, raycastResults, verboseLogger))
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
