namespace Borm.Model.Validation;

internal static class ValidatorFunctionWrapper
{
    public static Func<object, ValidationResult> Wrap<T>(IObjectValidator<T> validator) where T : class =>
        o => validator.Validate((T)o);

    public static Func<object, ValidationResult> Wrap<T>(Func<T, ValidationResult> validatorFunc) where T : class =>
        o => validatorFunc((T)o);
}