// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators.Utils
{
    /// <summary>
    /// Simulator class to reproduce pointer events.
    /// </summary>
    public sealed class PointerEventSimulator : IDisposable
    {
        private readonly GameObject _gameObject;
        private readonly ILogger _logger;
        private readonly bool _hasSelectable;
        private readonly SimulatedPointerEventData _eventData;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObject">Click target <c>GameObject</c></param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the game-title implementation.</param>
        /// <param name="logger">Logger set if you need</param>
        public PointerEventSimulator(GameObject gameObject, RaycastResult raycastResult, ILogger logger = null)
        {
            _gameObject = gameObject;
            _logger = logger;
            _hasSelectable = gameObject.GetComponent<Selectable>() != null;
            _eventData = new SimulatedPointerEventData(gameObject, raycastResult);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _eventData.Dispose();
        }

        /// <summary>
        /// Reproduce the sequence of events that happen when pointer-clicking.
        /// <list type="number">
        ///     <item><c>OnPointerEnter</c> (once at the beginning)</item>
        ///     <item><c>OnSelect</c> (once at the beginning, if <c>Selectable</c> is attached)</item>
        ///     <item>For each click:</item>
        ///     <item>-- <c>OnPointerDown</c></item>
        ///     <item>-- <c>OnInitializePotentialDrag</c></item>
        ///     <item>-- Wait for the hold time</item>
        ///     <item>-- <c>OnPointerUp</c></item>
        ///     <item>-- <c>OnPointerClick</c></item>
        ///     <item>-- Wait for interval</item>
        ///     <item><c>OnPointerExit</c> (once at the end)</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// <c>OnDeselect</c> event is called by the system when the focus moves to another element, so it is not called in this method.
        /// </remarks>
        /// <param name="holdMillis">Hold time in milliseconds if click-and-hold</param>
        /// <param name="clickCount">Number of clicks</param>
        /// <param name="intervalMillis">Interval between clicks in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async UniTask PointerClickAsync(int holdMillis = 0, int clickCount = 1, int intervalMillis = 0,
            CancellationToken cancellationToken = default)
        {
            var gameObjectNameCache = _gameObject.name;

            // Enter (once at the beginning)
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerEnterHandler);
            if (_hasSelectable)
            {
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.selectHandler);
            }

            // Multiple clicks
            for (var i = 0; i < clickCount; i++)
            {
                // Down
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.initializePotentialDrag);

                if (holdMillis > 0)
                {
                    await UniTask.Delay(holdMillis, ignoreTimeScale: true, cancellationToken: cancellationToken);
                }
                else
                {
                    await UniTask.NextFrame(cancellationToken: cancellationToken);
                }

                // Up
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerUpHandler);
                _eventData.SetStateToClicking();
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerClickHandler);
                _eventData.SetStateToClicked();

                // Wait for interval for multiple clicks
                if (intervalMillis <= 0)
                {
                    continue;
                }

                await UniTask.Delay(intervalMillis, ignoreTimeScale: true, cancellationToken: cancellationToken);
                if (_gameObject != null)
                {
                    continue;
                }

                _logger?.Log($"{gameObjectNameCache} is destroyed before pointer-up event.");
                return;
            }

            // Exit (once at the end)
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerExitHandler);
        }
    }
}
