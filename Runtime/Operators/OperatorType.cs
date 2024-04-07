// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// <c>IOperator</c> operation types.
    /// Intended for use in capture and playback features.
    /// </summary>
    public enum OperatorType
    {
        Click, // a.k.a. tap
        ClickAndHold, // a.k.a. touch and hold, long press
        DoubleClick, // a.k.a. double tap
        TextInput, // Recommended to implement <c>ITextInputOperator</c>
        DragAndDrop,
        Swipe,
        Flick,
        Pinch,
        Hover, // Hover mouse cursor
        RightClick, // Click right button on mouse
        ScrollWheel, // Scroll wheel on mouse. scrolling up/down and tilting left/right.
    }
}
