using System;
using System.Linq;
using System.Runtime.InteropServices;
using Axle.Verification;
using Axle.Collections.Immutable;

namespace Forest
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [StructLayout(LayoutKind.Sequential)]
    public struct ViewState : IEquatable<ViewState>
    {
        public static ViewState Create(object model, string resourceBundle = null) => new ViewState(
            model.VerifyArgument(nameof(model)).IsNotNull().Value, null, resourceBundle);

        public static ViewState UpdateModel(ViewState viewState, object model)
        {
            model.VerifyArgument(nameof(model)).IsNotNull();
            return new ViewState(model, viewState.DisabledCommands, viewState.ResourceBundle);
        }

        public static ViewState DisableCommand(ViewState viewState, string command)
        {
            command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
            return new ViewState(viewState.Model, viewState.DisabledCommands.Add(command), viewState.ResourceBundle);
        }
        public static ViewState EnableCommand(ViewState viewState, string command)
        {
            command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
            return new ViewState(viewState.Model, viewState.DisabledCommands.Remove(command), viewState.ResourceBundle);
        }

        public static ViewState AssignResourceBundle(ViewState viewState, string resourceBundle)
        {
            return new ViewState(viewState.Model, viewState.DisabledCommands, resourceBundle);
        }

        public static readonly ViewState Empty;

        private readonly ImmutableHashSet<string> _disabledCommands;

        private ViewState(object model, ImmutableHashSet<string> disabledCommands = null, string resourceBundle = null)
        {
            Model = model;
            _disabledCommands = disabledCommands;
            ResourceBundle = resourceBundle;
        }

        public bool Equals(ViewState other)
        {
            return Enumerable.SequenceEqual(DisabledCommands, other.DisabledCommands) 
                && Equals(Model, other.Model);
        }
        public override bool Equals(object obj) => obj is ViewState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DisabledCommands.GetHashCode();
                hashCode = (hashCode * 397) ^ (Model != null ? Model.GetHashCode() : 0);
                return hashCode;
            }
        }

        public object Model { get; }
        public ImmutableHashSet<string> DisabledCommands => _disabledCommands ?? ImmutableHashSet.Create(StringComparer.Ordinal);
        public string ResourceBundle { get; }
    }
}
