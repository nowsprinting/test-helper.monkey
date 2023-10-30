// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    [TestFixture]
    public class InteractiveComponentTest
    {
        /// <summary>
        /// InteractiveComponent test cases using 3D objects
        /// </summary>
        [TestFixture]
        [GameViewResolution(GameViewResolution.VGA)]
        public class ThreeD
        {
            private const string TestScene =
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyThreeD.unity";

            [Test]
            [LoadScene(TestScene)]
            public void Fields()
            {
                var gameObject = GameObject.Find("UsingEventTrigger");
                var component = gameObject.GetComponent<EventTrigger>();
                var sut = new InteractiveComponent(component);

                Assert.That(sut.component, Is.EqualTo(component));
                Assert.That(sut.gameObject, Is.EqualTo(gameObject));
                Assert.That(sut.transform, Is.EqualTo(gameObject.transform));
            }

            [TestCase("UsingEventHandler")] // Implements IPointerClickHandler
            [TestCase("UsingEventTrigger")] // Attached EventTrigger
            [TestCase("ChildInTheSight")] // Parent object is out of sight, but this object is in the sight
            [LoadScene(TestScene)]
            public void IsReallyInteractiveFromUser_reachableObjects_returnTrue(string targetName)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(DefaultScreenPointStrategy.GetScreenPoint), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [LoadScene(TestScene)]
            public void IsReallyInteractiveFromUser_unreachableObjects_returnFalse(string targetName)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(DefaultScreenPointStrategy.GetScreenPoint), Is.False);
            }
        }

        /// <summary>
        /// InteractiveComponent test cases using UI elements
        /// </summary>
        [TestFixture]
        [GameViewResolution(GameViewResolution.FullHD)] // TODO: want to use VGA
        public class UI
        {
            private const string TestScene =
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyUiWorldSpace.unity";

            [TestCase("Button")] // Attached Button
            [TestCase("ChildInTheSight")] // Parent object is out of sight, but this object is in the sight
            [TestCase("ButtonOnInnerCanvas")] // On the inner Canvas
            [LoadScene(TestScene)]
            public async Task IsReallyInteractiveFromUser_reachableObjects_returnTrue(string targetName)
            {
                await Task.Yield(); // wait for GraphicRaycaster initialization

                var target = InteractiveComponentCollector.FindInteractiveComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(DefaultScreenPointStrategy.GetScreenPoint), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [TestCase("NotInteractable")] // Interactable=false
            [TestCase("BeyondThe2D")] // Beyond the 2D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [TestCase("BeyondThe3D")] // Beyond the 3D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [LoadScene(TestScene)]
            public async Task IsReallyInteractiveFromUser_unreachableObjects_returnFalse(string targetName)
            {
                await Task.Yield(); // wait for GraphicRaycaster initialization

                var target = InteractiveComponentCollector.FindInteractiveComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(DefaultScreenPointStrategy.GetScreenPoint), Is.False);
            }

            [TestCase("Button", "ReceiveOnClick")]
            [LoadScene(TestScene)]
            public void Tap_Tapped(string targetName, string expectedMessage)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.CanTap(), Is.True);
                target.Tap(DefaultScreenPointStrategy.GetScreenPoint);

                LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
            }
        }
    }
}
