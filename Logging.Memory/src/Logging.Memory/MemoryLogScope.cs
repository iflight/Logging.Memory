using System;
using System.Threading;

namespace Logging.Memory
{
    public class MemoryLogScope
    {
        private readonly string name;
        private readonly object state;

        internal MemoryLogScope(string name, object state)
        {
            this.name = name;
            this.state = state;
        }

        public MemoryLogScope Parent { get; private set; }

        private static AsyncLocal<MemoryLogScope> _value = new AsyncLocal<MemoryLogScope>();
        public static MemoryLogScope Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new MemoryLogScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        public override string ToString()
        {
            return state?.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
