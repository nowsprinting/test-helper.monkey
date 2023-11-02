// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;

namespace TestHelper.Monkey
{
    public class InteractiveComponentGizmo : MonoBehaviour
    {
        private enum State
        {
            None, // Not initialized or inactive
            Ignore, // Attach IgnoreAnnotation
            Unreachable, // Not really interactive from user or not interactable
            Reachable, // Really interactive from user
            OperationTarget, // Operation target on the current step
        }

        private State _state = State.None;
        private string _operationLabel = null;

        public void UpdateState(bool operationTarget = false, string operationLabel = null)
        {
            if (operationTarget)
            {
                _state = State.OperationTarget;
                _operationLabel = operationLabel;
                return;
            }

            if (gameObject.GetComponent<IgnoreAnnotation>() != null)
            {
                _state = State.Ignore;
                return;
            }

            Func<GameObject, Vector2> screenPointFunction = DefaultScreenPointStrategy.GetScreenPoint;
            // TODO: Consider offset annotations

            _state = new InteractiveComponent(this).IsReallyInteractiveFromUser(screenPointFunction)
                ? State.Reachable
                : State.Unreachable;
        }

        private void OnDrawGizmos()
        {
            switch (_state)
            {
                case State.Ignore:
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: mizaru
                    break;
                case State.Unreachable:
                    Gizmos.color = new Color(0xef, 0x81, 0x0f);
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: mizaru
                    break;
                case State.Reachable:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: saru
                    break;
                case State.OperationTarget:
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: saru
                    // TODO: with draw _operationLabel
                    break;
            }
        }
    }
}
