namespace NetLeaf
{
    internal class AtomicBoolean
    {
        private volatile int _value;

        internal AtomicBoolean() : this(false)
        {

        }

        internal AtomicBoolean(bool value)
        {
            _value = value ? 1 : 0;
        }

        internal bool Get()
        {
            return _value != 0;
        }

        internal void Set(bool value)
        {
            Interlocked.Exchange(ref _value, value ? 1 : 0);
        }

        internal bool GetAndSet(bool value)
        {
            return Interlocked.Exchange(ref _value, value ? 1 : 0) != 0;
        }

        internal bool CompareAndSet(bool expected, bool result)
        {
            int e = expected ? 1 : 0;
            int r = result ? 1 : 0;

            return Interlocked.CompareExchange(ref _value, r, e) == e;
        }

        public static implicit operator bool(AtomicBoolean value)
        {
            return value.Get();
        }

    }
}
