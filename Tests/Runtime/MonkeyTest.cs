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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    [TestFixture]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    public class MonkeyTest
    {
        [SetUp]
        public async Task SetUp()
        {
            await EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
        }

        [Test]
        public async Task Run_finish()
        {
            var config = new MonkeyConfig
            {
                Lifetime = new TimeSpan(0, 0, 0, 1), // 1sec
            };
            var cancellationTokenSource = new CancellationTokenSource();

            await Monkey.Run(config, cancellationTokenSource.Token);
        }

        [Test]
        public async Task Run_cancel()
        {
            var config = new MonkeyConfig
            {
                Lifetime = new TimeSpan(0, 0, 0, 5), // 5sec
            };
            var cancellationTokenSource = new CancellationTokenSource();

            Monkey.Run(config, cancellationTokenSource.Token).Forget();

            await UniTask.Delay(1000, DelayType.DeltaTime, cancellationToken: cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
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
                Lifetime = new TimeSpan(0, 0, 0, 5), // 5sec
                SecondsToErrorForNoInteractiveComponent = 1, // 1sec
            };
            var cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Monkey.Run(config, cancellationTokenSource.Token);
                Assert.Fail();
            }
            catch (UnityEngine.Assertions.AssertionException e)
            {
                Assert.That(e.Message, Does.StartWith("Interactive component not found in 1 seconds"));
            }
        }

        [Test]
        public async Task Run_noInteractiveComponentAndSecondsToErrorForNoInteractiveComponentIsZero_finish()
        {
            foreach (var component in InteractiveComponentCollector.FindInteractiveComponents(true))
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = new TimeSpan(0, 0, 0, 2), // 2sec
                SecondsToErrorForNoInteractiveComponent = 0, // not detect error
            };
            var cancellationTokenSource = new CancellationTokenSource();

            await Monkey.Run(config, cancellationTokenSource.Token);
        }

        [Test]
        public async Task Run_usingConfigObjects()
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = new TimeSpan(0, 0, 0, 1), // 1sec
                Random = new RandomImpl(0), // fix seed
                Logger = spyLogger,
            };
            var cancellationTokenSource = new CancellationTokenSource();

            await Monkey.Run(config, cancellationTokenSource.Token);

            Assert.That(spyLogger.Messages, Does.Contain("Using System.Random, seed=0"));
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
            Assert.That(components.Count, Is.EqualTo(3)); // Removed not interactive objects.
        }

        [Test]
        public void Lottery_NoInteractiveComponent_returnNull()
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
            Assert.That(components.Count, Is.EqualTo(0)); // Removed not interactive objects.
        }

        [Test]
        public void Lottery_WithIgnoreAnnotation_returnNull()
        {
            var components = new List<InteractiveComponent>()
            {
                new InteractiveComponent(GameObject.Find("UsingMultipleEventTriggers").GetComponent<EventTrigger>()),
            };
            components[0].gameObject.AddComponent<IgnoreAnnotation>();

            var random = new StubRandom(0);
            var actual = Monkey.Lottery(ref components, random);

            Assert.That(actual, Is.Null);
            Assert.That(components.Count, Is.EqualTo(0)); // Removed not interactive objects.
        }

        private static object[][] s_componentAndOperations = new[]
        {
            new object[] { "UsingOnPointerClickHandler", 0, "Click" },
            new object[] { "UsingPointerClickEventTrigger", 0, "Click" },
            new object[] { "UsingOnPointerDownUpHandler", 0, "LongTap" },
            new object[] { "UsingPointerDownUpEventTrigger", 0, "LongTap" },
            new object[] { "UsingMultipleEventTriggers", 0, "Click" },
            new object[] { "UsingMultipleEventTriggers", 1, "LongTap" },
        };

        [TestCaseSource(nameof(s_componentAndOperations))]
        public async Task DoOperation_invokeOperationByLottery(string target, int index, string operation)
        {
            var component = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == target);
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                LongTapDelayMillis = 1, // 1ms
                Random = new StubRandom(index), // for lottery operation
                Logger = spyLogger,
            };

            await Monkey.DoOperation(component, config);

            Assert.That(spyLogger.Messages, Does.Contain($"Do operation {target} {operation}"));
        }

        [TestCaseSource(nameof(s_componentAndOperations))]
        public async Task Run_lotteryComponentsAndOperations(string target, int _, string operation)
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = new TimeSpan(0, 0, 0, 1), // 1sec
                DelayMillis = 1, // 1ms
                LongTapDelayMillis = 1, // 1ms
                Random = new RandomImpl(0), // fix seed
                Logger = spyLogger,
            };
            var cancellationTokenSource = new CancellationTokenSource();

            await Monkey.Run(config, cancellationTokenSource.Token);

            Assert.That(spyLogger.Messages, Does.Contain($"Do operation {target} {operation}"));
        }
    }
}
