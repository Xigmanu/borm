using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Model.Validation;

[ExcludeFromCodeCoverage]
internal static class ValidatorFunctionWrapper
{
    [DebuggerStepThrough]
    public static ObjectValidator Wrap<T>(IObjectValidator<T> validator) where T : class =>
        (o, _) => validator.Validate((T)o);

    [DebuggerStepThrough]
    public static ObjectValidator Wrap<T>(Func<T, ValidationResult> validatorFunc) where T : class =>
        (o, _) => validatorFunc((T)o);
}