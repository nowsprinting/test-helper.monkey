// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [TestFixture]
    public class InteractiveComponentCollectorTest
    {
        /// <summary>
        /// InteractiveComponentCollector test cases using 3D objects
        /// </summary>
        [TestFixture]
        public class ThreeD
        {
            private static readonly string[] s_reachableObjects =
            {
                "UsingEventHandler", // Implements IPointerClickHandler
                "UsingEventTrigger", // Attached EventTrigger
                "ChildInTheSight", // Parent object is out of sight, but this object is in the sight
            };

            private static readonly string[] s_unreachableObjects =
            {
                "BeyondTheWall", // Beyond the another object
                "OutOfSight", // Out of sight
            };

            private static IEnumerable<string> s_interactiveObjects()
            {
                return s_reachableObjects.Concat(s_unreachableObjects);
            }

            [SetUp]
            public async Task SetUp()
            {
                await EditorSceneManager.LoadSceneAsyncInPlayMode(
                    "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyThreeD.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
            }

            [Test]
            public void FindInteractiveObjects_findAllInteractiveObjects()
            {
                var actual = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .Select(x => x.gameObject.name)
                    .ToArray();
                Assert.That(actual, Is.EquivalentTo(s_interactiveObjects()));
            }

            [Test]
            public void FindInteractiveObjects_reallyInteractiveOnly_findReachableObjects()
            {
                var actual = InteractiveComponentCollector.FindInteractiveComponents(true)
                    .Select(x => x.gameObject.name)
                    .ToArray();
                Assert.That(actual, Is.EquivalentTo(s_reachableObjects));
            }
        }

        /// <summary>
        /// InteractiveComponentCollector test cases using 2D objects
        /// </summary>
        [TestFixture]
        public class TwoD
        {
            private static readonly string[] s_reachableObjects =
            {
                "UsingEventHandler", // Implements IPointerClickHandler
                "UsingEventTrigger", // Attached EventTrigger
                "ChildInTheSight", // Parent object is out of sight, but this object is in the sight
            };

            private static readonly string[] s_unreachableObjects =
            {
                "BeyondTheWall", // Beyond the another object
                "OutOfSight", // Out of sight
            };

            private static IEnumerable<string> s_interactiveObjects()
            {
                return s_reachableObjects.Concat(s_unreachableObjects);
            }

            [SetUp]
            public async Task SetUp()
            {
                await EditorSceneManager.LoadSceneAsyncInPlayMode(
                    "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyTwoD.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
            }

            [Test]
            public void FindInteractiveObjects_findAllInteractiveObjects()
            {
                var actual = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .Select(x => x.gameObject.name)
                    .ToArray();
                Assert.That(actual, Is.EquivalentTo(s_interactiveObjects()));
            }

            [Test]
            public void FindInteractiveObjects_reallyInteractiveOnly_findReachableObjects()
            {
                var actual = InteractiveComponentCollector.FindInteractiveComponents(true)
                    .Select(x => x.gameObject.name)
                    .ToArray();
                Assert.That(actual, Is.EquivalentTo(s_reachableObjects));
            }
        }

        [TestFixture]
        public class UI
        {
            private static readonly string[] s_reachableUiObjects =
            {
                "Button", // Attached Button
                "ChildInTheSight", // Parent object is out of sight, but this object is in the sight
                "ButtonOnInnerCanvas", // On the inner Canvas
            };

            private static readonly string[] s_unreachableUiObjects =
            {
                "BeyondTheWall", // Beyond the another object
                "OutOfSight", // Out of sight
                "NotInteractable", // Interactable=false
                "BeyondThe2D", // Beyond the 2D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
                "BeyondThe3D", // Beyond the 3D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            };

            private static IEnumerable<string> s_interactiveUiObjects()
            {
                return s_reachableUiObjects.Concat(s_unreachableUiObjects);
            }

            /// <summary>
            /// InteractiveComponentCollector test cases using UI elements on screen-space-overlay canvas
            /// </summary>
            [TestFixture]
            public class ScreenSpaceOverlay
            {
                private static readonly string[] s_unreachableUiObjectsInOverlayCanvas =
                {
                    "BeyondTheWall", // Beyond the another object
                    "OutOfSight", // Out of sight
                    "NotInteractable", // Interactable=false
                };

                private static IEnumerable<string> s_interactiveUiObjectsInOverlayCanvas()
                {
                    return s_reachableUiObjects.Concat(s_unreachableUiObjectsInOverlayCanvas);
                }

                [SetUp]
                public async Task SetUp()
                {
                    await EditorSceneManager.LoadSceneAsyncInPlayMode(
                        "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyUiScreenSpaceOverlay.unity",
                        new LoadSceneParameters(LoadSceneMode.Single));
                    await UniTask.NextFrame(); // Wait 1 frame because warmup for GraphicRaycaster
                }

                [Test]
                public void FindInteractiveObjects_findAllInteractiveObjects()
                {
                    var actual = InteractiveComponentCollector.FindInteractiveComponents(false)
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_interactiveUiObjectsInOverlayCanvas()));
                }

                [Test]
                [Category("IgnoreCI")] // GraphicRaycaster not work on batchmode
                public void FindInteractiveObjects_reallyInteractiveOnly_findReachableObjects()
                {
                    var actual = InteractiveComponentCollector.FindInteractiveComponents(true)
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_reachableUiObjects));
                }
            }

            /// <summary>
            /// InteractiveComponentCollector test cases using UI elements on screen-space-camera canvas
            /// </summary>
            [TestFixture]
            public class ScreenSpaceCamera
            {
                [SetUp]
                public async Task SetUp()
                {
                    await EditorSceneManager.LoadSceneAsyncInPlayMode(
                        "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyUiScreenSpaceCamera.unity",
                        new LoadSceneParameters(LoadSceneMode.Single));
                    await UniTask.NextFrame(); // Wait 1 frame because warmup for GraphicRaycaster
                }

                [Test]
                public void FindInteractiveObjects_findAllInteractiveObjects()
                {
                    var actual = InteractiveComponentCollector.FindInteractiveComponents(false)
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_interactiveUiObjects()));
                }

                [Test]
                [Category("IgnoreCI")] // GraphicRaycaster not work on batchmode
                public void FindInteractiveObjects_reallyInteractiveOnly_findReachableObjects()
                {
                    var actual = InteractiveComponentCollector.FindInteractiveComponents(true)
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_reachableUiObjects));
                }
            }

            /// <summary>
            /// InteractiveComponentCollector test cases using UI elements on world space canvas
            /// </summary>
            public class WorldSpace
            {
                [SetUp]
                public async Task SetUp()
                {
                    await EditorSceneManager.LoadSceneAsyncInPlayMode(
                        "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyUiWorldSpace.unity",
                        new LoadSceneParameters(LoadSceneMode.Single));
                    await UniTask.NextFrame(); // Wait 1 frame because warmup for GraphicRaycaster
                }

                [Test]
                public void FindInteractiveObjects_findAllInteractiveObjects()
                {
                    var actual = InteractiveComponentCollector.FindInteractiveComponents(false)
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_interactiveUiObjects()));
                }

                [Test]
                [Category("IgnoreCI")] // GraphicRaycaster not work on batchmode
                public void FindInteractiveObjects_reallyInteractiveOnly_findReachableObjects()
                {
                    var actual = InteractiveComponentCollector.FindInteractiveComponents(true)
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_reachableUiObjects));
                }
            }
        }
    }
}
