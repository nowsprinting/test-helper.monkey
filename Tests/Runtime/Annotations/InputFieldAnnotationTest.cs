// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Annotations.Enums;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestHelper.Monkey.Annotations
{
    [TestFixture]
    public partial class InputFieldAnnotationTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/InputFields.unity";
        private GameObject _annotationAttachedGameObject;
        private MonkeyConfig _config;

        [SetUp]
        public void SetUp()
        {
            _annotationAttachedGameObject = GameObject.Find("InputFieldWithAnnotation");

            _config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(500), // 500ms
                DelayMillis = 1,                           // 1ms
                BufferLengthForDetectLooping = 0,          // disable loop operation detection
                Operators = new[] { new UGUITextInputOperator() }
            };
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task DefaultRandomStringParameter_Alphanumeric5to10()
        {
            var validator = _annotationAttachedGameObject.AddComponent<SpyInputFieldValidator>();
            validator.SetValidPattern("^[a-zA-Z0-9]{5,10}$");

            await Monkey.Run(_config); // as successful if no LogError output

            Object.Destroy(validator); // teardown
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task AttachInputFieldAnnotation_Digits6to9()
        {
            var annotation = _annotationAttachedGameObject.GetComponent<InputFieldAnnotation>();
            annotation.charactersKind = CharactersKind.Digits;
            annotation.minimumLength = 6;
            annotation.maximumLength = 9;

            var validator = _annotationAttachedGameObject.AddComponent<SpyInputFieldValidator>();
            validator.SetValidPattern("^[0-9]{6,9}$");

            await Monkey.Run(_config); // as successful if no LogError output

            Object.Destroy(validator); // teardown
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task AttachInputFieldAnnotation_Printable6to9()
        {
            var annotation = _annotationAttachedGameObject.GetComponent<InputFieldAnnotation>();
            annotation.charactersKind = CharactersKind.Printable;
            annotation.minimumLength = 6;
            annotation.maximumLength = 9;

            var validator = _annotationAttachedGameObject.AddComponent<SpyInputFieldValidator>();
            validator.SetValidPattern("^.{6,9}$");

            await Monkey.Run(_config); // as successful if no LogError output

            Object.Destroy(validator); // teardown
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task DisabledAnnotation_IgnoreAnnotation()
        {
            var annotation = _annotationAttachedGameObject.GetComponent<InputFieldAnnotation>();
            annotation.charactersKind = CharactersKind.Printable;
            annotation.minimumLength = 4;
            annotation.maximumLength = 11;
            annotation.enabled = false; // disabled

            var validator = _annotationAttachedGameObject.AddComponent<SpyInputFieldValidator>();
            validator.SetValidPattern("^[a-zA-Z0-9]{5,10}$"); // respect default parameters

            await Monkey.Run(_config); // as successful if no LogError output

            Object.Destroy(validator); // teardown
        }
    }
}
