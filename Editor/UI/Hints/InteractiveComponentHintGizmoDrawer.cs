// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey;
using UnityEditor;
using UnityEngine;

namespace Editor.UI
{
    /// <summary>
    /// Gizmo Drawer for <c cref="InteractiveComponentHint" />
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
                Handles.color = hint.inactivePointColor;
                Handles.DrawWireDisc(worldPoint, camNorm, 0.1f);
                GUI.color = hint.inactivePointColor;
                Handles.Label(worldPoint + s_labelOffset, label);
            }
            
            foreach (var worldPointAndLabel in hint.ReallyInteractives)
            {
                var (worldPoint, camNorm) = worldPointAndLabel.Key;
                var label = worldPointAndLabel.Value;
                Handles.color = hint.activePointColor;
                Handles.DrawWireDisc(worldPoint, camNorm, 0.1f);
                GUI.color = hint.activePointColor;
                Handles.Label(worldPoint + s_labelOffset, label);
            }
        }
    }
}
