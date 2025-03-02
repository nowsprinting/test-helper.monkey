// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using TestHelper.RuntimeInternals;
using UnityEngine;

namespace TestHelper.Monkey.Operators.Utils
{
    /// <summary>
    /// Build and output log message with a screenshot for operation.
    /// </summary>
    public class OperationLogger
    {
        private readonly GameObject _gameObject;
        private readonly IOperator _operator;
        private readonly ILogger _logger;
        private readonly ScreenshotOptions _screenshotOptions;

        /// <summary>
        /// Comments for the <c>Component</c>.
        /// e.g., button text, texture name.
        /// If omitted, output instance ID.
        /// </summary>
        public readonly List<string> Comments = new List<string>();

        /// <summary>
        /// Properties for the operation.
        /// e.g., click position, input text.
        /// </summary>
        public readonly Dictionary<string, object> Properties = new Dictionary<string, object>();

        private string _lastScreenshotPath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="operator"></param>
        /// <param name="logger"></param>
        /// <param name="screenshotOptions"></param>
        public OperationLogger(GameObject gameObject, IOperator @operator, ILogger logger,
            ScreenshotOptions screenshotOptions = null)
        {
            _gameObject = gameObject;
            _operator = @operator;
            _logger = logger;
            _screenshotOptions = screenshotOptions;
        }

        /// <summary>
        /// Output log message with a screenshot.
        /// Call this before the operation, after the shown effects.
        /// </summary>
        public async UniTask Log()
        {
            if (_screenshotOptions != null)
            {
                _lastScreenshotPath = _screenshotOptions.FilenameStrategy.GetFilename();
                await ScreenshotHelper.TakeScreenshot(
                        directory: _screenshotOptions.Directory,
                        filename: _lastScreenshotPath,
                        superSize: _screenshotOptions.SuperSize,
                        stereoCaptureMode: _screenshotOptions.StereoCaptureMode,
                        logFilepath: false
                    )
                    .ToUniTask(_gameObject.GetComponent<MonoBehaviour>());
            }

            _logger.Log(BuildMessage());
        }

        internal string BuildMessage()
        {
            var builder = new StringBuilder();
            builder.Append($"{_operator.GetType().Name} operates to {_gameObject.name}");

            if (Comments.Count > 0)
            {
                builder.Append("(");
                builder.Append(string.Join(", ", Comments));
                builder.Append(")");
            }
            else
            {
                builder.Append($"({_gameObject.GetInstanceID()})");
            }

            if (Properties.Count > 0)
            {
                foreach (var prop in Properties)
                {
                    builder.Append($", {prop.Key}={Format(prop.Value)}");
                }
            }

            if (!string.IsNullOrEmpty(_lastScreenshotPath))
            {
                builder.Append($", screenshot={_lastScreenshotPath}");
            }

            return builder.ToString();
        }

        private static string Format(object value)
        {
            if (value is Vector2 vector2)
            {
                return $"({vector2.x:F0},{vector2.y:F0})"; // format as an integer because the screen position
            }

            if (value is Vector3 vector3)
            {
                return $"({vector3.x:F2},{vector3.y:F2},{vector3.z:F2})"; // C#8.0 or older have different formats
            }

            return value.ToString();
        }
    }
}
