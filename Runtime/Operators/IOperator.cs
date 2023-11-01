// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.Monkey.Operators
{
    public interface IOperator
    {
        public UniTask DoOperation(MonoBehaviour component, Func<GameObject, Vector2> screenPointStrategy,
            CancellationToken cancellationToken = default
        );
    }
}
