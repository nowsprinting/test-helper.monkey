// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using NUnit.Framework;
using TestHelper.Monkey.Operators;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Extensions
{
    public class ComponentExtensionsTest
    {
        private static readonly IOperator s_clickOperator = new UGUIClickOperator();
        private static readonly IOperator s_clickAndHoldOperator = new UGUIClickAndHoldOperator();
        private static readonly IOperator s_textInputOperator = new UGUITextInputOperator();

        private readonly IEnumerable<IOperator> _operators = new[]
        {
            s_clickOperator, s_clickAndHoldOperator, // Matches UnityEngine.UI.Button
            s_textInputOperator, // Does not match UnityEngine.UI.Button
        };

        [Test]
        public void GetOperators_Button_GotClickAndClickAndHoldOperator()
        {
            var button = new GameObject().AddComponent<Button>();
            var actual = button.SelectOperators(_operators);

            Assert.That(actual, Is.EquivalentTo(new[] { s_clickOperator, s_clickAndHoldOperator }));
        }

        [Test]
        public void GetOperatorsByType_Button_GotClickOperator()
        {
            var button = new GameObject().AddComponent<Button>();
            var actual = button.SelectOperators<IClickOperator>(_operators);

            Assert.That(actual, Is.EquivalentTo(new[] { s_clickOperator }));
        }
    }
}
