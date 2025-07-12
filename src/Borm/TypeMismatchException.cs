namespace Borm;

public sealed class TypeMismatchException : InvalidOperationException
{
    public TypeMismatchException(string message, Type expected, Type actual)
        : base(message)
    {
        Expected = expected;
        Actual = actual;
    }

    public Type Actual { get; }

    public Type Expected { get; }
}
