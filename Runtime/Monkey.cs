// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Exceptions;
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
    ///   <item>Detects that cannot continue</item>
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
        /// <exception cref="InfiniteLoopException">Thrown if a repeating operation is detected within the specified buffer length</exception>
        /// <exception cref="TimeoutException">Thrown if an object that can be interacted with does not exist</exception>
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
            var operationSequence = new List<int>(config.BufferLengthForDetectLooping);

            config.Logger.Log($"Using {config.Random}");

            try
            {
                while (Time.realtimeSinceStartup < endTime)
                {
                    var (didAction, instanceId) = await RunStep(
                        config.Random,
                        config.Logger,
                        config.Screenshots,
                        interactableComponentsFinder,
                        config.IgnoreStrategy,
                        config.ReachableStrategy,
                        config.Verbose,
                        cancellationToken);
                    if (didAction)
                    {
                        lastOperationTime = Time.realtimeSinceStartup;

                        // Detecting infinite loop
                        if (config.BufferLengthForDetectLooping > 0)
                        {
                            if (operationSequence.Count >= config.BufferLengthForDetectLooping)
                                operationSequence.Remove(0);
                            operationSequence.Add(instanceId);
                            if (DetectInfiniteLoop(ref operationSequence))
                            {
                                throw new InfiniteLoopException();
                            }
                        }
                    }
                    else if (config.SecondsToErrorForNoInteractiveComponent > 0 &&
                             config.SecondsToErrorForNoInteractiveComponent <
                             (Time.realtimeSinceStartup - lastOperationTime))
                    {
                        // No interactive component found
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
        /// <param name="ignoreStrategy">Strategy to examine whether <c>GameObject</c> should be ignored. from <c>MonkeyConfig</c></param>
        /// <param name="reachableStrategy">Strategy to examine whether <c>GameObject</c> is reachable from the user. from <c>MonkeyConfig</c></param>
        /// <param name="verbose">Output verbose logs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any operator was executed, and the instance ID of operated <c>GameObject</c></returns>
        public static async UniTask<(bool, int)> RunStep(
            IRandom random,
            ILogger logger,
            ScreenshotOptions screenshotOptions,
            InteractableComponentsFinder interactableComponentsFinder,
            IIgnoreStrategy ignoreStrategy,
            IReachableStrategy reachableStrategy,
            bool verbose = false,
            CancellationToken cancellationToken = default)
        {
            var lotteryEntries = GetLotteryEntries(interactableComponentsFinder, verbose ? logger : null).Distinct();
            var (selectedObject, selectedOperator, raycastResult) = LotteryOperator(
                lotteryEntries, random, ignoreStrategy, reachableStrategy, verbose ? logger : null);
            if (selectedObject == null || selectedOperator == null)
            {
                return (false, 0);
            }

            await selectedOperator.OperateAsync(selectedObject, raycastResult, logger, screenshotOptions,
                cancellationToken);

            return (true, selectedObject.GetInstanceID());
        }

        internal static IEnumerable<(GameObject, IOperator)> GetLotteryEntries(
            InteractableComponentsFinder finder,
            ILogger verboseLogger = null)
        {
            var lotteryEntries = verboseLogger != null ? new StringBuilder() : null;

            foreach (var (component, @operator) in finder.FindInteractableComponentsAndOperators())
            {
                var gameObject = component.gameObject;
                if (verboseLogger != null)
                {
                    lotteryEntries.Append(
                        $"{gameObject.name}({gameObject.GetInstanceID()}):{component.GetType().Name}:{@operator.GetType().Name}, ");
                }

                yield return (gameObject, @operator);
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

        internal static (GameObject, IOperator, RaycastResult) LotteryOperator(
            IEnumerable<(GameObject, IOperator)> operators,
            IRandom random,
            IIgnoreStrategy ignoreStrategy,
            IReachableStrategy reachableStrategy,
            ILogger verboseLogger = null)
        {
            var operatorList = operators.ToList();

            while (operatorList.Count > 0)
            {
                var (selectedObject, selectedOperator) = operatorList[random.Next(operatorList.Count)];
                if (!ignoreStrategy.IsIgnored(selectedObject, verboseLogger) &&
                    reachableStrategy.IsReachable(selectedObject, out var raycastResult, verboseLogger))
                {
                    return (selectedObject, selectedOperator, raycastResult);
                }

                operatorList.Remove((selectedObject, selectedOperator));
            }

            verboseLogger?.Log("Lottery entries are empty or all of not reachable.");
            return (null, null, default);
        }

        internal static bool DetectInfiniteLoop(ref List<int> sequence)
        {
            for (var patternLength = 2; patternLength <= sequence.Count / 2; patternLength++)
            {
                var pattern = sequence.Skip(sequence.Count - patternLength).Take(patternLength).ToArray();
                var isLoop = true;
                for (var i = 0; i < patternLength; i++)
                {
                    if (pattern[i] != sequence[sequence.Count - patternLength - patternLength + i])
                    {
                        isLoop = false;
                        break;
                    }
                }

                if (isLoop)
                {
                    return true;
                }
            }

            return false;
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
