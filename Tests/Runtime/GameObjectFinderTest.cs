// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Exceptions;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.GameObjectMatchers;
using TestHelper.Monkey.Paginators;
using TestHelper.Monkey.TestDoubles;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if !UNITY_2022_1_OR_NEWER
using System.IO;
#endif

#if !UNITY_2023_1_OR_NEWER
using Cysharp.Threading.Tasks;
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
            private const string TestScenePath = "../Scenes/GameObjectFinderUI.unity";

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
                var result = await _sut.FindByNameAsync(target, reachable, interactable);
                Assert.That(result.GameObject.name, Is.EqualTo(target));
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
                    Assert.That(e.Message, Is.EqualTo($"GameObject (name={target}) is not found."));
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
                    Assert.That(e.Message, Is.EqualTo($"GameObject (name={target}) is found, but not reachable."));
                }
            }

            [TestCase("OutOfSight")]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_NotReachableWithVerbose(string target)
            {
                var spyLogger = new SpyLogger();
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                var sut = new GameObjectFinder(0.1d, reachableStrategy);

                try
                {
                    await sut.FindByNameAsync(target, reachable: true, interactable: false);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Is.EqualTo($"GameObject (name={target}) is found, but not reachable."));
                }

                Assert.That(spyLogger.Messages, Is.Not.Empty);
                Assert.That(spyLogger.Messages[0],
                    Does.Match(@"Not reachable to OutOfSight\(\d+\), position=\(\d+,\d+\).*\. Raycast is not hit\."));
                // Note: with or without camera
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
                    Assert.That(e.Message, Is.EqualTo($"GameObject (name={target}) is found, but not interactable."));
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
                var result = await _sut.FindByPathAsync(path, reachable: false, interactable: false);
                Assert.That(result.GameObject.transform.GetPath(),
                    Is.EqualTo("/Canvas/Parent/Child/Grandchild/Interactable"));
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
                        Is.EqualTo($"GameObject (path={path}) is not found."));
                }
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_MultipleGameObjectsMatching_OnlyOneReachable_Found()
            {
                // Move to out of sight
                ((RectTransform)GameObject.Find("ButtonOnDialog").transform).pivot = new Vector2(0, 300f);
                ((RectTransform)GameObject.Find("Interactable").transform).pivot = new Vector2(0, 300f);

                var matcher = new ButtonMatcher(); // Tree buttons match, one is not reachable
                var result = await _sut.FindByMatcherAsync(matcher, reachable: true, interactable: false);
                Assert.That(result.GameObject.transform.GetPath(),
                    Is.EqualTo("/Canvas/NotInteractable"));
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_MultipleGameObjectsMatching_OnlyOneInteractable_Found()
            {
                var matcher = new ButtonMatcher(path: "**/*Interactable"); // Two buttons match, one is not interactable
                var result = await _sut.FindByMatcherAsync(matcher, reachable: false, interactable: true);
                Assert.That(result.GameObject.transform.GetPath(),
                    Is.EqualTo("/Canvas/Parent/Child/Grandchild/Interactable"));
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_MultipleGameObjectsMatching_ThrowException()
            {
                var matcher = new ButtonMatcher(); // Three buttons match
                try
                {
                    await _sut.FindByMatcherAsync(matcher, reachable: false, interactable: false);
                    Assert.Fail("Expected MultipleGameObjectsMatchingException but was not thrown");
                }
                catch (MultipleGameObjectsMatchingException e)
                {
                    Assert.That(e.Message,
                        Is.EqualTo($"Multiple GameObjects matching the condition ({matcher}) were found."));
                }
            }

            [TestCase("Text under the Button", "/Canvas/Button (Legacy)")]
            [TestCase("TMP Text under the Button", "/Canvas/Button")]
            [LoadScene("../Scenes/GameObjectFinderText.unity")]
            public async Task FindByMatcherAsync_TextInButton_Found(string text, string expectedPath)
            {
                var matcher = new ButtonMatcher(text: text);
                var result = await _sut.FindByMatcherAsync(matcher, reachable: false, interactable: false);
                Assert.That(result.GameObject.transform.GetPath(), Is.EqualTo(expectedPath));
            }

            [TestCase("Text under the Toggle", "/Canvas/Toggle")]
            [LoadScene("../Scenes/GameObjectFinderText.unity")]
            public async Task FindByMatcherAsync_TextInToggle_Found(string text, string expectedPath)
            {
                var matcher = new ToggleMatcher(text: text);
                var result = await _sut.FindByMatcherAsync(matcher, reachable: false, interactable: false);
                Assert.That(result.GameObject.transform.GetPath(), Is.EqualTo(expectedPath));
            }
        }

        [TestFixture("2D")]
        [TestFixture("3D")]
        [BuildScene("../Scenes/GameObjectFinder2D.unity")]
        [BuildScene("../Scenes/GameObjectFinder3D.unity")]
        public class Object
        {
            private readonly GameObjectFinder _sut = new GameObjectFinder(0.1d);
            private readonly string _testScenePath;

            public Object(string dimension)
            {
                _testScenePath = $"../Scenes/GameObjectFinder{dimension}.unity";
            }

            [SetUp]
            public async Task SetUp()
            {
                await SceneManagerHelper.LoadSceneAsync(_testScenePath);
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
                var result = await _sut.FindByNameAsync(target, reachable, interactable);
                Assert.That(result.GameObject.name, Is.EqualTo(target));
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
                    Assert.That(e.Message, Is.EqualTo($"GameObject (name={target}) is found, but not reachable."));
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
                    Assert.That(e.Message, Is.EqualTo($"GameObject (name={target}) is found, but not interactable."));
                }
            }
        }

        [TestFixture]
        public class MultipleScene
        {
            private const string TestScenePath = "../Scenes/GameObjectFinder2D.unity";

            private readonly GameObjectFinder _sut = new GameObjectFinder(0.1d);

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_DontDestroyOnLoad_Found()
            {
                var target = GameObject.Find("EventHandler");

                GameObject.DontDestroyOnLoad(target);

                var result = await _sut.FindByNameAsync(target.name);
                Assert.That(result.GameObject, Is.EqualTo(target));

                // Teardown
                GameObject.Destroy(target);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByNameAsync_OnInactiveScene_Found()
            {
                var target = GameObject.Find("EventHandler");

                var newScene = SceneManager.CreateScene("NewScene");
                SceneManager.SetActiveScene(newScene);

                var result = await _sut.FindByNameAsync(target.name);
                Assert.That(result.GameObject, Is.EqualTo(target));
            }
        }

        [TestFixture]
        public class InPaginator
        {
            private const string TestScenePath = "../Scenes/ScrollViews.unity";

            private readonly GameObjectFinder _sut = new GameObjectFinder(2.0d);

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_PaginatorAndObjectExists_ReturnsGameObjectFinderResult()
            {
                var scrollView = GameObject.Find("Vertical Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();
                var paginator = new UguiScrollRectPaginator(scrollRect);
                var matcher = new NameMatcher("Vertical_Button_10");

                var result = await _sut.FindByMatcherAsync(matcher, paginator: paginator);

                Assert.That(result.GameObject.name, Is.EqualTo("Vertical_Button_10"));
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_CancellationTokenCancelled_ThrowsOperationCanceledException()
            {
                var scrollView = GameObject.Find("Vertical Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();
                var paginator = new UguiScrollRectPaginator(scrollRect);
                var matcher = new NameMatcher("NotExistingObject");

                var cts = new CancellationTokenSource();
                cts.CancelAfter(100);

                try
                {
                    await _sut.FindByMatcherAsync(matcher, paginator: paginator, cancellationToken: cts.Token);
                    Assert.Fail("Expected exception but was not thrown");
                }
                catch (OperationCanceledException)
                {
                    Assert.Pass();
                }
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_ObjectInFirstPage_ReturnsGameObject()
            {
                var scrollView = GameObject.Find("Vertical Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();
                var paginator = new UguiScrollRectPaginator(scrollRect);
                var matcher = new NameMatcher("Vertical_Button_1");

                var result = await _sut.FindByMatcherAsync(matcher, paginator: paginator);

                Assert.That(result.GameObject.name, Is.EqualTo("Vertical_Button_1"));
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_ObjectInLastPage_ReturnsGameObject()
            {
                var scrollView = GameObject.Find("Vertical Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();
                var paginator = new UguiScrollRectPaginator(scrollRect);
                var matcher = new NameMatcher("Vertical_Button_29");

                var result = await _sut.FindByMatcherAsync(matcher, paginator: paginator);

                Assert.That(result.GameObject.name, Is.EqualTo("Vertical_Button_29"));
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_ObjectNotFound_ThrowsTimeoutException()
            {
                var scrollView = GameObject.Find("Vertical Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();
                var paginator = new UguiScrollRectPaginator(scrollRect);
                var matcher = new NameMatcher("NotExistingObject");

                try
                {
                    await _sut.FindByMatcherAsync(matcher, paginator: paginator);
                    Assert.Fail("Expected exception but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Does.EndWith("is not found."));
                }
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task FindByMatcherAsync_SomePageReturnsNotReachable_SavesReasonAndReturns()
            {
                var scrollView = GameObject.Find("Vertical Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();
                var paginator = new UguiScrollRectPaginator(scrollRect);
                var matcher = new NameMatcher("Vertical_Button_10");

                // Turn off raycast target to make it unreachable
                var targetButton = GameObject.Find("Vertical_Button_10");
                targetButton.GetComponent<Image>().raycastTarget = false;
                targetButton.GetComponentInChildren<Text>().raycastTarget = false;

                try
                {
                    await _sut.FindByMatcherAsync(matcher, reachable: true, paginator: paginator);
                    Assert.Fail("Expected exception but was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Does.EndWith("is found, but not reachable."));
                }
            }
        }
    }
}
