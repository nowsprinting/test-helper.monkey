// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class ClickOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        [LoadScene(TestScene)]
        public void Click(string targetName, string expectedMessage)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanClick(), Is.True);
            target.Click();
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        [LoadScene(TestScene)]
        public void Tap(string targetName, string expectedMessage) // Same as Click
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanTap(), Is.True);
            target.Tap();
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerDownUpHandler")]
        [TestCase("UsingPointerDownUpEventTrigger")]
        [LoadScene(TestScene)]
        public void CanNotClick(string targetName)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanClick(), Is.False);
        }
    }
}
