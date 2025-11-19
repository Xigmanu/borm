namespace Borm.Model.Validation;

internal static class ValidatorFunctionWrapper
{
    public static ValidatorFunc Wrap<T>(IObjectValidator<T> validator) where T : class =>
        o => validator.Validate((T)o);

    public static ValidatorFunc Wrap<T>(Func<T, ValidationResult> validatorFunc) where T : class =>
        o => validatorFunc((T)o);
}