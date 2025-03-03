// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Exceptions;
using TestHelper.Monkey.Operators;
using TestHelper.Random;
using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Run configuration for monkey testing
    /// </summary>
    public class MonkeyConfig
    {
        /// <summary>
        /// Running time
        /// </summary>
        public TimeSpan Lifetime { get; set; } = new TimeSpan(0, 0, 1, 0); // 1min

        /// <summary>
        /// Delay time between operations
        /// </summary>
        public int DelayMillis { get; set; } = 200;

        /// <summary>
        /// Seconds to determine that a throw <see cref="TimeoutException"/> when an object that can be interacted with does not exist.
        /// Disable detection if set to 0.
        /// </summary>
        public int SecondsToErrorForNoInteractiveComponent { get; set; } = 5;

        /// <summary>
        /// An <see cref="InfiniteLoopException"/> is thrown if a repeating operation is detected within the specified buffer length.
        /// For example, if the buffer length is 10, repeating 5-step sequences can be detected.
        /// Disable detection if set to 0.
        /// </summary>
        public int BufferLengthForDetectLooping { get; set; } = 10;

        /// <summary>
        /// Pseudo-random number generator
        /// </summary>
        public IRandom Random { get; set; } = new RandomWrapper();

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger { get; set; } = Debug.unityLogger;

        /// <summary>
        /// Output verbose log if true
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Show Gizmos on <c>GameView</c> during running monkey test if true
        /// </summary>
        public bool Gizmos { get; set; }

        /// <summary>
        /// Take screenshots during running the monkey test if set a <c>ScreenshotOptions</c> instance.
        /// </summary>
        public ScreenshotOptions Screenshots { get; set; }

        /// <summary>
        /// Function returns the <c>Component</c> is interactable or not.
        /// </summary>
        public Func<Component, bool> IsInteractable { get; set; } = DefaultComponentInteractableStrategy.IsInteractable;

        /// <summary>
        /// Strategy to examine whether <c>GameObject</c> should be ignored.
        /// <c>verboseLogger</c> will be overridden at runtime by the <c>Logger</c> if <c>Verbose</c> is true.
        /// </summary>
        /// <remarks>
        /// This strategy replaces the v0.14.0 or older <c>IsIgnore</c> function.
        /// </remarks>
        public IIgnoreStrategy IgnoreStrategy { get; set; } = new DefaultIgnoreStrategy();

        /// <summary>
        /// Strategy to examine whether <c>GameObject</c> is reachable from the user.
        /// <c>verboseLogger</c> will be overridden at runtime by the <c>Logger</c> if <c>Verbose</c> is true.
        /// </summary>
        /// <remarks>
        /// This strategy replaces the v0.14.0 or older <c>IsReachable</c> function.
        /// </remarks>
        public IReachableStrategy ReachableStrategy { get; set; } = new DefaultReachableStrategy();

        /// <summary>
        /// Operators that the monkey invokes.
        /// <c>logger</c> and <c>screenshotOptions</c> will be overridden at runtime by the same name properties in this <c>MonkeyConfig</c> instance.
        /// </summary>
        public IEnumerable<IOperator> Operators { get; set; } = new IOperator[]
        {
            new UGUIClickOperator(), // Specify screen click point strategy as a constructor argument, if necessary
            new UGUIClickAndHoldOperator(), // Specify screen click point strategy and hold millis, if necessary
            new UGUITextInputOperator(), // Specify random text input strategy, if necessary
        };
    }
}
