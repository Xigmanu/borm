namespace Borm.Model.Validation;

internal static class ValidatorFunctionWrapper
{
    public static ObjectValidator Wrap<T>(IObjectValidator<T> validator) where T : class =>
        (o, _) => validator.Validate((T)o);

    public static ObjectValidator Wrap<T>(Func<T, ValidationResult> validatorFunc) where T : class =>
        (o, _) => validatorFunc((T)o);
}