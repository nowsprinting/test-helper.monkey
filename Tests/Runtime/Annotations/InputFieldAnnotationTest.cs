// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Annotations.Enums;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    [TestFixture]
    [GameViewResolution(GameViewResolution.VGA)]
    public partial class InputFieldAnnotationTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/InputFields.unity";
        private GameObject _annotationAttachedGameObject;

        [SetUp]
        public void SetUp()
        {
            var defaultTextValidator = GameObject.Find("InputField").AddComponent<SpyInputFieldValidator>();
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

            var validator = _annotationAttachedGameObject.AddComponent<SpyInputFieldValidator>();
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

            var validator = _annotationAttachedGameObject.AddComponent<SpyInputFieldValidator>();
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
