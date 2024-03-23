// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class TouchAndHoldOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";

        [TestCase("UsingOnPointerDownUpHandler", "OnPointerDown", "OnPointerUp")]
        [TestCase("UsingPointerDownUpEventTrigger", "ReceivePointerDown", "ReceivePointerUp")]
        [TestCase("DestroyItselfIfPointerDown", "OnPointerDown", "DestroyImmediate")]
        [LoadScene(TestScene)]
        public async Task TouchAndHold(string targetName, string expectedMessage1, string expectedMessage2)
        {
            var target = InteractiveComponentCollector.FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanTouchAndHold(), Is.True);
            await target.TouchAndHold(DefaultScreenPointStrategy.GetScreenPoint);
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage1}");
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage2}");
        }

        [TestCase("UsingOnPointerClickHandler")]
        [TestCase("UsingPointerClickEventTrigger")]
        [LoadScene(TestScene)]
        public void CanNotTouchAndHold(string targetName)
        {
            var target = InteractiveComponentCollector.FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanTouchAndHold(), Is.False);
        }
    }
}
