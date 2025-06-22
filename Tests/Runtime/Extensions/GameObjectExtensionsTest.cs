// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
#if !UNITY_2022_1_OR_NEWER
using System.Linq;
using Cysharp.Threading.Tasks;
#endif

namespace TestHelper.Monkey.Extensions
{
    [TestFixture]
    public class GameObjectExtensionsTest
    {
        [TestFixture(RenderMode.ScreenSpaceOverlay)]
        [TestFixture(RenderMode.ScreenSpaceCamera)]
        [TestFixture(RenderMode.WorldSpace)]
        public class UI
        {
            private const string TestScenePath =
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity";

            private readonly GameObjectFinder _sut = new GameObjectFinder(0.1d);
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
                var result = await _sut.FindByNameAsync(target, reachable: false);
                Assert.That(result.GameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.True);
                // TODO: Remove when remove obsolete method, Already copied to DefaultReachableStrategyTest.
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_NotReachable(string target)
            {
                var result = await _sut.FindByNameAsync(target, reachable: false);
                Assert.That(result.GameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.False);
                // TODO: Remove when remove obsolete method, Already copied to DefaultReachableStrategyTest.
            }
        }

        [TestFixture("2D")]
        [TestFixture("3D")]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public class Object
        {
            private readonly GameObjectFinder _sut = new GameObjectFinder(0.1d);
            private readonly string _testScenePath;

            public Object(string dimension)
            {
                _testScenePath =
                    $"Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinder{dimension}.unity";
            }

            [SetUp]
            public async Task SetUp()
            {
#if UNITY_EDITOR
                await EditorSceneManager.LoadSceneAsyncInPlayMode(_testScenePath,
                    new LoadSceneParameters(LoadSceneMode.Single));
#endif
            }

            [TestCase("NotInteractable")]
            public async Task IsReachable_Reachable(string target)
            {
                var result = await _sut.FindByNameAsync(target, reachable: false);
                Assert.That(result.GameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.True);
                // TODO: Remove when remove obsolete method, Already copied to DefaultReachableStrategyTest.
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            public async Task IsReachable_NotReachable(string target)
            {
                var result = await _sut.FindByNameAsync(target, reachable: false);
                Assert.That(result.GameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.False);
                // TODO: Remove when remove obsolete method, Already copied to DefaultReachableStrategyTest.
            }
        }

        [Test]
        public void GetInteractableComponents_GotInteractableComponents()
        {
            var gameObject = new GameObject();
            var onPointerClickHandler = gameObject.AddComponent<SpyOnPointerClickHandler>();
            var onPointerDownUpHandler = gameObject.AddComponent<SpyOnPointerDownUpHandler>();
            gameObject.AddComponent<Image>(); // Not interactable

            var actual = gameObject.GetInteractableComponents();
            Assert.That(actual, Is.EquivalentTo(new Component[] { onPointerClickHandler, onPointerDownUpHandler }));
        }

        [Test]
        public void GetInteractableComponents_NoInteractableComponents_ReturnsEmpty()
        {
            var button = new GameObject().AddComponent<Button>();
            button.interactable = false;

            var actual = button.gameObject.GetInteractableComponents();
            Assert.That(actual, Is.Empty);
        }
    }
}
