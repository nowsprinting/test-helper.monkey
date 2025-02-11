// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;

namespace TestHelper.Monkey
{
    [TestFixture]
    public class InteractableComponentsFinderTest
    {
        /// <summary>
        /// InteractableComponentsFinder test cases using 3D objects
        /// </summary>
        [TestFixture]
        public class ThreeD
        {
            private const string TestScene = "../Scenes/MonkeyThreeD.unity";

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

            [Test]
            [LoadScene(TestScene)]
            public void FindInteractiveObjects_findAllInteractiveObjects()
            {
                var actual = new InteractableComponentsFinder().FindInteractableComponents()
                    .Select(x => x.gameObject.name)
                    .ToArray();
                Assert.That(actual, Is.EquivalentTo(s_interactiveObjects()));
            }
        }

        /// <summary>
        /// InteractableComponentsFinder test cases using 2D objects
        /// </summary>
        [TestFixture]
        public class TwoD
        {
            private const string TestScene = "../Scenes/MonkeyTwoD.unity";

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

            [Test]
            [LoadScene(TestScene)]
            public void FindInteractiveObjects_findAllInteractiveObjects()
            {
                var actual = new InteractableComponentsFinder().FindInteractableComponents()
                    .Select(x => x.gameObject.name)
                    .ToArray();
                Assert.That(actual, Is.EquivalentTo(s_interactiveObjects()));
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
                "BeyondThe2D", // Beyond the 2D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
                "BeyondThe3D", // Beyond the 3D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            };

            private static IEnumerable<string> s_interactiveUiObjects()
            {
                return s_reachableUiObjects.Concat(s_unreachableUiObjects);
            }

            /// <summary>
            /// InteractableComponentsFinder test cases using UI elements on screen-space-overlay canvas
            /// </summary>
            [TestFixture]
            public class ScreenSpaceOverlay
            {
                private const string TestScene = "../Scenes/MonkeyUiScreenSpaceOverlay.unity";

                private static readonly string[] s_unreachableUiObjectsInOverlayCanvas =
                {
                    "BeyondTheWall", // Beyond the another object
                    "OutOfSight", // Out of sight
                };

                private static IEnumerable<string> s_interactiveUiObjectsInOverlayCanvas()
                {
                    return s_reachableUiObjects.Concat(s_unreachableUiObjectsInOverlayCanvas);
                }

                [Test]
                [LoadScene(TestScene)]
                public void FindInteractiveObjects_findAllInteractiveObjects()
                {
                    var actual = new InteractableComponentsFinder().FindInteractableComponents()
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_interactiveUiObjectsInOverlayCanvas()));
                }
            }

            /// <summary>
            /// InteractableComponentsFinder test cases using UI elements on screen-space-camera canvas
            /// </summary>
            [TestFixture]
            public class ScreenSpaceCamera
            {
                private const string TestScene = "../Scenes/MonkeyUiScreenSpaceCamera.unity";

                [Test]
                [LoadScene(TestScene)]
                public void FindInteractiveObjects_findAllInteractiveObjects()
                {
                    var actual = new InteractableComponentsFinder().FindInteractableComponents()
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_interactiveUiObjects()));
                }
            }

            /// <summary>
            /// InteractableComponentsFinder test cases using UI elements on world space canvas
            /// </summary>
            [TestFixture]
            public class WorldSpace
            {
                private const string TestScene = "../Scenes/MonkeyUiWorldSpace.unity";

                [Test]
                [LoadScene(TestScene)]
                public void FindInteractiveObjects_findAllInteractiveObjects()
                {
                    var actual = new InteractableComponentsFinder().FindInteractableComponents()
                        .Select(x => x.gameObject.name)
                        .ToArray();
                    Assert.That(actual, Is.EquivalentTo(s_interactiveUiObjects()));
                }
            }
        }
    }
}
