using System.Collections;
using System.Collections.Generic;
using Forest.Collections;

namespace Forest.Commands
{
    /// <summary>
    /// A class that serves as a read-only collection of <see cref="ICommand">command</see> objects. 
    /// </summary>
    public sealed class CommandBag : ReadOnlyBag<string, ICommand>, IEnumerable<ICommand>
    {
        internal CommandBag(IDictionary<string, ICommand> dictionary) : base(dictionary) { }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<ICommand> GetEnumerator() => Values.GetEnumerator();

        new public IEnumerable<string> Keys => base.Keys;
    }
}