// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey;
using TestHelper.Monkey.Hints;
using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey.Editor.UI.Hints
{
    /// <summary>
    /// Gizmo Drawer for <c cref="InteractiveComponentHint">InteractiveComponentHint</c>.
    /// </summary>
    public static class InteractiveComponentHintGizmoDrawer
    {
        private static readonly Vector3 s_labelOffset = new Vector3(0.1f, 0, 0);

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void OnDrawGizmos(InteractiveComponentHint hint, GizmoType _)
        {
            if (!hint.enabled)
            {
                return;
            }

            foreach (var (origPoint, camNorm, worldPoint) in hint.OriginalRelation)
            {
                Handles.color = hint.originalPointColor;
                Handles.DrawWireDisc(origPoint, camNorm, 0.1f);
                Handles.DrawLine(origPoint, worldPoint);
            }

            foreach (var worldPointAndLabel in hint.NotReallyInteractives)
            {
                var (worldPoint, camNorm) = worldPointAndLabel.Key;
                var label = worldPointAndLabel.Value;
                Handles.color = hint.unreachableColor;
                Handles.DrawWireDisc(worldPoint, camNorm, 0.1f);
                GUI.color = hint.unreachableColor;
                Handles.Label(worldPoint + s_labelOffset, label);
            }
            
            foreach (var worldPointAndLabel in hint.ReallyInteractives)
            {
                var (worldPoint, camNorm) = worldPointAndLabel.Key;
                var label = worldPointAndLabel.Value;
                Handles.color = hint.reachableColor;
                Handles.DrawWireDisc(worldPoint, camNorm, 0.1f);
                GUI.color = hint.reachableColor;
                Handles.Label(worldPoint + s_labelOffset, label);
            }
        }
    }
}
