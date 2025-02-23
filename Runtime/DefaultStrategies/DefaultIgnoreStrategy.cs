// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Extensions;
using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    /// <summary>
    /// Default strategy to examine whether <c>GameObject</c> should be ignored.
    /// </summary>
    public class DefaultIgnoreStrategy : IIgnoreStrategy
    {
        private ILogger _verboseLogger;

        /// <inheritdoc/>
        public ILogger VerboseLogger
        {
            set
            {
                _verboseLogger = value;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        public DefaultIgnoreStrategy(ILogger verboseLogger = null)
        {
            _verboseLogger = verboseLogger;
        }

        /// <summary>
        /// Returns whether the <c>GameObject</c> is ignored or not.
        /// Default implementation is to check whether the <c>GameObject</c> has <c>IgnoreAnnotation</c> component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>True if <c>GameObject</c> is ignored</returns>
        public bool IsIgnored(GameObject gameObject)
        {
            var hasIgnoreAnnotation = gameObject.TryGetEnabledComponent<IgnoreAnnotation>(out _);
            if (hasIgnoreAnnotation && _verboseLogger != null)
            {
                _verboseLogger.Log($"Ignored {gameObject.name}({gameObject.GetInstanceID()}).");
            }

            return hasIgnoreAnnotation;
        }

        /// <summary>
        /// Ensure the <c>GameObject</c> is ignored or not.
        /// Default implementation is to check whether the GameObject has <c>IgnoreAnnotation</c> component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <returns>True if GameObject is ignored</returns>
        [Obsolete("Use instance method instead")]
        public static bool IsIgnored(GameObject gameObject, ILogger verboseLogger = null)
        {
            var hasIgnoreAnnotation = gameObject.TryGetEnabledComponent<IgnoreAnnotation>(out _);
            if (hasIgnoreAnnotation && verboseLogger != null)
            {
                verboseLogger.Log($"Ignored {gameObject.name}({gameObject.GetInstanceID()}).");
            }

            return hasIgnoreAnnotation;
        }
    }
}
