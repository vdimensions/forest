﻿using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    public sealed class InvokeCommandInstruction : ForestInstruction
    {
        public InvokeCommandInstruction(string key, string commandName, object commandArg)
        {
            Key = key;
            CommandName = commandName;
            CommandArg = commandArg;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is InvokeCommandInstruction ic
                && cmp.Equals(Key, ic.Key)
                && cmp.Equals(CommandName, ic.CommandName)
                && Equals(CommandArg, ic.CommandArg);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Key, CommandName, CommandArg);

        public string Key { get; }
        public string CommandName { get; }
        public object CommandArg { get; }
    }
}