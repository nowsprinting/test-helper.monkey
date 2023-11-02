// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Random;
using TestHelper.Monkey.TestDoubles;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    [GameViewResolution(GameViewResolution.VGA)]
    public class TextInputOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";


        [TestCase("InputField")]
        [LoadScene(TestScene)]
        public void InputText(string targetName)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanTextInput(), Is.True);
            Assert.That(target.component.TryGetComponent<InputField>(out var inputField));

            target.TextInput(
                _ => RandomStringParameters.Default,
                new StubRandomString("RANDOM")
            );
            Assert.That(inputField.text, Is.EqualTo("RANDOM"));
        }


        [TestCase("UsingPointerClickEventTrigger")]
        [TestCase("UsingOnPointerClickHandler")]
        [TestCase("UsingOnPointerDownUpHandler")]
        [TestCase("UsingPointerDownUpEventTrigger")]
        [TestCase("UsingMultipleEventTriggers")]
        [LoadScene(TestScene)]
        public void CanNotInputText(string targetName)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanTextInput(), Is.False);
        }
    }
}
