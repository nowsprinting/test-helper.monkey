// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Random;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [TestFixture]
    public class MonkeyTest
    {
        [SetUp]
        public async Task SetUp()
        {
#if UNITY_EDITOR
            await UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
        }

        [Test]
        public async Task Run_finish()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
            };
            var task = Monkey.Run(config);
            await UniTask.Delay(1000, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public async Task Run_cancel()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.MaxValue, // Test that it does not overflow
            };
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var task = Monkey.Run(config, cancellationTokenSource.Token);
                await UniTask.Delay(1000, DelayType.DeltaTime);

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            }
        }

        [Test]
        public async Task Run_noInteractiveComponent_abort()
        {
            foreach (var component in InteractiveComponentCollector.FindInteractiveComponents(false))
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(5), // 5sec
                SecondsToErrorForNoInteractiveComponent = 1, // 1sec
            };

            try
            {
                await Monkey.Run(config);
                Assert.Fail("AssertionException was not thrown");
            }
            catch (UnityEngine.Assertions.AssertionException e)
            {
                Assert.That(e.Message, Does.Contain("Interactive component not found in 1 seconds"));
            }
        }

        [Test]
        public async Task Run_noInteractiveComponentAndSecondsToErrorForNoInteractiveComponentIsZero_finish()
        {
            foreach (var component in InteractiveComponentCollector.FindInteractiveComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(2), // 2sec
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
                SecondsToErrorForNoInteractiveComponent = 0, // not detect error
            };

            var task = Monkey.Run(config);
            await UniTask.Delay(2200, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public async Task Run_usingConfigObjects()
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(1), // 1sec
                Random = new RandomImpl(0), // fix seed
                Logger = spyLogger,
            };

            await Monkey.Run(config);

            Assert.That(spyLogger.Messages, Does.Contain("Using wrapping System.Random, seed=0"));
        }

        [Test]
        public void Lottery_hitInteractiveComponent_returnComponent()
        {
            var components = InteractiveComponentCollector.FindInteractiveComponents(false).ToList();
            for (var i = 0; i < components.Count; i++)
            {
                var random = new StubRandom(i);
                var expected = components[i];
                var actual = Monkey.Lottery(ref components, random);

                Assert.That(actual.gameObject.name, Is.EqualTo(expected.gameObject.name));
            }
        }

        [Test]
        public void Lottery_hitNotInteractiveComponent_returnNextLotteryComponent()
        {
            var components = new List<InteractiveComponent>()
            {
                new InteractiveComponent(
                    GameObject.Find("UsingOnPointerClickHandler").GetComponent<SpyOnPointerClickHandler>()),
                new InteractiveComponent(
                    GameObject.Find("UsingPointerClickEventTrigger").GetComponent<EventTrigger>()),
                new InteractiveComponent(
                    GameObject.Find("UsingOnPointerDownUpHandler").GetComponent<SpyOnPointerDownUpHandler>()),
                new InteractiveComponent(
                    GameObject.Find("UsingPointerDownUpEventTrigger").GetComponent<EventTrigger>()),
            };
            components[0].gameObject.SetActive(false);

            var random = new StubRandom(0, 1);
            var expected = components[2];
            var actual = Monkey.Lottery(ref components, random);

            Assert.That(actual.gameObject.name, Is.EqualTo(expected.gameObject.name));
            Assert.That(components, Has.Count.EqualTo(3)); // Removed not interactive objects.
        }

        [Test]
        public void Lottery_noInteractiveComponent_returnNull()
        {
            var components = new List<InteractiveComponent>()
            {
                new InteractiveComponent(
                    GameObject.Find("UsingOnPointerClickHandler").GetComponent<SpyOnPointerClickHandler>()),
            };
            components[0].gameObject.SetActive(false);

            var random = new StubRandom(0);
            var actual = Monkey.Lottery(ref components, random);

            Assert.That(actual, Is.Null);
            Assert.That(components, Has.Count.EqualTo(0)); // Removed not interactive objects.
        }

        [Test]
        public void Lottery_withIgnoreAnnotation_returnNull()
        {
            var components = new List<InteractiveComponent>()
            {
                new InteractiveComponent(
                    GameObject.Find("UsingMultipleEventTriggers").GetComponent<EventTrigger>()),
            };
            components[0].gameObject.AddComponent<IgnoreAnnotation>();

            var random = new StubRandom(0);
            var actual = Monkey.Lottery(ref components, random);

            Assert.That(actual, Is.Null);
            Assert.That(components, Has.Count.EqualTo(0)); // Removed not interactive objects.
        }

        private static object[][] s_componentAndOperations = new[]
        {
            new object[] { "UsingOnPointerClickHandler", 0, "Click" },
            new object[] { "UsingPointerClickEventTrigger", 0, "Click" },
            new object[] { "UsingOnPointerDownUpHandler", 0, "TouchAndHold" },
            new object[] { "UsingPointerDownUpEventTrigger", 0, "TouchAndHold" },
            new object[] { "UsingMultipleEventTriggers", 0, "Click" },
            new object[] { "UsingMultipleEventTriggers", 1, "TouchAndHold" },
        };

        [TestCaseSource(nameof(s_componentAndOperations))]
        public async Task DoOperation_invokeOperationByLottery(string target, int index, string operation)
        {
            var component = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == target);
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                TouchAndHoldDelayMillis = 1, // 1ms
                Random = new StubRandom(index), // for lottery operation
                Logger = spyLogger,
            };

            await Monkey.DoOperation(component, config);

            Assert.That(spyLogger.Messages, Does.Contain($"Do operation {target} {operation}"));
        }

        [Test]
        public async Task DoOperation_cancelDuringTouchAndHold_cancel()
        {
            const string Target = "UsingOnPointerDownUpHandler";
            const int Index = 0;
            const string Operation = "TouchAndHold";

            var component = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == Target);
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                TouchAndHoldDelayMillis = 1000, // 1sec
                Random = new StubRandom(Index), // for lottery operation
                Logger = spyLogger,
            };
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var task = Monkey.DoOperation(component, config, cancellationTokenSource.Token);
                await UniTask.Delay(100, DelayType.DeltaTime);

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
                Assert.That(spyLogger.Messages, Does.Contain($"Do operation {Target} {Operation}"));
            }
        }

        [TestCaseSource(nameof(s_componentAndOperations))]
        [Category("IgnoreCI")] // Ignore on CI due to low fps
        public async Task Run_lotteryComponentsAndOperations(string target, int _, string operation)
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(10), // 10sec
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
                Random = new RandomImpl(0), // fix seed
                Logger = spyLogger,
            };

            await Monkey.Run(config);

            Assert.That(spyLogger.Messages, Does.Contain($"Do operation {target} {operation}"));
        }
    }
}
