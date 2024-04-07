// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UGUIClickAndHoldOperatorTest
    {
        private readonly IOperator _sut = new UGUIClickAndHoldOperator(holdMillis: 500);

        [Test]
        public void IsMatch_CanNotTouchAndHold_ReturnFalse()
        {
            var component = new GameObject().AddComponent<SpyOnPointerClickHandler>();

            Assert.That(_sut.IsMatch(component), Is.False);
        }

        [Test]
        public async Task OperateAsync_EventHandler_InvokeOnPointerDownAndUp()
        {
            var component = new GameObject("ClickAndHoldTarget").AddComponent<SpyOnPointerDownUpHandler>();

            Assume.That(_sut.IsMatch(component), Is.True);
            await _sut.OperateAsync(component);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerUp");
        }

        [Test]
        public async Task OperateAsync_EventTrigger_InvokeOnPointerDownAndUp()
        {
            var receiver = new GameObject("ClickAndHoldTarget").AddComponent<SpyPointerDownUpEventReceiver>();
            var component = receiver.gameObject.GetComponent<EventTrigger>();

            Assume.That(_sut.IsMatch(component), Is.True);
            await _sut.OperateAsync(component);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerUp");
        }

        [Test]
        public async Task OperateAsync_DestroyAfterPointerDown_NoError()
        {
            var component = new GameObject("ClickAndHoldTarget").AddComponent<StubDestroyingItselfWhenPointerDown>();

            Assume.That(_sut.IsMatch(component), Is.True);
            await _sut.OperateAsync(component);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.DestroyImmediate");
        }

        [Test]
        public async Task OperateAsync_Cancel()
        {
            var component = new GameObject("ClickAndHoldTarget").AddComponent<StubLogErrorWhenOnPointerUp>();
            Assume.That(_sut.IsMatch(component), Is.True);

            var cancellationTokenSource = new CancellationTokenSource();
            _sut.OperateAsync(component, cancellationTokenSource.Token).Forget();
            await UniTask.NextFrame();

            cancellationTokenSource.Cancel(); // Not output LogError from StubLogErrorWhenOnPointerUp
            await UniTask.NextFrame();

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
        }
    }
}
