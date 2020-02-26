using System.Threading;

namespace SocketHook.HostedWpfSample.Threading
{
    public sealed class SafeState
    {
        private long _state;

        public bool Value
        {
            get => Interlocked.Read(ref _state) == 1;
            set
            {
                if (value) SetTrue();
                else SetFalse();
            }
        }

        public void SetTrue() => Interlocked.CompareExchange(ref _state, 1, 0);
        public void SetFalse() => Interlocked.CompareExchange(ref _state, 0, 1);

        public static implicit operator bool(SafeState state) => state.Value;
    }
}
