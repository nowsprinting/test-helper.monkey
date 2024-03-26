// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TestHelper.Monkey
{
    [TestFixture]
    public class GameObjectFinderTest
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

            [TestCase("OutOfSight", false, false)]
            [TestCase("BehindTheWall", false, false)]
            [TestCase("ActiveText", false, false)]
            [TestCase("ActiveText", true, false)]
            [TestCase("Dialog", false, false)]
            [TestCase("Dialog", true, false)] // Child objects do not block raycast
            [TestCase("NotInteractable", false, false)]
            [TestCase("NotInteractable", true, false)]
            [TestCase("Interactable", false, false)]
            [TestCase("Interactable", true, false)]
            [TestCase("Interactable", false, true)]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_Found(string target, bool reachable, bool interactable)
            {
                var actual = await _sut.FindByNameAsync(target, reachable, interactable);
                Assert.That(actual.name, Is.EqualTo(target));
            }

            [TestCase("NotActiveSelf")]
            [TestCase("NotActiveInHierarchy")]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_NotFound(string target)
            {
                try
                {
                    await _sut.FindByNameAsync(target, reachable: false, interactable: false);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo($"GameObject `{target}` is not found."));
                }
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_NotReachable(string target)
            {
                try
                {
                    await _sut.FindByNameAsync(target, reachable: true, interactable: false);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo($"GameObject `{target}` is found, but not reachable."));
                }
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            [TestCase("ActiveText")]
            [TestCase("Dialog")]
            [TestCase("NotInteractable")]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_NotInteractable(string target)
            {
                try
                {
                    await _sut.FindByNameAsync(target, reachable: false, interactable: true);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo($"GameObject `{target}` is found, but not interactable."));
                }
            }

            [TestCase("/Canvas/Parent/Child/Grandchild/Interactable")]
            [TestCase("/Canvas/Parent/Child/?ran?child/Interactable")]
            [TestCase("/Canvas/*/Child/Grandchild/Interactable")]
            [TestCase("/Canvas/Parent/**/Interactable")]
            [TestCase("**/Interactable")]
            [LoadScene(TestScenePath)]
            public async Task FindByPathAsync_Found(string path)
            {
                var actual = await _sut.FindByPathAsync(path, reachable: false, interactable: false);
                Assert.That(actual.name, Is.EqualTo("Interactable"));
            }

            [TestCase("/Parent/Child/Grandchild/Interactable")]
            [TestCase("/Canvas/Parent/Child/Interactable")]
            [TestCase("Interactable")]
            [LoadScene(TestScenePath)]
            public async Task FindByPathAsync_NotMatchPath(string path)
            {
                try
                {
                    await _sut.FindByPathAsync(path, reachable: false, interactable: false);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message,
                        Is.EqualTo($"GameObject `Interactable` is found, but it does not match path `{path}`."));
                }
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

            [TestCase("OutOfSight", false, false)]
            [TestCase("BehindTheWall", false, false)]
            [TestCase("NotInteractable", false, false)]
            [TestCase("NotInteractable", true, false)]
            [TestCase("EventHandler", false, false)]
            [TestCase("EventHandler", true, false)]
            [TestCase("EventHandler", false, true)]
            [TestCase("EventTrigger", false, false)]
            [TestCase("EventTrigger", true, false)]
            [TestCase("EventTrigger", false, true)]
            public async Task FindByNameAsync_Found(string target, bool reachable, bool interactable)
            {
                var actual = await _sut.FindByNameAsync(target, reachable, interactable);
                Assert.That(actual.name, Is.EqualTo(target));
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            public async Task FindByNameAsync_NotReachable(string target)
            {
                try
                {
                    await _sut.FindByNameAsync(target, reachable: true, interactable: false);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo($"GameObject `{target}` is found, but not reachable."));
                }
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            [TestCase("NotInteractable")]
            public async Task FindByNameAsync_NotInteractable(string target)
            {
                try
                {
                    await _sut.FindByNameAsync(target, reachable: false, interactable: true);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo($"GameObject `{target}` is found, but not interactable."));
                }
            }
        }
    }
}
