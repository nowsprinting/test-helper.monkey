// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Spy implementation of IPointerClickHandler that records click timestamps.
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpySequenceTrackingClickHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Gets the list of timestamps when OnPointerClick was called.
        /// </summary>
        public List<DateTime> ClickTimestamps { get; private set; } = new List<DateTime>();

        /// <summary>
        /// Resets the spy state.
        /// </summary>
        public void Reset()
        {
            ClickTimestamps.Clear();
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            ClickTimestamps.Add(DateTime.Now);
        }
    }
}