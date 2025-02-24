// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey.DefaultStrategies;
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
        public void CanOperate_CanNotTouchAndHold_ReturnFalse()
        {
            var component = new GameObject().AddComponent<SpyOnPointerClickHandler>();

            Assert.That(_sut.CanOperate(component), Is.False);
        }

        [Test]
        public async Task OperateAsync_EventHandler_InvokeOnPointerDownAndUp()
        {
            var component = new GameObject("ClickAndHoldTarget").AddComponent<SpyOnPointerDownUpHandler>();
            var position = TransformPositionStrategy.GetScreenPoint(component.gameObject);

            Assume.That(_sut.CanOperate(component), Is.True);
            await _sut.OperateAsync(component, position);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerUp");
        }

        [Test]
        public async Task OperateAsync_EventTrigger_InvokeOnPointerDownAndUp()
        {
            var receiver = new GameObject("ClickAndHoldTarget").AddComponent<SpyPointerDownUpEventReceiver>();
            var component = receiver.gameObject.GetComponent<EventTrigger>();
            var position = TransformPositionStrategy.GetScreenPoint(component.gameObject);

            Assume.That(_sut.CanOperate(component), Is.True);
            await _sut.OperateAsync(component, position);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerUp");
        }

        [Test]
        public async Task OperateAsync_DestroyAfterPointerDown_NoError()
        {
            var component = new GameObject("ClickAndHoldTarget").AddComponent<StubDestroyingItselfWhenPointerDown>();
            var position = TransformPositionStrategy.GetScreenPoint(component.gameObject);

            Assume.That(_sut.CanOperate(component), Is.True);
            await _sut.OperateAsync(component, position);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.DestroyImmediate");
        }

        [Test]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task OperateAsync_Cancel()
        {
            var component = new GameObject("ClickAndHoldTarget").AddComponent<StubLogErrorWhenOnPointerUp>();
            var position = TransformPositionStrategy.GetScreenPoint(component.gameObject);

            Assume.That(_sut.CanOperate(component), Is.True);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                _sut.OperateAsync(component, position, cancellationTokenSource.Token).Forget();
                await UniTask.NextFrame();

                cancellationTokenSource.Cancel(); // Not output LogError from StubLogErrorWhenOnPointerUp
                await UniTask.NextFrame();

                LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            }
        }
    }
}
