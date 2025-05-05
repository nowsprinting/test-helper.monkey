// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    public class MonkeyTest
    {
        private const string MeasurePackageVersion = "0.13.3";

        [Test]
        [Performance, Version(MeasurePackageVersion)]
        [LoadScene("../Scenes/Operators.unity")]
        public void GetLotteryEntries_GotAllInteractableComponentAndOperators()
        {
            var config = new MonkeyConfig();
            var interactableComponentsFinder = new InteractableComponentsFinder(config);

            Measure.Method(() =>
                {
                    // ReSharper disable once IteratorMethodResultIsIgnored
                    Monkey.GetLotteryEntries(interactableComponentsFinder);
                })
                .WarmupCount(5)
                .MeasurementCount(20)
                .GC()
                .Run();
        }

        [Test]
        [Performance, Version(MeasurePackageVersion)]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_BingoReachableComponent()
        {
            var config = new MonkeyConfig();
            var interactableComponentsFinder = new InteractableComponentsFinder(config);
            var operators = Monkey.GetLotteryEntries(interactableComponentsFinder);

            Measure.Method(() =>
                {
                    Monkey.LotteryOperator(operators, config.Random, config.IgnoreStrategy, config.ReachableStrategy);
                })
                .WarmupCount(5)
                .MeasurementCount(20)
                .GC()
                .Run();
        }

        [UnityTest]
        [Performance, Version(MeasurePackageVersion)]
        [LoadScene("../Scenes/Operators.unity")]
        public IEnumerator RunStep_finish()
        {
            var config = new MonkeyConfig();
            var interactableComponentsFinder = new InteractableComponentsFinder(config);

            using (Measure.Frames().Scope())
            {
                yield return Monkey.RunStep(
                        config.Random,
                        config.Logger,
                        config.Screenshots,
                        interactableComponentsFinder,
                        config.IgnoreStrategy,
                        config.ReachableStrategy)
                    .ToCoroutine();
            }
        }
    }
}
