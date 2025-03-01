// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Monkey.TestDoubles;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UGUITextInputOperatorTest
    {
        private ITextInputOperator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new UGUITextInputOperator(randomString: new StubRandomString("RANDOM"));
        }

        [Test]
        public void CanOperate_NotInputText_ReturnFalse()
        {
            var component = new GameObject().AddComponent<Button>();

            Assert.That(_sut.CanOperate(component), Is.False);
        }

        [Test]
        public void OperateAsync_InputText_SetsRandomText()
        {
            var component = new GameObject().AddComponent<InputField>();

            Assume.That(_sut.CanOperate(component), Is.True);
            _sut.OperateAsync(component, default(RaycastResult));

            Assert.That(component.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_InputTextWithText_SetsSpecifiedText()
        {
            var component = new GameObject().AddComponent<InputField>();

            Assume.That(_sut.CanOperate(component), Is.True);
            _sut.OperateAsync(component, "SPECIFIED");

            Assert.That(component.text, Is.EqualTo("SPECIFIED"));
        }

        [Test]
        public void OperateAsync_TmpInputText_SetsRandomText()
        {
            var component = new GameObject().AddComponent<TMP_InputField>();

            Assume.That(_sut.CanOperate(component), Is.True);
            _sut.OperateAsync(component, default(RaycastResult));

            Assert.That(component.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_TmpInputTextWithText_SetsSpecifiedText()
        {
            var component = new GameObject().AddComponent<TMP_InputField>();

            Assume.That(_sut.CanOperate(component), Is.True);
            _sut.OperateAsync(component, "SPECIFIED");

            Assert.That(component.text, Is.EqualTo("SPECIFIED"));
        }
    }
}
