// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Monkey.Random;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UGUITextInputOperatorTest
    {
        private readonly ITextInputOperator _sut = new UGUITextInputOperator(
            _ => RandomStringParameters.Default,
            new StubRandomString("RANDOM"));

        [Test]
        public void IsMatch_CanNotInputText_ReturnFalse()
        {
            var component = new GameObject().AddComponent<Button>();

            Assert.That(_sut.IsMatch(component), Is.False);
        }

        [Test]
        public void OperateAsync_InputText()
        {
            var component = new GameObject().AddComponent<InputField>();

            Assume.That(_sut.IsMatch(component), Is.True);
            _sut.OperateAsync(component);

            Assert.That(component.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_InputSpecifiedText()
        {
            var component = new GameObject().AddComponent<InputField>();

            Assume.That(_sut.IsMatch(component), Is.True);
            _sut.OperateAsync(component, "SPECIFIED");

            Assert.That(component.text, Is.EqualTo("SPECIFIED"));
        }
    }
}
