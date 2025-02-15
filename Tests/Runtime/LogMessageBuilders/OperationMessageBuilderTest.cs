// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;

namespace TestHelper.Monkey.LogMessageBuilders
{
    [TestFixture]
    public class OperationMessageBuilderTest
    {
        private readonly Component _component = new GameObject("Target").AddComponent<FakeComponent>();
        private readonly IOperator _operator = new UGUIClickOperator();

        [Test]
        public void ToString_WithoutComment()
        {
            var sut = new OperationMessageBuilder(_component, _operator);
            Assert.That(sut.ToString(), Is.EqualTo("UGUIClickOperator operates to Target"));
        }

        [Test]
        public void ToString_WithOneComment()
        {
            var sut = new OperationMessageBuilder(_component, _operator);
            sut.AddComment("filename.png");
            Assert.That(sut.ToString(), Is.EqualTo("UGUIClickOperator operates to Target (filename.png)"));
        }

        [Test]
        public void ToString_WithTwoComments()
        {
            var sut = new OperationMessageBuilder(_component, _operator);
            sut.AddComment("comment1");
            sut.AddComment("comment2");
            Assert.That(sut.ToString(), Is.EqualTo("UGUIClickOperator operates to Target (comment1, comment2)"));
        }
    }
}
