// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.Random;
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
        public TimeSpan Lifetime = new TimeSpan(0, 0, 3, 0); // 3min

        /// <summary>
        /// Delay time between operations
        /// </summary>
        public int DelayMillis = 200;

        /// <summary>
        /// Seconds to determine that an error has occurred when an object that can be interacted with does not exist
        /// </summary>
        public int SecondsToErrorForNoInteractiveComponent = 5;

        /// <summary>
        /// Delay time for long tap
        /// </summary>
        public int LongTapDelayMillis = 1000;

        /// <summary>
        /// Random generator
        /// </summary>
        public IRandom Random = new RandomImpl(DateTime.Now.Millisecond);

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger = Debug.unityLogger;
    }
}
