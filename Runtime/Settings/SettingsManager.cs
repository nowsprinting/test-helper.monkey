// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEditor; // TODO: use only in Editor
using UnityEngine;

namespace TestHelper.Monkey.Settings
{
    public static class SettingsManager
    {
        private const string PackageName = "com.nowsprinting.test-helper.monkey";
        private static UnityEditor.SettingsManagement.Settings s_instance;

        internal static UnityEditor.SettingsManagement.Settings Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new UnityEditor.SettingsManagement.Settings(PackageName);

                return s_instance;
            }
        }

        public static Color IgnoreColor => Instance.Get<Color>(
            "IgnoreColor", SettingsScope.Project, Color.red);

        public static Color UnreachableColor => Instance.Get<Color>(
            "UnreachableColor", SettingsScope.Project, new Color(0xef, 0x81, 0x0f));

        public static Color ReachableColor => Instance.Get<Color>(
            "ReachableColor", SettingsScope.Project, Color.yellow);

        public static Color OperationTargetColor => Instance.Get<Color>(
            "OperationTargetColor", SettingsScope.Project, Color.green);
    }
}
