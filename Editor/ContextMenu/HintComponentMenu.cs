// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey;
using TestHelper.Monkey.Hints;
using UnityEditor;
using UnityEngine;

namespace Editor.ContextMenu
{
    /// <summary>
    /// Attaches interactive component hints
    /// </summary>
    public static class HintComponentMenu
    {
        [MenuItem("GameObject/TestHelper.Monkey/Interactive Component Hint Object")] // on Hierarchy window
        private static void CreateInteractiveComponentHintObjectMenuItem()
        {
            new GameObject("Interactive Component Hint").AddComponent<InteractiveComponentHint>();
        }
    }
}
