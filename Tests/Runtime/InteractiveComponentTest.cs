// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Operators;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey
{
    [TestFixture]
    public class InteractiveComponentTest
    {
        [Test]
        public void CreateInteractableComponent_CreateInstance()
        {
            var button = new GameObject("InteractableButton").AddComponent<Button>();
            var actual = InteractiveComponent.CreateInteractableComponent(button);
            Assert.That(actual, Is.Not.Null);
        }

        [Test]
        public void CreateInteractableComponent_NotInteractable_ThrowsException()
        {
            var button = new GameObject("NotInteractableButton").AddComponent<Button>();
            button.interactable = false;
            Assert.That(() => InteractiveComponent.CreateInteractableComponent(button), Throws.ArgumentException
                .And.Message.Contains("Component is not interactable"));
        }

        /// <summary>
        /// InteractiveComponent test cases using 3D objects
        /// </summary>
        [TestFixture]
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
                var sut = InteractiveComponent.CreateInteractableComponent(component);

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
                var target = new InteractiveComponentCollector().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReachable(), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [LoadScene(TestScene)]
            public void IsReallyInteractiveFromUser_unreachableObjects_returnFalse(string targetName)
            {
                var target = new InteractiveComponentCollector().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReachable(), Is.False);
            }
        }

        /// <summary>
        /// InteractiveComponent test cases using UI elements
        /// </summary>
        [TestFixture]
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

                var target = new InteractiveComponentCollector().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReachable(), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [TestCase("BeyondThe2D")] // Beyond the 2D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [TestCase("BeyondThe3D")] // Beyond the 3D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [LoadScene(TestScene)]
            public async Task IsReallyInteractiveFromUser_unreachableObjects_returnFalse(string targetName)
            {
                await Task.Yield(); // wait for GraphicRaycaster initialization

                var target = new InteractiveComponentCollector().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReachable(), Is.False);
            }

            [Test]
            [LoadScene(TestScene)]
            public void FilterAvailableOperators_Button_GotClickOperator()
            {
                var clickOperator = new DefaultClickOperator();
                var touchAndHoldOperator = new DefaultTouchAndHoldOperator();
                var textInputOperator = new DefaultTextInputOperator();
                IEnumerable<IOperator> operators = new IOperator[]
                {
                    clickOperator, touchAndHoldOperator, // Match
                    textInputOperator, // Not match
                };

                var target = new InteractiveComponentCollector().FindInteractableComponents()
                    .First(x => x.gameObject.name == "Button");
                var actual = target.FilterAvailableOperators(operators);

                Assert.That(actual, Is.EquivalentTo(new IOperator[] { clickOperator, touchAndHoldOperator }));
            }
        }
    }
}
