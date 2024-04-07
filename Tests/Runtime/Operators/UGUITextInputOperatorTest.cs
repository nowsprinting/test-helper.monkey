// Copyright (c) 2023-2024 Koji Hasegawa.
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
    public class UGUITextInputOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";

        private readonly IOperator _sut = new UGUITextInputOperator(
            _ => RandomStringParameters.Default,
            new StubRandomString("RANDOM"));

        [TestCase("InputField")]
        [LoadScene(TestScene)]
        public void OperateAsync_InputText(string targetName)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assume.That(_sut.IsMatch(target.component), Is.True);
            _sut.OperateAsync(target.component);

            Assert.That(((InputField)target.component).text, Is.EqualTo("RANDOM"));
        }

        [TestCase("UsingPointerClickEventTrigger")]
        [TestCase("UsingOnPointerClickHandler")]
        [TestCase("UsingOnPointerDownUpHandler")]
        [TestCase("UsingPointerDownUpEventTrigger")]
        [TestCase("UsingMultipleEventTriggers")]
        [LoadScene(TestScene)]
        public void IsMatch_CanNotInputText_ReturnFalse(string targetName)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(_sut.IsMatch(target.component), Is.False);
        }
    }
}
