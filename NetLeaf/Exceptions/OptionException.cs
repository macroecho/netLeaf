namespace NetLeaf.Exceptions
{
    internal class OptionException : Exception
    {
        internal OptionException(string name, string message) : base(string.Concat(name, ", ", message))
        {

        }
    }
}
