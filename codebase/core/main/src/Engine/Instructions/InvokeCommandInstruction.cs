using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    public sealed class InvokeCommandInstruction : ForestInstruction
    {
        public InvokeCommandInstruction(string instanceID, string commandName, object commandArg)
        {
            InstanceID = instanceID;
            CommandName = commandName;
            CommandArg = commandArg;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is InvokeCommandInstruction ic
                && cmp.Equals(InstanceID, ic.InstanceID)
                && cmp.Equals(CommandName, ic.CommandName)
                && Equals(CommandArg, ic.CommandArg);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(InstanceID, CommandName, CommandArg);

        public string InstanceID { get; }
        public string CommandName { get; }
        public object CommandArg { get; }
    }
}