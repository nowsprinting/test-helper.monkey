// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq; // Do not remove, required for Unity 2022 or earlier
using System.Threading.Tasks;
using Cysharp.Threading.Tasks; // Do not remove, required for Unity 2022 or earlier
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.TestDoubles;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.DefaultStrategies
{
    [TestFixture]
    public class DefaultReachableStrategyTest
    {
        [TestFixture(RenderMode.ScreenSpaceOverlay)]
        [TestFixture(RenderMode.ScreenSpaceCamera)]
        [TestFixture(RenderMode.WorldSpace)]
        public class UI
        {
            private const string TestScenePath = "../../Scenes/GameObjectFinderUI.unity";
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);
            private readonly RenderMode _canvasRenderMode;

            public UI(RenderMode canvasRenderMode)
            {
                _canvasRenderMode = canvasRenderMode;
            }

            [SetUp]
            public void SetUp()
            {
                var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
                canvas.renderMode = _canvasRenderMode;
                if (_canvasRenderMode == RenderMode.WorldSpace)
                {
                    canvas.worldCamera = Camera.main;
                    canvas.transform.position = new Vector3(0, 0, 500);
                }
            }

            [TestCase("ActiveText")]
            [TestCase("Dialog")] // Child objects do not block raycast
            [LoadScene(TestScenePath)]
            public async Task IsReachable_Reachable(string target)
            {
                var gameObject = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(DefaultReachableStrategy.IsReachable(gameObject), Is.True);
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_NotReachable(string target)
            {
                var gameObject = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(DefaultReachableStrategy.IsReachable(gameObject), Is.False);
            }
        }

        [TestFixture("2D")]
        [TestFixture("3D")]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public class Object
        {
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);
            private readonly string _testScenePath;

            public Object(string dimension)
            {
                _testScenePath = $"../../Scenes/GameObjectFinder{dimension}.unity";
            }

            [SetUp]
            public async Task SetUp()
            {
                await SceneManagerHelper.LoadSceneAsync(_testScenePath);
            }

            [TestCase("NotInteractable")]
            public async Task IsReachable_Reachable(string target)
            {
                var gameObject = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(DefaultReachableStrategy.IsReachable(gameObject), Is.True);
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            public async Task IsReachable_NotReachable(string target)
            {
                var gameObject = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(DefaultReachableStrategy.IsReachable(gameObject), Is.False);
            }
        }

        [TestFixture]
        public class Verbose
        {
            private const string TestScenePath = "../../Scenes/GameObjectFinderUI.unity";
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);

            [TestCase("ActiveText")]
            [TestCase("Dialog")] // Child objects do not block raycast
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_Reachable_NotOutputLog(string target)
            {
                var gameObject = await _finder.FindByNameAsync(target, reachable: false);
                var spyLogger = new SpyLogger();
                var actual = DefaultReachableStrategy.IsReachable(gameObject, verboseLogger: spyLogger);
                Assume.That(actual, Is.True);

                Assert.That(spyLogger.Messages, Is.Empty);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_OutOfSight_LogVerboseNotHit()
            {
                var gameObject = await _finder.FindByNameAsync("OutOfSight", reachable: false);
                var spyLogger = new SpyLogger();
                var actual = DefaultReachableStrategy.IsReachable(gameObject, verboseLogger: spyLogger);
                Assume.That(actual, Is.False);

                Assert.That(spyLogger.Messages.Count, Is.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Does.Match(
                    @"Not reachable to OutOfSight\(\d+\), position=\(\d+,\d+\)\. Raycast is not hit\."));
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_BehindOtherObject_LogHitOtherObject()
            {
                var gameObject = await _finder.FindByNameAsync("BehindTheWall", reachable: false);
                var spyLogger = new SpyLogger();
                var actual = DefaultReachableStrategy.IsReachable(gameObject, verboseLogger: spyLogger);
                Assume.That(actual, Is.False);

                Assert.That(spyLogger.Messages.Count, Is.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Does.Match(
                    @"Not reachable to BehindTheWall\(\d+\), position=\(\d+,\d+\)\. Raycast hit other objects: Wall, BehindTheWall"));
            }
        }
    }
}
