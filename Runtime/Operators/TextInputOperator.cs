// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Random;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    internal static class TextInputOperator
    {
        internal static bool CanTextInput(MonoBehaviour component)
        {
            if (component.gameObject.TryGetComponent(typeof(IgnoreAnnotation), out _))
            {
                return false;
            }

            return component is InputField;
        }

        internal static void Input(
            MonoBehaviour component,
            Func<GameObject, RandomStringParameters> randomStringParams,
            IRandomString randomString
        )
        {
            if (!(component is InputField inputField))
            {
                return;
            }

            var annotation = component.gameObject.GetComponent<InputFieldAnnotation>();
            if (annotation != null)
            {
                // Overwrite rule if annotation is attached.
                randomStringParams = _ => new RandomStringParameters(
                    annotation.minimumLength,
                    annotation.maximumLength,
                    annotation.charactersKind);
            }

            inputField.text = randomString.Next(randomStringParams(component.gameObject));
        }
    }
}
