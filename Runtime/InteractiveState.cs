// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.Monkey
{
    public enum InteractiveState // TODO: naming
    {
        None, // Not initialized or inactive
        Ignore, // Attach IgnoreAnnotation
        Unreachable, // Not really interactive from user or not interactable
        Reachable, // Really interactive from user
        OperationTarget, // Operation target on the current step
    }
}
