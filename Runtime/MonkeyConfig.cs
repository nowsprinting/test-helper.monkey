// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.DefaultStrategies;
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
        /// Seconds to determine that an error has occurred when an object that can be interacted with does not exist
        /// </summary>
        public int SecondsToErrorForNoInteractiveComponent { get; set; } = 5;

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
        /// <c>verboseLogger</c> will be overridden at runtime by the values of <c>Verbose</c> and <c>Logger</c>.
        /// </summary>
        public IIgnoreStrategy IgnoreStrategy { get; set; } = new DefaultIgnoreStrategy();

        /// <summary>
        /// Strategy to examine whether <c>GameObject</c> is reachable from the user.
        /// <c>verboseLogger</c> will be overridden at runtime by the values of <c>Verbose</c> and <c>Logger</c>.
        /// </summary>
        public IReachableStrategy ReachableStrategy { get; set; } = new DefaultReachableStrategy();

        /// <summary>
        /// Operators that the monkey invokes.
        /// </summary>
        public IEnumerable<IOperator> Operators { get; set; } = new IOperator[]
        {
            new UGUIClickOperator(), // Specify screen click point strategy as a constructor argument, if necessary
            new UGUIClickAndHoldOperator(), // Specify screen click point strategy and hold millis, if necessary
            new UGUITextInputOperator(), // Specify random text input strategy, if necessary
        };

        internal void ApplyVerboseLogger()
        {
            if (!Verbose)
            {
                return;
            }

            IgnoreStrategy.VerboseLogger = Logger;
            ReachableStrategy.VerboseLogger = Logger;
        }
    }
}
