using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Forest.Collections.Immutable
{
    public static class ImmutableStack
    {
        public static ImmutableStack<T> Create<T>()
            => new ImmutableStack<T>(System.Collections.Immutable.ImmutableStack.Create<T>());
        
        public static ImmutableStack<T> CreateRange<T>(IEnumerable<T> items)
            => new ImmutableStack<T>(System.Collections.Immutable.ImmutableStack.CreateRange<T>(items));
    }
    public class ImmutableStack<T> : IReadOnlyCollection<T>
    {
        public static readonly ImmutableStack<T> Empty = ImmutableStack.Create<T>();
        
        private readonly System.Collections.Immutable.ImmutableStack<T> _impl;

        internal ImmutableStack(System.Collections.Immutable.ImmutableStack<T> impl)
        {
            _impl = impl;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>) _impl).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Removes all objects from the immutable stack.</summary>
        /// <returns>An empty immutable stack.</returns>
        public ImmutableStack<T> Clear() => ImmutableStack.CreateRange(_impl.Clear());

        /// <summary>Inserts an element at the top of the immutable stack and returns the new stack.</summary>
        /// <param name="value">The element to push onto the stack.</param>
        /// <returns>The new stack.</returns>
        public ImmutableStack<T> Push(T value) => ImmutableStack.CreateRange(_impl.Push(value));

        /// <summary>Removes the element at the top of the immutable stack and returns the new stack.</summary>
        /// <returns>The new stack; never <c>null</c></returns>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        public ImmutableStack<T> Pop() => ImmutableStack.CreateRange(_impl.Pop());
        
        public ImmutableStack<T> Pop(out T item) => ImmutableStack.CreateRange(_impl.Pop(out item));

        /// <summary>Returns the element at the top of the immutable stack without removing it.</summary>
        /// <returns>The element at the top of the stack.</returns>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        public T Peek() => _impl.Peek();

        /// <summary>Gets a value that indicates whether this immutable stack is empty.</summary>
        /// <returns>
        /// <see langword="true" /> if this stack is empty; otherwise,<see langword="false" />.</returns>
        public bool IsEmpty => _impl.IsEmpty;

        public int Count => this.Count();
    }
}