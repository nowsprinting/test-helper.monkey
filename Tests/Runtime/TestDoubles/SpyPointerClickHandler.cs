// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Spy implementation of IPointerClickHandler that records click events.
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpyPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Gets a value indicating whether OnPointerClick was called.
        /// </summary>
        public bool WasClicked { get; private set; }

        /// <summary>
        /// Gets the number of times OnPointerClick was called.
        /// </summary>
        public int ClickCount { get; private set; }

        /// <summary>
        /// Resets the spy state.
        /// </summary>
        public void Reset()
        {
            WasClicked = false;
            ClickCount = 0;
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            WasClicked = true;
            ClickCount++;
        }
    }
}