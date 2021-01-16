using System;
using System.Collections.Generic;
using System.Linq;
#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using Axle.Collections.Immutable;
using Forest.Engine.Instructions;

namespace Forest.StateManagement
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class ChangeLog
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly ForestState _initialState;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly ForestState _finalState;

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly ImmutableList<Tuple<TreeModification, Type>> _changes;

        public ChangeLog(ForestState initialState, ForestState finalState, IEnumerable<TreeModification> changes)
        {
            _initialState = initialState;
            _finalState = finalState;
            _changes = ImmutableList.CreateRange(changes.Select(x => Tuple.Create(x, x.GetType())));
        }

        public ForestState InitialState => _initialState;

        public ForestState FinalState => _finalState;

        public IEnumerable<TreeModification> Changes => _changes.Select(x => x.Item1);
    }
}