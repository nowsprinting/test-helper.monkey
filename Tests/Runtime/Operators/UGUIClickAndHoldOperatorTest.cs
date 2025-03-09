// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UGUIClickAndHoldOperatorTest
    {
        private readonly IOperator _sut = new UGUIClickAndHoldOperator(holdMillis: 500);

        [Test]
        public void CanOperate_CanNotTouchAndHold_ReturnFalse()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<SpyOnPointerClickHandler>();

            Assert.That(_sut.CanOperate(gameObject), Is.False);
        }

        [Test]
        [LoadScene("../../Scenes/MissingComponent.unity")]
        public void CanOperate_ButtonWithMissingComponent_ReturnTrue()
        {
            var buttonWithMissing = Object.FindAnyObjectByType<Button>().gameObject;

            Assert.That(_sut.CanOperate(buttonWithMissing), Is.True);
        }

        [Test]
        public async Task OperateAsync_EventHandler_InvokeOnPointerDownAndUp()
        {
            var gameObject = new GameObject("ClickAndHoldTarget");
            gameObject.AddComponent<SpyOnPointerDownUpHandler>();
            var position = TransformPositionStrategy.GetScreenPoint(gameObject);
            var raycastResult = new RaycastResult { screenPosition = position };

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject, raycastResult);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerUp");
        }

        [Test]
        public async Task OperateAsync_EventTrigger_InvokeOnPointerDownAndUp()
        {
            var gameObject = new GameObject("ClickAndHoldTarget");
            gameObject.AddComponent<SpyPointerDownUpEventReceiver>();
            var position = TransformPositionStrategy.GetScreenPoint(gameObject);
            var raycastResult = new RaycastResult { screenPosition = position };

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject, raycastResult);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerUp");
        }

        [Test]
        public async Task OperateAsync_DestroyAfterPointerDown_NoError()
        {
            var gameObject = new GameObject("ClickAndHoldTarget");
            gameObject.AddComponent<StubDestroyingItselfWhenPointerDown>();
            var position = TransformPositionStrategy.GetScreenPoint(gameObject);
            var raycastResult = new RaycastResult { screenPosition = position };

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject, raycastResult);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.Destroy");
        }

        [Test]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task OperateAsync_Cancel()
        {
            var gameObject = new GameObject("ClickAndHoldTarget");
            gameObject.AddComponent<StubLogErrorWhenOnPointerUp>();
            var position = TransformPositionStrategy.GetScreenPoint(gameObject);
            var raycastResult = new RaycastResult { screenPosition = position };

            Assume.That(_sut.CanOperate(gameObject), Is.True);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                _sut.OperateAsync(gameObject, raycastResult, null, null, cancellationTokenSource.Token).Forget();
                await UniTask.NextFrame();

                cancellationTokenSource.Cancel(); // Not output LogError from StubLogErrorWhenOnPointerUp
                await UniTask.NextFrame();

                LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            }
        }
    }
}
