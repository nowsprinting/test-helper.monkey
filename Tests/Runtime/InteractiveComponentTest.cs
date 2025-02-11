// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
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
            public void IsReachable_reachableObjects_returnTrue(string targetName)
            {
                var target = new InteractableComponentsFinder().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.gameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [LoadScene(TestScene)]
            public void IsReachable_unreachableObjects_returnFalse(string targetName)
            {
                var target = new InteractableComponentsFinder().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.gameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.False);
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
            public async Task IsReachable_reachableObjects_returnTrue(string targetName)
            {
                await Task.Yield(); // wait for GraphicRaycaster initialization

                var target = new InteractableComponentsFinder().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.gameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [TestCase("BeyondThe2D")] // Beyond the 2D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [TestCase("BeyondThe3D")] // Beyond the 3D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [LoadScene(TestScene)]
            public async Task IsReachable_unreachableObjects_returnFalse(string targetName)
            {
                await Task.Yield(); // wait for GraphicRaycaster initialization

                var target = new InteractableComponentsFinder().FindInteractableComponents()
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.gameObject.IsReachable(DefaultReachableStrategy.IsReachable), Is.False);
            }
        }

        [TestFixture]
        public class Operators
        {
            private static readonly IOperator s_clickOperator = new UGUIClickOperator();
            private static readonly IOperator s_clickAndHoldOperator = new UGUIClickAndHoldOperator();
            private static readonly IOperator s_textInputOperator = new UGUITextInputOperator();

            private readonly IEnumerable<IOperator> _operators = new[]
            {
                s_clickOperator, s_clickAndHoldOperator, // Matches UnityEngine.UI.Button
                s_textInputOperator, // Does not match UnityEngine.UI.Button
            };

            [Test]
            public void GetOperators_Button_GotClickAndClickAndHoldOperator()
            {
                var button = new GameObject().AddComponent<Button>();
                var sut = InteractiveComponent.CreateInteractableComponent(button, operators: _operators);
                var actual = sut.GetOperators();

                Assert.That(actual, Is.EquivalentTo(new[] { s_clickOperator, s_clickAndHoldOperator }));
            }

            [Test]
            public void GetOperatorsByType_Button_GotClickOperator()
            {
                var button = new GameObject().AddComponent<Button>();
                var sut = InteractiveComponent.CreateInteractableComponent(button, operators: _operators);
                var actual = sut.GetOperatorsByType<IClickOperator>();

                Assert.That(actual, Is.EquivalentTo(new[] { s_clickOperator }));
            }

            [Test]
            public void Click_InvokeOnClick()
            {
                var button = new GameObject().AddComponent<Button>();
                button.onClick.AddListener(() => Debug.Log("Invoke Button.OnClick!"));
                var sut = InteractiveComponent.CreateInteractableComponent(button, operators: _operators);

                Assume.That(sut.CanClick(), Is.True);
                sut.Click();

                LogAssert.Expect(LogType.Log, "Invoke Button.OnClick!");
            }

            [Test]
            public async Task ClickAndHold_InvokeOnPointerDownAndUp()
            {
                var button = new GameObject("ClickAndHoldTarget").AddComponent<SpyOnPointerDownUpHandler>();
                var sut = InteractiveComponent.CreateInteractableComponent(button, operators: _operators);

                Assume.That(sut.CanClickAndHold(), Is.True);
                await sut.ClickAndHold();

                LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
                LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerUp");
            }

            [Test]
            public void TextInput_InputRandomText()
            {
                var inputField = new GameObject("InputField").AddComponent<InputField>();
                var sut = InteractiveComponent.CreateInteractableComponent(inputField, operators: _operators);

                Assume.That(sut.CanTextInput(), Is.True);
                sut.TextInput();

                Assert.That(inputField.text, Does.Match("\\w{5,10}"));
            }

            [Test]
            public void TextInput_InputSpecifiedText()
            {
                var inputField = new GameObject("InputField").AddComponent<InputField>();
                var sut = InteractiveComponent.CreateInteractableComponent(inputField, operators: _operators);

                Assume.That(sut.CanTextInput(), Is.True);
                sut.TextInput("specified text!");

                Assert.That(inputField.text, Is.EqualTo("specified text!"));
            }
        }
    }
}
