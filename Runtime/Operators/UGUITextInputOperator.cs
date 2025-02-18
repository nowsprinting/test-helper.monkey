// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Random;
using TestHelper.Random;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_TMP
using TMPro;
#endif

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Text input operator for Unity UI (uGUI) <c>InputField</c> component.
    /// </summary>
    public class UGUITextInputOperator : ITextInputOperator
    {
        private readonly Func<GameObject, RandomStringParameters> _randomStringParams;
        private readonly IRandomString _randomString;

        /// <summary>
        /// Input random text that is randomly generated by <paramref name="randomStringParams"/>
        /// </summary>
        /// <param name="randomStringParams">Random string generation parameters</param>
        /// <param name="randomString">Random string generator</param>
        public UGUITextInputOperator(
            Func<GameObject, RandomStringParameters> randomStringParams = null,
            IRandomString randomString = null)
        {
            _randomStringParams = randomStringParams ?? (_ => RandomStringParameters.Default);
            _randomString = randomString ?? new RandomStringImpl(new RandomWrapper());
        }

        /// <inheritdoc />
        public bool CanOperate(Component component)
        {
#if ENABLE_TMP
            return component is InputField || component is TMP_InputField;
#else
            return component is InputField;
#endif
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(Component component, CancellationToken cancellationToken = default)
        {
            if (!CanOperate(component))
            {
                throw new ArgumentException("Component must be of type InputField or TMP_InputField.");
            }

            Func<GameObject, RandomStringParameters> randomStringParams;
            if (component.gameObject.TryGetEnabledComponent<InputFieldAnnotation>(out var annotation))
            {
                // Overwrite rule if annotation is attached.
                randomStringParams = _ => new RandomStringParameters(
                    (int)annotation.minimumLength,
                    (int)annotation.maximumLength,
                    annotation.charactersKind);
            }
            else
            {
                randomStringParams = _randomStringParams;
            }

            if (component is InputField inputField)
            {
                inputField.text = _randomString.Next(randomStringParams(component.gameObject));
            }
#if ENABLE_TMP
            if (component is TMP_InputField tmpInputField)
            {
                tmpInputField.text = _randomString.Next(randomStringParams(component.gameObject));
            }
#endif
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(Component component, string text,
            CancellationToken cancellationToken = default)
        {
            if (!CanOperate(component))
            {
                throw new ArgumentException("Component must be of type InputField or TMP_InputField.");
            }

            if (component is InputField inputField)
            {
                inputField.text = text;
            }
#if ENABLE_TMP
            if (component is TMP_InputField tmpInputField)
            {
                tmpInputField.text = text;
            }
#endif
        }
    }
}
