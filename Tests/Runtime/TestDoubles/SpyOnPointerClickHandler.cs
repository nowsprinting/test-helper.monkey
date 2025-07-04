// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Spy implementation of IPointerClickHandler that records click events with timestamps, counts, and logging.
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpyOnPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Gets the list of timestamps when OnPointerClick was called.
        /// </summary>
        public List<DateTime> ClickTimestamps { get; private set; } = new List<DateTime>();

        /// <summary>
        /// Gets a value indicating whether OnPointerClick was called.
        /// </summary>
        public bool WasClicked => ClickTimestamps.Count > 0;

        /// <summary>
        /// Gets the number of times OnPointerClick was called.
        /// </summary>
        public int ClickCount => ClickTimestamps.Count;

        /// <summary>
        /// Resets the spy state.
        /// </summary>
        public void Reset()
        {
            ClickTimestamps.Clear();
        }

        private void Log([CallerMemberName] string member = "")
        {
            Debug.Log($"{this.name}.{member}");
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            Log();
            ClickTimestamps.Add(DateTime.Now);
        }
    }
}
