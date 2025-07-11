// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators.Utils;
using TestHelper.Monkey.Random;
using TestHelper.Random;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private readonly ScreenshotOptions _screenshotOptions;
        private readonly ILogger _logger;

        /// <summary>
        /// Input random text that is randomly generated by <paramref name="randomStringParams"/>
        /// </summary>
        /// <param name="randomStringParams">Random string generation parameters</param>
        /// <param name="randomString">Random string generator</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UGUITextInputOperator(
            Func<GameObject, RandomStringParameters> randomStringParams = null, IRandomString randomString = null,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            _randomStringParams = randomStringParams ?? (_ => RandomStringParameters.Default);
            _randomString = randomString ?? new RandomStringImpl(new RandomWrapper());
            _screenshotOptions = screenshotOptions;
            _logger = logger ?? Debug.unityLogger;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
#if ENABLE_TMP
            return gameObject.TryGetEnabledComponent<InputField>(out _) ||
                   gameObject.TryGetEnabledComponent<TMP_InputField>(out _);
#else
            return gameObject.TryGetEnabledComponent<InputField>(out _);
#endif
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method does not require a <c>RaycastResult</c>.
        /// </remarks>
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult _,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            Func<GameObject, RandomStringParameters> randomStringParams;
            if (gameObject.TryGetEnabledComponent<InputFieldAnnotation>(out var annotation))
            {
                // Overwrite rule if annotation is attached.
                randomStringParams = __ => new RandomStringParameters(
                    (int)annotation.minimumLength,
                    (int)annotation.maximumLength,
                    annotation.charactersKind);
            }
            else
            {
                randomStringParams = _randomStringParams;
            }

            var text = _randomString.Next(randomStringParams(gameObject));
            await OperateAsync(gameObject, text, logger, screenshotOptions, cancellationToken);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        public async UniTask OperateAsync(GameObject gameObject, string text,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            logger = logger ?? _logger;
            screenshotOptions = screenshotOptions ?? _screenshotOptions;

            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(gameObject, this, logger, screenshotOptions);
            operationLogger.Properties.Add("text", $"\"{text}\"");
            await operationLogger.Log();

            // Select before input text
            ExecuteEvents.ExecuteHierarchy(gameObject, null, ExecuteEvents.selectHandler);
            // Note: OnDeselect event is called by the system when the focus moves to another element, so it is not called in this method.

            // Input text
            if (gameObject.TryGetEnabledComponent<InputField>(out var inputField))
            {
                inputField.text = text;
            }
#if ENABLE_TMP
            if (gameObject.TryGetEnabledComponent<TMP_InputField>(out var tmpInputField))
            {
                tmpInputField.text = text;
            }
#endif
        }
    }
}
