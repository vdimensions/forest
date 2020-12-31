using System;
using System.Collections.Generic;
using System.Linq;

namespace Forest
{
    public sealed class TreeChangeScope : IDisposable
    {
        private readonly IDictionary<string, Tuple<uint, uint>> _revisions = new Dictionary<string, Tuple<uint, uint>>(StringComparer.Ordinal);

        public TreeChangeScope(Tree tree)
        {
            var maxDetectedRevision = 0u;
            foreach (var node in tree.Reverse())
            {
                if (maxDetectedRevision < node.Revision)
                {
                    maxDetectedRevision = node.Revision;
                }
                _revisions.Add(node.Key, Tuple.Create(node.Revision, node.Revision));
            }
            TargetRevision = 1u + maxDetectedRevision;
        }

        private Tree.Node BumpNodeRevision(Tree.Node node)
        {
            var n = node;
            while (n.Revision < TargetRevision)
            {
                n = n.UpdateRevision();
            }
            return n;
        }

        public Tree.Node UpdateRevision(Tree.Node node)
        {
            var n = BumpNodeRevision(node);
            if (!_revisions.TryGetValue(node.Key, out var existing))
            {
                _revisions.Add(node.Key, Tuple.Create(node.Revision, n.Revision));
            }
            else
            {
                _revisions[node.Key] = Tuple.Create(existing.Item1, n.Revision);
            }
            return n;
        }

        void IDisposable.Dispose()
        {
            _revisions.Clear();
        }

        public IEnumerable<string> ChangedViewKeys => _revisions
            .Where(x => x.Value.Item2 == TargetRevision)
            .Select(x => x.Key);

        public uint TargetRevision { get; }
    }
}