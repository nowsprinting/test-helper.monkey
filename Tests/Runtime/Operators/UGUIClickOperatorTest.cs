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
    public class UGUIClickOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";
        private readonly IOperator _sut = new UGUIClickOperator();

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        [LoadScene(TestScene)]
        public void OperateAsync_InvokeOnClick(string targetName, string expectedMessage)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assume.That(_sut.IsMatch(target.component), Is.True);
            _sut.OperateAsync(target.component);

            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerDownUpHandler")]
        [TestCase("UsingPointerDownUpEventTrigger")]
        [LoadScene(TestScene)]
        public void IsMatch_CanNotClick_ReturnFalse(string targetName)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(_sut.IsMatch(target.component), Is.False);
        }
    }
}
