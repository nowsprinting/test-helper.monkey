// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.ScreenPointStrategies;
using TestHelper.Random;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [Obsolete]
        public int TouchAndHoldDelayMillis { get; set; } = 1000;

        /// <summary>
        /// Random number generator
        /// </summary>
        public IRandom Random { get; set; } = new RandomWrapper();

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger { get; set; } = Debug.unityLogger;

        /// <summary>
        /// Function returns the screen position where monkey operators operate on for the specified gameObject
        /// </summary>
        [Obsolete]
        public Func<GameObject, Vector2> ScreenPointStrategy { get; set; } = DefaultScreenPointStrategy.GetScreenPoint;

        /// <summary>
        /// Function returns the <c>GameObject</c> is reachable from user or not.
        /// </summary>
        public Func<GameObject, Func<GameObject, Vector2>, PointerEventData, List<RaycastResult>, bool>
            IsReachable { get; set; } = DefaultReachableStrategy.IsReachable;

        /// <summary>
        /// Function returns the <c>Component</c> is interactable or not.
        /// </summary>
        public Func<Component, bool> IsInteractable { get; set; } = DefaultComponentInteractableStrategy.IsInteractable;

        /// <summary>
        /// Show Gizmos on <c>GameView</c> during running monkey test if true
        /// </summary>
        public bool Gizmos { get; set; }

        /// <summary>
        /// Take screenshots during running the monkey test if set a <c>ScreenshotOptions</c> instance.
        /// </summary>
        public ScreenshotOptions Screenshots { get; set; }

        /// <summary>
        /// Operators that the monkey performs.
        /// </summary>
        public IEnumerable<IOperator> Operators { get; set; } = new IOperator[]
        {
            new UGUIClickOperator(), // Specify screen click point strategy as a constructor argument, if necessary
            new UGUIClickAndHoldOperator(), // Specify screen click point strategy and hold millis, if necessary
            new UGUITextInputOperator(), // Specify random text input strategy, if necessary
        };
    }
}
