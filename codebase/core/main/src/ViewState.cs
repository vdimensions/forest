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
        public static ViewState Create(object model, string resourceBundle)
        {
            return new ViewState(model.VerifyArgument(nameof(model)).IsNotNull().Value, resourceBundle, null);
        }

        public static ViewState UpdateModel(ViewState viewState, object model)
        {
            model.VerifyArgument(nameof(model)).IsNotNull();
            return new ViewState(model, viewState.ResourceBundle, viewState.DisabledCommands);
        }

        public static ViewState DisableCommand(ViewState viewState, string command)
        {
            command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
            return new ViewState(viewState.Model, viewState.ResourceBundle, viewState.DisabledCommands.Add(command));
        }
        public static ViewState EnableCommand(ViewState viewState, string command)
        {
            command.VerifyArgument(nameof(command)).IsNotNullOrEmpty();
            return new ViewState(viewState.Model, viewState.ResourceBundle, viewState.DisabledCommands.Remove(command));
        }

        public static ViewState AssignResourceBundle(ViewState viewState, string resourceBundle)
        {
            return new ViewState(viewState.Model, resourceBundle, viewState.DisabledCommands);
        }

        public static readonly ViewState Empty;

        private readonly ImmutableHashSet<string> _disabledCommands;

        private ViewState(object model, string resourceBundle, ImmutableHashSet<string> disabledCommands = null)
        {
            Model = model;
            _disabledCommands = disabledCommands;
            ResourceBundle = resourceBundle;
        }

        public bool Equals(ViewState other)
        {
            return Equals(Model, other.Model)
                && (ReferenceEquals(DisabledCommands, other.DisabledCommands) || DisabledCommands.SetEquals(other.DisabledCommands))
                && StringComparer.Ordinal.Equals(ResourceBundle, other.ResourceBundle);
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
        public ImmutableHashSet<string> DisabledCommands => _disabledCommands ?? ImmutableHashSet<string>.Empty;
        public string ResourceBundle { get; }
    }
}
