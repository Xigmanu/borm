namespace Borm;

public sealed class TypeMismatchException : InvalidOperationException
{
    private readonly Type _actual;
    private readonly Type _expected;

    public TypeMismatchException(string message, Type expected, Type actual)
        : base(message)
    {
        _expected = expected;
        _actual = actual;
    }

    public Type Actual => _actual;

    public Type Expected => _expected;
}
