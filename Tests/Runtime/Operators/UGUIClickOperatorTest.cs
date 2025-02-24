// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UGUIClickOperatorTest
    {
        private readonly IOperator _sut = new UGUIClickOperator();

        [Test]
        public void CanOperate_CanNotClick_ReturnFalse()
        {
            var component = new GameObject().AddComponent<SpyOnPointerDownUpHandler>();

            Assert.That(_sut.CanOperate(component), Is.False);
        }

        [Test]
        public void OperateAsync_EventHandler_InvokeOnClick()
        {
            var component = new GameObject("ClickTarget").AddComponent<SpyOnPointerClickHandler>();
            var position = TransformPositionStrategy.GetScreenPoint(component.gameObject);

            Assume.That(_sut.CanOperate(component), Is.True);
            _sut.OperateAsync(component, position);

            LogAssert.Expect(LogType.Log, "ClickTarget.OnPointerClick");
        }

        [Test]
        public void OperateAsync_EventTrigger_InvokeOnClick()
        {
            var receiver = new GameObject("ClickTarget").AddComponent<SpyPointerClickEventReceiver>();
            var component = receiver.gameObject.GetComponent<EventTrigger>();
            var position = TransformPositionStrategy.GetScreenPoint(component.gameObject);

            Assume.That(_sut.CanOperate(component), Is.True);
            _sut.OperateAsync(component, position);

            LogAssert.Expect(LogType.Log, "ClickTarget.ReceivePointerClick");
        }
    }
}
