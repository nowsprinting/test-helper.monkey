// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Annotations.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Annotations
{
    [TestFixture]
    [GameViewResolution(GameViewResolution.VGA)]
    public class InputFieldAnnotationTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/InputFields.unity";
        private GameObject _annotationAttachedGameObject;

        private class InputFieldValidator : MonoBehaviour
        {
            private InputField _inputField;
            private Regex _pattern;

            public void SetValidPattern(string pattern)
            {
                _pattern = new Regex(pattern);
            }

            private void Start()
            {
                _inputField = GetComponent<InputField>();
            }

            private void Update()
            {
                if (_inputField.text == string.Empty)
                {
                    return;
                }

                if (!_pattern.IsMatch(_inputField.text))
                {
                    Debug.LogError($"{gameObject.name}: {_inputField.text} is not match pattern.");
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            var defaultTextValidator = GameObject.Find("InputField").AddComponent<InputFieldValidator>();
            defaultTextValidator.SetValidPattern("^[a-zA-Z0-9]{5,10}$");

            _annotationAttachedGameObject = GameObject.Find("InputFieldWithAnnotation");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task DefaultRandomStringParameterOnly()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(500), // 500ms
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
            };
            await Monkey.Run(config); // as successful if no LogError output
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task AttachInputFieldAnnotation_Digits6to9()
        {
            var annotation = _annotationAttachedGameObject.GetComponent<InputFieldAnnotation>();
            annotation.charactersKind = CharactersKind.Digits;
            annotation.minimumLength = 6;
            annotation.maximumLength = 9;

            var validator = _annotationAttachedGameObject.AddComponent<InputFieldValidator>();
            validator.SetValidPattern("^[0-9]{6,9}$");

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(500), // 500ms
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
            };
            await Monkey.Run(config); // as successful if no LogError output
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task AttachInputFieldAnnotation_Printable4to11()
        {
            var annotation = _annotationAttachedGameObject.GetComponent<InputFieldAnnotation>();
            annotation.charactersKind = CharactersKind.Printable;
            annotation.minimumLength = 4;
            annotation.maximumLength = 11;

            var validator = _annotationAttachedGameObject.AddComponent<InputFieldValidator>();
            validator.SetValidPattern("^.{4,11}$");

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(500), // 500ms
                DelayMillis = 1, // 1ms
                TouchAndHoldDelayMillis = 1, // 1ms
            };
            await Monkey.Run(config); // as successful if no LogError output
        }
    }
}
