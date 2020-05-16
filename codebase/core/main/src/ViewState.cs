using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Axle.Verification;

namespace Forest
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [StructLayout(LayoutKind.Sequential)]
    public struct ViewState : IEquatable<ViewState>
    {
        public static ViewState Create(object model) => new ViewState(model.VerifyArgument(nameof(model)).IsNotNull().Value);

        public static ViewState UpdateModel(ViewState viewState, object model)
        {
            model.VerifyArgument(nameof(model)).IsNotNull();
            return new ViewState(model, viewState.DisabledCommands, viewState.DisabledLinks);
        }

        public static ViewState DisableCommand(ViewState viewState, string command)
        {
            command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
            return new ViewState(viewState.Model, viewState.DisabledCommands.Add(command), viewState.DisabledLinks);
        }
        public static ViewState EnableCommand(ViewState viewState, string command)
        {
            command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
            return new ViewState(viewState.Model, viewState.DisabledCommands.Remove(command), viewState.DisabledLinks);
        }

        // public static ViewState DisableLink(ViewState viewState, string link)
        // {
        //     link.VerifyArgument(nameof(link)).IsNotNullOrEmpty();
        //     return new ViewState(viewState.Model, viewState.DisabledCommands, viewState.DisabledLinks.Add(link));
        // }
        // public static ViewState EnableLink(ViewState viewState, string link)
        // {
        //     link.VerifyArgument(nameof(link)).IsNotNullOrEmpty();
        //     return new ViewState(viewState.Model, viewState.DisabledCommands, viewState.DisabledLinks.Remove(link));
        // }

        public static readonly ViewState Empty;

        private readonly IImmutableSet<string> _disabledCommands;
        private readonly IImmutableSet<string> _disabledLinks;

        private ViewState(object model, IImmutableSet<string> disabledCommands = null, IImmutableSet<string> disabledLinks = null)
        {
            Model = model;
            _disabledCommands = disabledCommands;
            _disabledLinks = disabledLinks;
        }

        public bool Equals(ViewState other)
        {
            return Enumerable.SequenceEqual(DisabledCommands, other.DisabledCommands) 
                && Enumerable.SequenceEqual(DisabledLinks, other.DisabledLinks) 
                && Equals(Model, other.Model);
        }
        public override bool Equals(object obj) => obj is ViewState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DisabledCommands.GetHashCode();
                hashCode = (hashCode * 397) ^ DisabledLinks.GetHashCode();
                hashCode = (hashCode * 397) ^ (Model != null ? Model.GetHashCode() : 0);
                return hashCode;
            }
        }

        public object Model { get; }
        public IImmutableSet<string> DisabledCommands => _disabledCommands ?? ImmutableHashSet.Create<string>(StringComparer.Ordinal);
        public IImmutableSet<string> DisabledLinks => _disabledLinks ?? ImmutableHashSet.Create<string>(StringComparer.Ordinal);
    }
}
