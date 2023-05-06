// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using TestHelper.Monkey.Random;
using UnityEngine;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Run configuration for monkey testing
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MonkeyConfig
    {
        /// <summary>
        /// Running time
        /// </summary>
        public TimeSpan Lifetime = new TimeSpan(0, 0, 1, 0); // 1min

        /// <summary>
        /// Delay time between operations
        /// </summary>
        public int DelayMillis = 200;

        /// <summary>
        /// Seconds to determine that an error has occurred when an object that can be interacted with does not exist
        /// </summary>
        public int SecondsToErrorForNoInteractiveComponent = 5;

        /// <summary>
        /// Delay time for touch-and-hold
        /// </summary>
        public int TouchAndHoldDelayMillis = 1000;

        /// <summary>
        /// Random generator
        /// </summary>
        public IRandom Random = new RandomImpl(Environment.TickCount);

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger = Debug.unityLogger;
    }
}
