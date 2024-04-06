// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class DefaultTouchAndHoldOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";
        private readonly IOperator _sut = new DefaultTouchAndHoldOperator(holdMillis: 100);

        [TestCase("UsingOnPointerDownUpHandler", "OnPointerDown", "OnPointerUp")]
        [TestCase("UsingPointerDownUpEventTrigger", "ReceivePointerDown", "ReceivePointerUp")]
        [TestCase("DestroyItselfIfPointerDown", "OnPointerDown", "DestroyImmediate")]
        [LoadScene(TestScene)]
        public async Task TouchAndHold(string targetName, string expectedMessage1, string expectedMessage2)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assume.That(_sut.IsMatch(target.component), Is.True);
            await _sut.Operate(target.component);

            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage1}");
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage2}");
        }

        [TestCase("UsingOnPointerClickHandler")]
        [TestCase("UsingPointerClickEventTrigger")]
        [LoadScene(TestScene)]
        public void CanNotTouchAndHold(string targetName)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(_sut.IsMatch(target.component), Is.False);
        }
    }
}
