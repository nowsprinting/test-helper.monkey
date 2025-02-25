// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using TestHelper.Monkey.Operators;
using UnityEngine;

namespace TestHelper.Monkey.LogMessageBuilders
{
    /// <summary>
    /// Build log message for operation.
    /// </summary>
    [Obsolete("Use OperationRecorder instead.")]
    public class OperationMessageBuilder
    {
        private readonly Component _component;
        private readonly IOperator _operator;
        private readonly List<string> _comments = new List<string>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="component">Operate target component</param>
        /// <param name="operator">Operator</param>
        public OperationMessageBuilder(Component component, IOperator @operator)
        {
            _component = component;
            _operator = @operator;
        }

        /// <summary>
        /// Add comment to the message.
        /// </summary>
        /// <param name="comment"></param>
        public void AddComment(string comment)
        {
            _comments.Add(comment);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"{_operator.GetType().Name} operates to {_component.gameObject.name}");

            if (_comments.Count > 0)
            {
                builder.Append(" (");
                builder.Append(string.Join(", ", _comments));
                builder.Append(")");
            }

            return builder.ToString();
        }
    }
}
