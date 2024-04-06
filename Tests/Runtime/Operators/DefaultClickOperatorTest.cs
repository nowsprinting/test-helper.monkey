// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class DefaultClickOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";
        private readonly IOperator _sut = new DefaultClickOperator();

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        [LoadScene(TestScene)]
        public void Click(string targetName, string expectedMessage)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assume.That(_sut.IsMatch(target.component), Is.True);
            _sut.Operate(target.component);

            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        [LoadScene(TestScene)]
        public void Tap(string targetName, string expectedMessage) // Same as Click
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assume.That(_sut.IsMatch(target.component), Is.True);
            _sut.Operate(target.component);

            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerDownUpHandler")]
        [TestCase("UsingPointerDownUpEventTrigger")]
        [LoadScene(TestScene)]
        public void CanNotClick(string targetName)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(_sut.IsMatch(target.component), Is.False);
        }
    }
}
