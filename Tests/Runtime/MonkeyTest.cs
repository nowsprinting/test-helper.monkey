﻿// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.TestDoubles;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable MethodSupportsCancellation

namespace TestHelper.Monkey
{
    [TestFixture]
    public class MonkeyTest
    {
        private const string TestScene = "../Scenes/Operators.unity";

        private IEnumerable<IOperator> _operators;
        private InteractiveComponentCollector _interactiveComponentCollector;

        [SetUp]
        public void SetUp()
        {
            _operators = new IOperator[]
            {
                new UGUIClickOperator(), // click
                new UGUIClickAndHoldOperator(1), // click and hold 1ms
                new UGUITextInputOperator()
            };
            _interactiveComponentCollector = new InteractiveComponentCollector(operators: _operators);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_finish()
        {
            var config = new MonkeyConfig();
            var didAct = await Monkey.RunStep(
                config.Random,
                config.Logger,
                config.Screenshots,
                config.IsReachable,
                config.IsIgnored,
                _interactiveComponentCollector);

            Assert.That(didAct, Is.EqualTo(true));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_noInteractiveComponent_abort()
        {
            foreach (var component in _interactiveComponentCollector.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig();
            var didAct = await Monkey.RunStep(
                config.Random,
                config.Logger,
                config.Screenshots,
                config.IsReachable,
                config.IsIgnored,
                _interactiveComponentCollector);

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
                Operators = _operators,
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
        public async Task Run_noInteractiveComponent_throwTimeoutException()
        {
            foreach (var component in _interactiveComponentCollector.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(2), // 2sec
                SecondsToErrorForNoInteractiveComponent = 1, // 1sec
            };

            try
            {
                await Monkey.Run(config);
                Assert.Fail("TimeoutException was not thrown");
            }
            catch (TimeoutException e)
            {
                Assert.That(e.Message, Does.Contain("Interactive component not found in 1 seconds"));
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_noInteractiveComponentAndSecondsToErrorForNoInteractiveComponentIsZero_finish()
        {
            foreach (var component in _interactiveComponentCollector.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(2), // 2sec
                DelayMillis = 1, // 1ms
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
                Random = new RandomWrapper(0), // pin seed
                Logger = spyLogger,
                Operators = _operators,
            };

            await Monkey.Run(config);

            Assert.That(spyLogger.Messages, Does.Contain("Using RandomWrapper using System.Random, seed=0"));
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
                Gizmos = true, // show Gizmos
                Operators = _operators,
            };
            var task = Monkey.Run(config);
            await UniTask.Delay(1000, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(GameViewControlHelper.GetGizmos(), Is.False, "Reverted Gizmos");
        }

        [Test]
        [LoadScene(TestScene)]
        public void GetLotteryEntries_GotAllInteractableComponentAndOperators()
        {
            var lotteryEntries = Monkey.GetLotteryEntries(_interactiveComponentCollector, DefaultIgnoreStrategy.IsIgnored);
            var actual = new List<string>();
            foreach (var (component, @operator) in lotteryEntries)
            {
                actual.Add(
                    $"{component.gameObject.name}|{component.GetType().Name}|{@operator.GetType().Name}");
            }

            var expected = new List<string>
            {
                "UsingOnPointerClickHandler|SpyOnPointerClickHandler|UGUIClickOperator",
                "UsingPointerClickEventTrigger|EventTrigger|UGUIClickOperator",
                "UsingOnPointerDownUpHandler|SpyOnPointerDownUpHandler|UGUIClickAndHoldOperator",
                "UsingPointerDownUpEventTrigger|EventTrigger|UGUIClickAndHoldOperator",
                "UsingMultipleEventTriggers|EventTrigger|UGUIClickOperator",
                "UsingMultipleEventTriggers|EventTrigger|UGUIClickAndHoldOperator",
                "DestroyItselfIfPointerDown|StubDestroyingItselfWhenPointerDown|UGUIClickAndHoldOperator",
                "InputField|InputField|UGUIClickOperator",
                "InputField|InputField|UGUIClickAndHoldOperator",
                "InputField|InputField|UGUITextInputOperator",
            };

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        [LoadScene(TestScene)]
        public void GetLotteryEntries_WithIgnoreAnnotation_Excluded()
        {
            GameObject.Find("UsingOnPointerClickHandler").AddComponent<IgnoreAnnotation>();

            var lotteryEntries = Monkey.GetLotteryEntries(_interactiveComponentCollector, DefaultIgnoreStrategy.IsIgnored);
            var actual = new List<string>();
            foreach (var (component, _) in lotteryEntries)
            {
                actual.Add($"{component.gameObject.name}");
            }

            Assert.That(actual, Does.Not.Contain("UsingOnPointerClickHandler"));
        }

        [Test]
        public void LotteryOperator_NothingOperators_ReturnNull()
        {
            var operators = new List<(Component, IOperator)>();
            var random = new StubRandom(0);
            var actual = Monkey.LotteryOperator(operators, random, DefaultReachableStrategy.IsReachable);

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_NotReachableComponentOnly_ReturnNull()
        {
            var cube = GameObject.Find("Cube");
            var clickable = cube.AddComponent<SpyOnPointerClickHandler>();
            clickable.transform.position = new Vector3(0, 0, -20); // out of sight

            var operators = new List<(Component, IOperator)> { (clickable, new UGUIClickOperator()), };
            var random = new RandomWrapper();
            var actual = Monkey.LotteryOperator(operators, random, DefaultReachableStrategy.IsReachable);

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_BingoReachableComponent_ReturnOperator()
        {
            var cube = GameObject.Find("Cube");
            var clickable = cube.AddComponent<SpyOnPointerClickHandler>();
            var clickOperator = new UGUIClickOperator();

            var operators = new List<(Component, IOperator)>()
            {
                (null, null), // dummy
                (clickable, clickOperator),
                (null, null), // dummy
            };
            var random = new StubRandom(new int[] { 1 });
            var actual = Monkey.LotteryOperator(operators, random, DefaultReachableStrategy.IsReachable);

            Assert.That(actual.Item1, Is.EqualTo(clickable));
            Assert.That(actual.Item2, Is.EqualTo(clickOperator));
        }

        [TestFixture]
        [GameViewResolution(GameViewResolution.VGA)]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public class Screenshots
        {
            private IEnumerable<IOperator> _operators;
            private InteractiveComponentCollector _interactiveComponentCollector;

            private const int FileSizeThreshold = 5441; // VGA size solid color file size
            private const int FileSizeThreshold2X = 100 * 1024; // Normal size is 80 to 90KB
            private readonly string _defaultOutputDirectory = CommandLineArgs.GetScreenshotDirectory();
            private string _filename;
            private string _path;

            [SetUp]
            public void SetUp()
            {
                _operators = new IOperator[]
                {
                    new UGUIClickOperator(), // click
                    new UGUIClickAndHoldOperator(1), // click and hold 1ms
                    new UGUITextInputOperator()
                };
                _interactiveComponentCollector = new InteractiveComponentCollector(operators: _operators);

                _filename = $"{TestContext.CurrentContext.Test.Name}_0001.png";
                _path = Path.Combine(_defaultOutputDirectory, _filename);

                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task RunStep_withScreenshots_takeScreenshotsAndSaveToDefaultPath()
            {
                var config = new MonkeyConfig
                {
                    Screenshots = new ScreenshotOptions(), // take screenshots and save files,
                };

                await Monkey.RunStep(
                    config.Random,
                    config.Logger,
                    config.Screenshots,
                    config.IsReachable,
                    config.IsIgnored,
                    _interactiveComponentCollector);

                Assert.That(_path, Does.Exist);
                Assert.That(new FileInfo(_path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task RunStep_withScreenshots_specifyPath_takeScreenshotsAndSaveToSpecifiedPath()
            {
                var relativeDirectory = Path.Combine("Logs", "TestHelper.Monkey", "SpecifiedPath");
                var filenamePrefix = "Run_withScreenshots_specifyPath";
                var filename = $"{filenamePrefix}_0001.png";
                var path = Path.Combine(relativeDirectory, filename);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                var config = new MonkeyConfig
                {
                    Screenshots = new ScreenshotOptions
                    {
                        Directory = relativeDirectory,
                        FilenameStrategy = new StubScreenshotFilenameStrategy(filename),
                        SuperSize = 2,
                    },
                };

                await Monkey.RunStep(
                    config.Random,
                    config.Logger,
                    config.Screenshots,
                    config.IsReachable,
                    config.IsIgnored,
                    _interactiveComponentCollector);

                Assert.That(path, Does.Exist);
                Assert.That(new FileInfo(path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [Description("This test fails with stereo rendering settings.")]
            [LoadScene(TestScene)]
            public async Task RunStep_withScreenshots_superSize_takeScreenshotsSuperSize()
            {
                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                    DelayMillis = 1, // 1ms
                    Screenshots = new ScreenshotOptions()
                    {
                        SuperSize = 2, // 2x size
                    },
                };

                await Monkey.RunStep(
                    config.Random,
                    config.Logger,
                    config.Screenshots,
                    config.IsReachable,
                    config.IsIgnored,
                    _interactiveComponentCollector);

                Assert.That(_path, Does.Exist);
                Assert.That(new FileInfo(_path), Has.Length.GreaterThan(FileSizeThreshold2X));
                // Note: This test fails with stereo rendering settings.
                //  See: https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
            }

            [Test]
            [LoadScene(TestScene)]
            [Description("Is it a stereo screenshot? See for yourself! Be a witness!!")]
            public async Task RunStep_withScreenshots_stereo_takeScreenshotsStereo()
            {
                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                    DelayMillis = 1, // 1ms
                    Screenshots = new ScreenshotOptions()
                    {
                        StereoCaptureMode = ScreenCapture.StereoScreenCaptureMode.BothEyes,
                    },
                };

                await Monkey.RunStep(
                    config.Random,
                    config.Logger,
                    config.Screenshots,
                    config.IsReachable,
                    config.IsIgnored,
                    _interactiveComponentCollector);

                Assert.That(_path, Does.Exist);
                // Note: Require stereo rendering settings.
                //  See: https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task Run_withScreenshots_noInteractiveComponent_takeScreenshot()
            {
                var interactiveComponentCollector = new InteractiveComponentCollector();
                foreach (var component in interactiveComponentCollector.FindInteractableComponents())
                {
                    component.gameObject.SetActive(false);
                }

                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromSeconds(2), // 2sec
                    SecondsToErrorForNoInteractiveComponent = 1, // 1sec
                    Screenshots = new ScreenshotOptions() // take screenshots and save files
                };

                try
                {
                    await Monkey.Run(config);
                    Assert.Fail("TimeoutException was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Does.Contain($"Interactive component not found in 1 seconds ({_filename})"));
                }

                Assert.That(_path, Does.Exist);
            }
        }

        [TestFixture]
        public class Verbose
        {
            private IEnumerable<IOperator> _operators;
            private InteractiveComponentCollector _interactiveComponentCollector;

            [SetUp]
            public void SetUp()
            {
                _operators = new IOperator[]
                {
                    new UGUIClickOperator(), // click
                    new UGUIClickAndHoldOperator(1), // click and hold 1ms
                    new UGUITextInputOperator()
                };
                _interactiveComponentCollector = new InteractiveComponentCollector(operators: _operators);
            }

            [Test]
            [LoadScene(TestScene)]
            public void GetLotteryEntriesWithoutVerbose_NotOutputLog()
            {
                var lotteryEntries = Monkey.GetLotteryEntries(_interactiveComponentCollector, DefaultIgnoreStrategy.IsIgnored);
                Assume.That(lotteryEntries.Count, Is.GreaterThan(0));

                LogAssert.NoUnexpectedReceived();
            }

            [Test]
            [LoadScene(TestScene)]
            public void GetLotteryEntriesWithVerbose_LogLotteryEntries()
            {
                GameObject.Find("UsingOnPointerClickHandler").AddComponent<IgnoreAnnotation>();

                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(_interactiveComponentCollector, DefaultIgnoreStrategy.IsIgnored, verboseLogger: spyLogger);
                Assume.That(lotteryEntries.Count, Is.GreaterThan(0));

                Assert.That(spyLogger.Messages.Count, Is.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Does.StartWith("Lottery entries: "));
                Assert.That(spyLogger.Messages[0], Does.Match(@"\[Ignored\]UsingOnPointerClickHandler\(\d+\)"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingPointerClickEventTrigger\(\d+\)"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingOnPointerDownUpHandler\(\d+\)"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingPointerDownUpEventTrigger\(\d+\)"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingMultipleEventTriggers\(\d+\)"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"DestroyItselfIfPointerDown\(\d+\)"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\)"));
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")] // no interactable objects
            public void GetLotteryEntriesWithVerbose_NoInteractableObject_LogNoLotteryEntries()
            {
                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(_interactiveComponentCollector, DefaultIgnoreStrategy.IsIgnored, verboseLogger: spyLogger);
                Assume.That(lotteryEntries, Is.Empty);

                Assert.That(spyLogger.Messages.Count, Is.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Is.EqualTo("No lottery entries."));
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")] // no interactable and reachable objects
            public void LotteryOperatorWithVerbose_NotReachableComponentOnly_LogNoLotteryEntries()
            {
                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(_interactiveComponentCollector, DefaultIgnoreStrategy.IsIgnored).ToList();
                var random = new RandomWrapper();
                Monkey.LotteryOperator(lotteryEntries, random, DefaultReachableStrategy.IsReachable, spyLogger);

                Assert.That(spyLogger.Messages, Does.Contain("Lottery entries are empty or all of not reachable."));
            }
        }
    }
}
