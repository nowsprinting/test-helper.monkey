// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey
{
    public enum GameObjectType
    {
        /// <summary>
        /// Find only active <c>GameObject</c>.
        /// </summary>
        Active,

        /// <summary>
        /// Find only pass hit test using raycaster.
        /// </summary>
        Reachable,

        /// <summary>
        /// Find only attached <c>EventTrigger</c> component or implements <c>IEventSystemHandler</c>.
        /// </summary>
        Interactive,
    }
}
