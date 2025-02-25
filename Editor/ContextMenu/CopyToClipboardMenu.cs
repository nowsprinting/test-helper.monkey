// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Extensions;
using UnityEditor;

namespace TestHelper.Monkey.Editor.ContextMenu
{
    public static class CopyToClipboardMenu
    {
        private const string Prefix = "GameObject/Copy to Clipboard/";

        [MenuItem(Prefix + "Hierarchy Path")]
        private static void CopyHierarchyPathMenuItem()
        {
            var selectedTransform = Selection.activeTransform;
            if (selectedTransform == null)
            {
                return;
            }

            var path = selectedTransform.GetPath();
            EditorGUIUtility.systemCopyBuffer = path;
        }

        [MenuItem(Prefix + "Instance ID")]
        private static void CopyInstanceIdMenuItem()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = selected.GetInstanceID().ToString();
        }
    }
}
