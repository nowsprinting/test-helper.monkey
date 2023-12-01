// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.ScreenPointStrategies;
using TestHelper.Monkey.TestDoubles;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    [TestFixture]
    [GameViewResolution(GameViewResolution.VGA)]
    public class MonkeyTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_finish()
        {
            var config = new MonkeyConfig
            {
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
            };

            var didAct = await Monkey.RunStep(config, 0);
            Assert.That(didAct, Is.EqualTo(true));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_noInteractiveComponent_abort()
        {
            foreach (var component in InteractiveComponentCollector.FindInteractiveComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
            };

            var didAct = await Monkey.RunStep(config, 0);
            Assert.That(didAct, Is.EqualTo(false));
        }

        [Test]
        [LoadScene(TestScene)]
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
        [LoadScene(TestScene)]
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
        [LoadScene(TestScene)]
        public async Task Run_noInteractiveComponent_abort()
        {
            foreach (var component in InteractiveComponentCollector.FindInteractiveComponents())
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
        [LoadScene(TestScene)]
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
        [LoadScene(TestScene)]
        public async Task Run_usingConfigObjects()
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(1), // 1sec
                Random = new RandomImpl(0), // pin seed
                Logger = spyLogger,
            };

            await Monkey.Run(config);

            Assert.That(spyLogger.Messages, Does.Contain("Using Random using System.Random, seed=0"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Description("Shown Gizmos, See for yourself! Be a witness!!")]
        public async Task Run_withGizmos_showGizmosAndReverted()
        {
            Assume.That(GameViewControlHelper.GetGizmos(), Is.False);

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
                Gizmos = true, // show Gizmos
            };
            var task = Monkey.Run(config);
            await UniTask.Delay(1000, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(GameViewControlHelper.GetGizmos(), Is.False, "Reverted Gizmos");
        }

        [Test]
        [LoadScene(TestScene)]
        public void Lottery_hitInteractiveComponent_returnComponent()
        {
            var components = InteractiveComponentCollector
                .FindReallyInteractiveComponents(DefaultScreenPointStrategy.GetScreenPoint).ToList();
            for (var i = 0; i < components.Count; i++)
            {
                var random = new StubRandom(i);
                var expected = components[i];
                var actual = Monkey.Lottery(ref components, random, DefaultScreenPointStrategy.GetScreenPoint);

                Assert.That(actual.gameObject.name, Is.EqualTo(expected.gameObject.name));
            }
        }

        [Test]
        [LoadScene(TestScene)]
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
            var actual = Monkey.Lottery(ref components, random, DefaultScreenPointStrategy.GetScreenPoint);

            Assert.That(actual.gameObject.name, Is.EqualTo(expected.gameObject.name));
            Assert.That(components, Has.Count.EqualTo(3)); // Removed not interactive objects.
        }

        [Test]
        [LoadScene(TestScene)]
        public void Lottery_noInteractiveComponent_returnNull()
        {
            var components = new List<InteractiveComponent>()
            {
                new InteractiveComponent(
                    GameObject.Find("UsingOnPointerClickHandler").GetComponent<SpyOnPointerClickHandler>()),
            };
            components[0].gameObject.SetActive(false);

            var random = new StubRandom(0);
            var actual = Monkey.Lottery(ref components, random, DefaultScreenPointStrategy.GetScreenPoint);

            Assert.That(actual, Is.Null);
            Assert.That(components, Has.Count.EqualTo(0)); // Removed not interactive objects.
        }

        [Test]
        [LoadScene(TestScene)]
        public void Lottery_withIgnoreAnnotation_returnNull()
        {
            var components = new List<InteractiveComponent>()
            {
                new InteractiveComponent(
                    GameObject.Find("UsingMultipleEventTriggers").GetComponent<EventTrigger>()),
            };
            components[0].gameObject.AddComponent<IgnoreAnnotation>();

            var random = new StubRandom(0);
            var actual = Monkey.Lottery(ref components, random, DefaultScreenPointStrategy.GetScreenPoint);

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
        [LoadScene(TestScene)]
        public async Task DoOperation_invokeOperationByLottery(string target, int index, string operation)
        {
            var component = InteractiveComponentCollector.FindInteractiveComponents()
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
        [LoadScene(TestScene)]
        public async Task DoOperation_cancelDuringTouchAndHold_cancel()
        {
            const string Target = "UsingOnPointerDownUpHandler";
            const int Index = 0;
            const string Operation = "TouchAndHold";

            var component = InteractiveComponentCollector.FindInteractiveComponents()
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
        [LoadScene(TestScene)]
        public async Task Run_lotteryComponentsAndOperations(string target, int _, string operation)
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(10), // 10sec
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
                Random = new RandomImpl(0), // pin seed
                Logger = spyLogger,
            };

            await Monkey.Run(config);

            Assert.That(spyLogger.Messages, Does.Contain($"Do operation {target} {operation}"));
        }

        [TestFixture]
        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        public class Screenshots
        {
            private const int FileSizeThreshold = 5441; // VGA size solid color file size
            private const int FileSizeThreshold2X = 100 * 1024; // Normal size is 80 to 90KB

            [Test]
            [LoadScene(TestScene)]
            public async Task Run_withScreenshots_takeScreenshotsAndSaveToDefaultPath()
            {
                var path = Path.Combine(Application.persistentDataPath, "TestHelper.Monkey", "Screenshots",
                    $"{nameof(Run_withScreenshots_takeScreenshotsAndSaveToDefaultPath)}_0001.png");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                Assume.That(path, Does.Not.Exist);

                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                    DelayMillis = 1, // 1ms
                    TouchAndHoldDelayMillis = 1, // 1ms
                    Screenshots = new ScreenshotOptions(), // take screenshots and save files
                };
                await Monkey.Run(config);

                Assert.That(path, Does.Exist);
                Assert.That(new FileInfo(path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task Run_withScreenshots_specifyPath_takeScreenshotsAndSaveToSpecifiedPath()
            {
                var relativeDirectory = Path.Combine("Logs", "TestHelper.Monkey", "SpecifiedPath");
                var filenamePrefix = "Run_withScreenshots_specifyPath";
                var path = Path.Combine(relativeDirectory, $"{filenamePrefix}_0001.png");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                Assume.That(path, Does.Not.Exist);

                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                    DelayMillis = 1, // 1ms
                    TouchAndHoldDelayMillis = 1, // 1ms
                    Screenshots = new ScreenshotOptions()
                    {
                        Directory = relativeDirectory, // Relative path from project root when run in Editor
                        FilenamePrefix = filenamePrefix, // Prefix of filename
                        SuperSize = 2,
                    },
                };
                await Monkey.Run(config);

                Assert.That(path, Does.Exist);
                Assert.That(new FileInfo(path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [Description("This test fails with stereo rendering settings.")]
            [LoadScene(TestScene)]
            public async Task Run_withScreenshots_superSize_takeScreenshotsSuperSize()
            {
                var path = Path.Combine(Application.persistentDataPath, "TestHelper.Monkey", "Screenshots",
                    $"{nameof(Run_withScreenshots_superSize_takeScreenshotsSuperSize)}_0001.png");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                Assume.That(path, Does.Not.Exist);

                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                    DelayMillis = 1, // 1ms
                    TouchAndHoldDelayMillis = 1, // 1ms
                    Screenshots = new ScreenshotOptions()
                    {
                        SuperSize = 2, // 2x size
                    },
                };
                await Monkey.Run(config);

                Assert.That(path, Does.Exist);
                Assert.That(new FileInfo(path), Has.Length.GreaterThan(FileSizeThreshold2X));
                // Note: This test fails with stereo rendering settings.
                //  See: https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
            }

            [Test]
            [LoadScene(TestScene)]
            [Description("Is it a stereo screenshot? See for yourself! Be a witness!!")]
            public async Task Run_withScreenshots_stereo_takeScreenshotsStereo()
            {
                var path = Path.Combine(Application.persistentDataPath, "TestHelper.Monkey", "Screenshots",
                    $"{nameof(Run_withScreenshots_stereo_takeScreenshotsStereo)}_0001.png");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                Assume.That(path, Does.Not.Exist);

                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                    DelayMillis = 1, // 1ms
                    TouchAndHoldDelayMillis = 1, // 1ms
                    Screenshots = new ScreenshotOptions()
                    {
                        StereoCaptureMode = ScreenCapture.StereoScreenCaptureMode.BothEyes,
                    },
                };
                await Monkey.Run(config);

                Assert.That(path, Does.Exist);
                // Note: Require stereo rendering settings.
                //  See: https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
            }
        }
    }
}
