// Copyright (c) 2023-2025 Koji Hasegawa.
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
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Exceptions;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.TestDoubles;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    [TestFixture]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public class MonkeyTest
    {
        private const string TestScene = "../Scenes/Operators.unity";

        private IEnumerable<IOperator> _operators;
        private InteractableComponentsFinder _interactableComponentsFinder;

        [SetUp]
        public void SetUp()
        {
            _operators = new IOperator[]
            {
                new UGUIClickOperator(), // click
                new UGUIClickAndHoldOperator(1), // click and hold 1ms
                new UGUITextInputOperator()
            };
            _interactableComponentsFinder = new InteractableComponentsFinder(operators: _operators);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_finish()
        {
            var config = new MonkeyConfig();
            var (didAction, _) = await Monkey.RunStep(
                config.Random,
                config.Logger,
                config.Screenshots,
                _interactableComponentsFinder,
                config.IgnoreStrategy,
                config.ReachableStrategy);

            Assert.That(didAction, Is.EqualTo(true));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_noInteractiveComponent_DoNoAction()
        {
            // Make to no interactable objects
            foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig();
            var (didAction, _) = await Monkey.RunStep(
                config.Random,
                config.Logger,
                config.Screenshots,
                _interactableComponentsFinder,
                config.IgnoreStrategy,
                config.ReachableStrategy);

            Assert.That(didAction, Is.EqualTo(false));
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
            // Make to no interactable objects
            foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
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
            // Make to no interactable objects
            foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
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
            var lotteryEntries = Monkey.GetLotteryEntries(_interactableComponentsFinder);
            var actual = new List<string>();
            foreach (var (gameObject, @operator) in lotteryEntries)
            {
                actual.Add($"{gameObject.name}|{@operator.GetType().Name}");
            }

            var expected = new List<string>
            {
                "UsingOnPointerClickHandler|UGUIClickOperator",
                "UsingPointerClickEventTrigger|UGUIClickOperator",
                "UsingOnPointerDownUpHandler|UGUIClickAndHoldOperator",
                "UsingPointerDownUpEventTrigger|UGUIClickAndHoldOperator",
                "UsingMultipleEventTriggers|UGUIClickOperator",
                "UsingMultipleEventTriggers|UGUIClickAndHoldOperator",
                "DestroyItselfIfPointerDown|UGUIClickAndHoldOperator",
                "InputField|UGUIClickOperator",
                "InputField|UGUIClickAndHoldOperator",
                "InputField|UGUITextInputOperator",
            };

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        [LoadScene(TestScene)]
        public void GetLotteryEntries_NoOperators_ReturnsEmpty()
        {
            var notHasOperatorFinder = new InteractableComponentsFinder();
            var actual = Monkey.GetLotteryEntries(notHasOperatorFinder);

            Assert.That(actual, Is.Not.Null.And.Empty);
        }

        [Test]
        public void LotteryOperator_NothingOperators_ReturnNull()
        {
            var operators = Enumerable.Empty<(GameObject, IOperator)>();
            var random = new StubRandom(0);
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_IgnoredObjectOnly_ReturnNull()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<SpyOnPointerClickHandler>();
            cube.transform.position = new Vector3(0, 0, 0);
            cube.AddComponent<IgnoreAnnotation>(); // ignored

            var operators = new List<(GameObject, IOperator)> { (cube, new UGUIClickOperator()), };
            var random = new RandomWrapper();
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_NotReachableObjectOnly_ReturnNull()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<SpyOnPointerClickHandler>();
            cube.transform.position = new Vector3(0, 0, -20); // out of sight

            var operators = new List<(GameObject, IOperator)> { (cube, new UGUIClickOperator()), };
            var random = new RandomWrapper();
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_BingoReachableComponent_ReturnOperator()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<SpyOnPointerClickHandler>();
            var clickOperator = new UGUIClickOperator();

            var operators = new List<(GameObject, IOperator)>()
            {
                (null, null), // dummy
                (cube, clickOperator),
                (null, null), // dummy
            };
            var random = new StubRandom(new[] { 1 });
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.EqualTo(cube));
            Assert.That(actual.Item2, Is.EqualTo(clickOperator));
        }

        [TestFixture]
        [GameViewResolution(GameViewResolution.VGA)]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public class Screenshots
        {
            private IEnumerable<IOperator> _operators;
            private InteractableComponentsFinder _interactableComponentsFinder;

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
                _interactableComponentsFinder = new InteractableComponentsFinder(operators: _operators);

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
                    _interactableComponentsFinder,
                    config.IgnoreStrategy,
                    config.ReachableStrategy);

                Assert.That(_path, Does.Exist);
                Assert.That(new FileInfo(_path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task RunStep_withScreenshots_specifyPath_takeScreenshotsAndSaveToSpecifiedPath()
            {
                var relativeDirectory = Path.Combine(Application.temporaryCachePath,
                    TestContext.CurrentContext.Test.ClassName);
                if (Directory.Exists(relativeDirectory))
                {
                    Directory.Delete(relativeDirectory, true);
                }

                var filenamePrefix = TestContext.CurrentContext.Test.Name;
                var filename = $"{filenamePrefix}_0001.png";
                var path = Path.Combine(relativeDirectory, filename);

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
                    _interactableComponentsFinder,
                    config.IgnoreStrategy,
                    config.ReachableStrategy);

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
                    _interactableComponentsFinder,
                    config.IgnoreStrategy,
                    config.ReachableStrategy);

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
                    _interactableComponentsFinder,
                    config.IgnoreStrategy,
                    config.ReachableStrategy);

                Assert.That(_path, Does.Exist);
                // Note: Require stereo rendering settings.
                //  See: https://docs.unity3d.com/Manual/SinglePassStereoRendering.html
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task Run_withScreenshots_noInteractiveComponent_takeScreenshot()
            {
                // Make to no interactable objects
                foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
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
            private static InteractableComponentsFinder CreateInteractableComponentsFinder()
            {
                var operators = new IOperator[]
                {
                    new UGUIClickOperator(), // click
                    new UGUIClickAndHoldOperator(1), // click and hold 1ms
                    new UGUITextInputOperator()
                };
                return new InteractableComponentsFinder(operators: operators);
            }

            [Test]
            [LoadScene(TestScene)]
            public void GetLotteryEntries_NotOutputLog()
            {
                var lotteryEntries = Monkey.GetLotteryEntries(CreateInteractableComponentsFinder());
                Assume.That(lotteryEntries.Count, Is.GreaterThan(0));

                LogAssert.NoUnexpectedReceived();
            }

            [Test]
            [LoadScene(TestScene)]
            public void GetLotteryEntries_LogLotteryEntries()
            {
                GameObject.Find("UsingOnPointerClickHandler").AddComponent<IgnoreAnnotation>();

                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(CreateInteractableComponentsFinder(),
                    verboseLogger: spyLogger);
                Assume.That(lotteryEntries.Count, Is.GreaterThan(0));

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                // @formatter:off
                Assert.That(spyLogger.Messages[0], Does.StartWith("Lottery entries: "));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingPointerClickEventTrigger\(\d+\):EventTrigger:UGUIClickOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingOnPointerDownUpHandler\(\d+\):SpyOnPointerDownUpHandler:UGUIClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingPointerDownUpEventTrigger\(\d+\):EventTrigger:UGUIClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingOnPointerClickHandler\(\d+\):SpyOnPointerClickHandler:UGUIClickOperator")); // includes ignored objects
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingMultipleEventTriggers\(\d+\):EventTrigger:UGUIClickOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingMultipleEventTriggers\(\d+\):EventTrigger:UGUIClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"DestroyItselfIfPointerDown\(\d+\):StubDestroyingItselfWhenPointerDown:UGUIClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\):InputField:UGUIClickOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\):InputField:UGUIClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\):InputField:UGUITextInputOperator"));
                // @formatter:on
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")] // no interactable objects
            public void GetLotteryEntries_NoInteractableObject_LogNoLotteryEntries()
            {
                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(CreateInteractableComponentsFinder(),
                    verboseLogger: spyLogger);
                Assume.That(lotteryEntries, Is.Empty);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Is.EqualTo("No lottery entries."));
            }

            [Test]
            public void LotteryOperator_NothingOperators_LogNotLottery()
            {
                var operators = Enumerable.Empty<(GameObject, IOperator)>();
                var random = new StubRandom(0);
                var spyLogger = new SpyLogger();
                var ignoreStrategy = new DefaultIgnoreStrategy(verboseLogger: spyLogger);
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, spyLogger);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Is.EqualTo("Lottery entries are empty or all of not reachable."));
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
            public void LotteryOperator_IgnoredObjectOnly_LogNotLottery()
            {
                var cube = GameObject.Find("Cube");
                cube.AddComponent<SpyOnPointerClickHandler>();
                cube.transform.position = new Vector3(0, 0, 0);
                cube.AddComponent<IgnoreAnnotation>(); // ignored

                var operators = new List<(GameObject, IOperator)> { (cube, new UGUIClickOperator()), };
                var random = new RandomWrapper();
                var spyLogger = new SpyLogger();
                var ignoreStrategy = new DefaultIgnoreStrategy(verboseLogger: spyLogger);
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, spyLogger);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(2));
                Assert.That(spyLogger.Messages[0], Does.Match(@"Ignored Cube\(\d+\)."));
                Assert.That(spyLogger.Messages[1], Is.EqualTo("Lottery entries are empty or all of not reachable."));
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
            public void LotteryOperator_NotReachableObjectOnly_LogNotLottery()
            {
                var cube = GameObject.Find("Cube");
                cube.AddComponent<SpyOnPointerClickHandler>();
                cube.transform.position = new Vector3(0, 0, -20); // out of sight

                var operators = new List<(GameObject, IOperator)> { (cube, new UGUIClickOperator()), };
                var random = new RandomWrapper();
                var spyLogger = new SpyLogger();
                var ignoreStrategy = new DefaultIgnoreStrategy(verboseLogger: spyLogger);
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, spyLogger);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(2));
                Assert.That(spyLogger.Messages[0],
                    Does.Match(
                        @"Not reachable to Cube\(\d+\), position=\(\d+,\d+\), camera=Main Camera\(\d+\)\. Raycast is not hit\."));
                Assert.That(spyLogger.Messages[1], Is.EqualTo("Lottery entries are empty or all of not reachable."));
            }
        }

        [TestFixture]
        public class DetectingInfiniteLoop
        {
            [Test]
            [GameViewResolution(GameViewResolution.VGA)]
            [LoadScene("../Scenes/InfiniteLoop.unity")]
            public async Task Run_InfiniteLoop_throwsInfiniteLoopException()
            {
                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromSeconds(2), // 2sec
                    DelayMillis = 1, // 1ms
                    BufferLengthForDetectLooping = 10, // repeating 5-step sequences can be detected
                    Operators = new IOperator[] { new UGUIClickOperator() },
                };

                try
                {
                    await Monkey.Run(config);
                    Assert.Fail("InfiniteLoopException was not thrown");
                }
                catch (InfiniteLoopException)
                {
                    // pass
                }
            }

            [TestCase("1")] // under pattern length
            [TestCase("1, 1")] // under repeating length
            [TestCase("1, 1, 1")] // under repeating length
            [TestCase("1, 2, 3, 1, 2")] // not looping yet
            public void DetectInfiniteLoop_NotRepeatingSequence_ReturnsFalse(string commaSeparatedSequence)
                // Note: If a parameter type is `int[]`, all test names will contain `System.Int32[]` will be indistinguishable, so pass it as a comma-separated string and parse it.
            {
                var sequence = new List<int>(commaSeparatedSequence.Split(',').Select(int.Parse));
                Assert.That(Monkey.DetectInfiniteLoop(ref sequence), Is.False);
            }

            [TestCase("1, 1, 1, 1")] // pattern (1, 1) is repeated twice
            [TestCase("1, 2, 1, 2")]
            [TestCase("1, 2, 3, 1, 2, 3")]
            public void DetectInfiniteLoop_RepeatingSequence_ReturnsTrue(string commaSeparatedSequence)
                // Note: If a parameter type is `int[]`, all test names will contain `System.Int32[]` will be indistinguishable, so pass it as a comma-separated string and parse it.
            {
                var sequence = new List<int>(commaSeparatedSequence.Split(',').Select(int.Parse));
                Assert.That(Monkey.DetectInfiniteLoop(ref sequence), Is.True);
            }
        }
    }
}
