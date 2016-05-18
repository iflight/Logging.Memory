using System;
#if NET451
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif

namespace iflight.Logging
{
    public class MemoryLogScope
    {
        private readonly string _name;
        private readonly object _state;

        internal MemoryLogScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public MemoryLogScope Parent { get; private set; }

#if NET451
        private static string FieldKey = typeof(MemoryLogScope).FullName + ".Value";
        public static MemoryLogScope Current
        {
            get
            {
                var handle = CallContext.LogicalGetData(FieldKey) as ObjectHandle;
                if (handle == null)
                {
                    return default(MemoryLogScope);
                }

                return (MemoryLogScope)handle.Unwrap();
            }
            set
            {
                CallContext.LogicalSetData(FieldKey, new ObjectHandle(value));
            }
        }
#else
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
#endif

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new MemoryLogScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        public override string ToString()
        {
            return _state?.ToString();
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
