// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestHelper.Monkey.Matchers;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.Random;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;

namespace TestHelper.Monkey
{
    public struct MatcherOperatorPair
    {
        public IComponentMatcher _matcher;
        public IOperator _operator;

        public MatcherOperatorPair(IComponentMatcher matcher, IOperator @operator)
        {
            _matcher = matcher;
            _operator = @operator;
        }
    }

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

        // First, if there is a match in the PrimaryOperators.matcher, so draw a lottery.
        public List<MatcherOperatorPair> PrimaryOperators = new List<MatcherOperatorPair>();
        // e.g., new MatcherOperatorPair(new GameObjectNameMatcher("Phone"), new TextInputOperator(CharactersKind.Digits, 9, 12))

        // If there is no match in the PrimaryOperators.matcher, so use the SecondaryOperators.
        public List<MatcherOperatorPair> SecondaryOperators = new List<MatcherOperatorPair>()
        {
            new MatcherOperatorPair(new ClickableComponentMatcher(), new ClickOperator()),
            new MatcherOperatorPair(new TouchAndHoldableComponentMatcher(), new TouchAndHoldOperator(1000)),
            // new MatcherOperatorPair(new TextInputComponentMatcher(), new TextInputOperator(CharactersKind.Alphanumeric, 5, 10)),
        };

        /// <summary>
        /// Random generator
        /// </summary>
        public IRandom Random = new RandomImpl();

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger = Debug.unityLogger;

        /// <summary>
        /// Function returns the screen position where monkey operators operate on for the specified gameObject
        /// </summary>
        public Func<GameObject, Vector2> ScreenPointStrategy = DefaultScreenPointStrategy.GetScreenPoint;

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
    }
}
