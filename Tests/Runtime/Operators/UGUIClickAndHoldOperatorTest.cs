// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UGUIClickAndHoldOperatorTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity";
        private readonly IOperator _sut = new UGUIClickAndHoldOperator(holdMillis: 500);

        [TestCase("UsingOnPointerDownUpHandler", "OnPointerDown", "OnPointerUp")]
        [TestCase("UsingPointerDownUpEventTrigger", "ReceivePointerDown", "ReceivePointerUp")]
        [TestCase("DestroyItselfIfPointerDown", "OnPointerDown", "DestroyImmediate")]
        [LoadScene(TestScene)]
        public async Task OperateAsync_InvokeOnPointerDownAndUp(string targetName, string expectedMessage1,
            string expectedMessage2)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assume.That(_sut.IsMatch(target.component), Is.True);
            await _sut.OperateAsync(target.component);

            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage1}");
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage2}");
        }

        [Test]
        public async Task OperateAsync_Cancel()
        {
            var stub = new GameObject("StubLogErrorWhenOnPointerUp").AddComponent<StubLogErrorWhenOnPointerUp>();
            var target = InteractiveComponent.CreateInteractableComponent(stub);
            Assume.That(_sut.IsMatch(target.component), Is.True);

            var cancellationTokenSource = new CancellationTokenSource();
            _sut.OperateAsync(target.component, cancellationTokenSource.Token).Forget();
            await UniTask.NextFrame();

            cancellationTokenSource.Cancel(); // Not output LogError from StubLogErrorWhenOnPointerUp
            await UniTask.NextFrame();

            LogAssert.Expect(LogType.Log, $"{stub.gameObject.name}.OnPointerDown");
        }

        [TestCase("UsingOnPointerClickHandler")]
        [TestCase("UsingPointerClickEventTrigger")]
        [LoadScene(TestScene)]
        public void IsMatch_CanNotTouchAndHold_ReturnFalse(string targetName)
        {
            var target = new InteractiveComponentCollector().FindInteractableComponents()
                .First(x => x.gameObject.name == targetName);

            Assert.That(_sut.IsMatch(target.component), Is.False);
        }
    }
}
