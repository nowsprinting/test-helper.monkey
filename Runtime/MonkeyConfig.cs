// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using TestHelper.Monkey.Random;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using IRandom = TestHelper.Random.IRandom;
using RandomImpl = TestHelper.Random.RandomImpl;

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
        /// Random number generator
        /// </summary>
        public IRandom Random = new RandomImpl();

        /// <summary>
        /// Random string generator
        /// </summary>
        public IRandomString RandomString = new RandomStringImpl(new RandomImpl());

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger = Debug.unityLogger;

        /// <summary>
        /// Function returns the screen position where monkey operators operate on for the specified gameObject
        /// </summary>
        public Func<GameObject, Vector2> ScreenPointStrategy = DefaultScreenPointStrategy.GetScreenPoint;

        /// <summary>
        /// Function returns the random string generation parameters
        /// </summary>
        public Func<GameObject, RandomStringParameters>
            RandomStringParametersStrategy = DefaultRandomStringParameterGen;

        /// <summary>
        /// Show Gizmos on <c>GameView</c> during running monkey test if true
        /// </summary>
        public bool Gizmos = false;

        /// <summary>
        /// Take screenshots during running monkey test if true.
        /// </summary>
        public bool TakeScreenshots = false;

        /// <summary>
        /// Directory path to save screenshots.
        /// Default save path is <c>Application.persistentDataPath</c> + "/TestHelper.Monkey/Screenshots/".
        /// </summary>
        public string ScreenshotsDirectory = null;

        /// <summary>
        /// Prefix of screenshots filename.
        /// Default prefix is <c>CurrentTest.Name</c> when run in test-framework context.
        /// Using caller method name when run in runtime context.
        /// </summary>
        public string ScreenshotsFilenamePrefix = null;

        private static RandomStringParameters DefaultRandomStringParameterGen(GameObject _) =>
            RandomStringParameters.Default;
    }
}
