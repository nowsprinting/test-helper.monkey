// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Random;
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
        /// <summary>
        /// Run monkey testing.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async UniTask Run(MonkeyConfig config, CancellationToken cancellationToken = default)
        {
            var endTime = config.Lifetime == TimeSpan.MaxValue
                ? TimeSpan.MaxValue.TotalSeconds
                : config.Lifetime.Add(TimeSpan.FromSeconds(Time.time)).TotalSeconds;
            var lastOperationTime = Time.time;

            config.Logger.Log($"Using {config.Random}");

            while (Time.time < endTime)
            {
                var components = InteractiveComponentCollector.FindInteractiveComponents(false).ToList();
                var component = Lottery(ref components, config.Random);
                if (component != null)
                {
                    lastOperationTime = Time.time;
                    await DoOperation(component, config, cancellationToken);
                }
                else if (config.SecondsToErrorForNoInteractiveComponent > 0)
                {
                    Assert.IsTrue((Time.time - lastOperationTime) < config.SecondsToErrorForNoInteractiveComponent,
                        $"Interactive component not found in {config.SecondsToErrorForNoInteractiveComponent} seconds");
                }

                await UniTask.Delay(config.DelayMillis, DelayType.DeltaTime, cancellationToken: cancellationToken);
            }
        }

        internal static InteractiveComponent Lottery(ref List<InteractiveComponent> components, IRandom random)
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
                if (next.IsReallyInteractiveFromUser() && GetCanOperations(next).Any())
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

        private static IEnumerable<SupportOperation> GetCanOperations(InteractiveComponent component)
        {
            if (component.CanClick()) yield return SupportOperation.Click;
            if (component.CanTouchAndHold()) yield return SupportOperation.TouchAndHold;
        }

        internal static async UniTask DoOperation(InteractiveComponent component, MonkeyConfig config,
            CancellationToken cancellationToken = default)
        {
            var operations = GetCanOperations(component).ToArray();
            var operation = operations[config.Random.Next(operations.Length)];
            config.Logger.Log($"Do operation {component.gameObject.name} {operation.ToString()}");
            switch (operation)
            {
                case SupportOperation.Click:
                    component.Click();
                    break;
                case SupportOperation.TouchAndHold:
                    await component.TouchAndHold(config.TouchAndHoldDelayMillis, cancellationToken);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
