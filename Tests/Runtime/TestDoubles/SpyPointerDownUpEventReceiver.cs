﻿// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Pointer down/up event receiver using EventTrigger
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    [RequireComponent(typeof(EventTrigger))]
    internal class SpyPointerDownUpEventReceiver : MonoBehaviour
    {
        private void Log([CallerMemberName] string member = null)
        {
            Debug.Log($"{this.name}.{member}");
        }

        private static EventTrigger.Entry CreateEntry(EventTriggerType type, UnityAction<BaseEventData> call)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(call);
            return entry;
        }

        private void ReceivePointerDown(BaseEventData eventData) => Log();
        private void ReceivePointerUp(BaseEventData eventData) => Log();

        private void Awake()
        {
            var eventTrigger = this.GetComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateEntry(EventTriggerType.PointerDown, ReceivePointerDown));
            eventTrigger.triggers.Add(CreateEntry(EventTriggerType.PointerUp, ReceivePointerUp));
        }
    }
}
