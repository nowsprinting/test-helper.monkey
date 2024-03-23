// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;

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
            [LoadScene("Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity")]
            public async Task FindByNameAsync_Found(string target, bool reachable, bool interactable)
            {
                var actual = await _sut.FindByNameAsync(target, reachable, interactable);
                Assert.That(actual.name, Is.EqualTo(target));
            }

            [TestCase("NotActiveSelf")]
            [TestCase("NotActiveInHierarchy")]
            [LoadScene("Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity")]
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
            [LoadScene("Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity")]
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
            [LoadScene("Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity")]
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
            [LoadScene("Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity")]
            public async Task FindByPathAsync_Found(string path)
            {
                var actual = await _sut.FindByPathAsync(path, reachable: false, interactable: false);
                Assert.That(actual.name, Is.EqualTo("Interactable"));
            }

            [TestCase("/Parent/Child/Grandchild/Interactable")]
            [TestCase("/Canvas/Parent/Child/Interactable")]
            [TestCase("Interactable")]
            [LoadScene("Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/GameObjectFinderUI.unity")]
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

        // TODO: RenderMode.WorldSpaceで手前に2D/3Dオブジェクトがあるケース

        // TODO: 2Dオブジェクトのケース

        // TODO: 3Dオブジェクトのケース
    }
}
