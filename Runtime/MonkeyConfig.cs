// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.Random;
using TestHelper.Monkey.ScreenPointStrategies;
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
        /// Delay time for touch-and-hold
        /// </summary>
        public int TouchAndHoldDelayMillis { get; set; } = 1000;

        /// <summary>
        /// Random number generator
        /// </summary>
        public IRandom Random { get; set; } = new RandomWrapper();

        /// <summary>
        /// Random string generator
        /// </summary>
        public IRandomString RandomString { get; set; } = new RandomStringImpl(new RandomWrapper());

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger { get; set; } = Debug.unityLogger;

        /// <summary>
        /// Function returns the screen position where monkey operators operate on for the specified gameObject
        /// </summary>
        public Func<GameObject, Vector2> ScreenPointStrategy { get; set; } = DefaultScreenPointStrategy.GetScreenPoint;

        /// <summary>
        /// Function returns the random string generation parameters
        /// </summary>
        public Func<GameObject, RandomStringParameters> RandomStringParametersStrategy { get; set; } =
            DefaultRandomStringParameterGen;

        private static RandomStringParameters DefaultRandomStringParameterGen(GameObject _) =>
            RandomStringParameters.Default;

        /// <summary>
        /// Show Gizmos on <c>GameView</c> during running monkey test if true
        /// </summary>
        public bool Gizmos { get; set; } = false;

        /// <summary>
        /// Take screenshots during running the monkey test if set a <c>ScreenshotOptions</c> instance.
        /// </summary>
        public ScreenshotOptions Screenshots { get; set; } = null;
    }
}
