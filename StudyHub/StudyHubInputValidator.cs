using System.ComponentModel.DataAnnotations;

namespace StudyHub;

public static class StudyHubInputValidator
{
    private const int MinLoginLength = 3;
    private const int MaxLoginLength = 32;
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 64;
    private const int MaxMaterialTitleLength = 120;
    private const int MaxDescriptionLength = 500;
    private const int MinAdminTokenLength = 6;

    public static bool TryNormalizeLogin(string? login, out string normalized, out string error)
    {
        normalized = login?.Trim() ?? string.Empty;
        return TryValidate(new LoginInputModel { Value = normalized }, out error);
    }

    public static bool TryValidatePassword(string? password, out string normalized, out string error)
    {
        normalized = password?.Trim() ?? string.Empty;
        return TryValidate(new PasswordInputModel { Value = normalized }, out error);
    }

    public static bool TryValidateStudentId(int studentId, out string error)
    {
        return TryValidate(new StudentIdInputModel { Value = studentId }, out error);
    }

    public static bool TryNormalizeMaterialTitle(string? title, out string normalized, out string error)
    {
        normalized = title?.Trim() ?? string.Empty;
        return TryValidate(new MaterialTitleInputModel { Value = normalized }, out error);
    }

    public static bool TryNormalizeDescription(string? description, out string normalized, out string error)
    {
        normalized = description?.Trim() ?? string.Empty;
        return TryValidate(new DescriptionInputModel { Value = normalized }, out error);
    }

    public static bool TryValidateAdminToken(string? token, out string normalized, out string error)
    {
        normalized = token?.Trim() ?? string.Empty;
        return TryValidate(new AdminTokenInputModel { Value = normalized }, out error);
    }

    private static bool TryValidate(object model, out string error)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        error = isValid
            ? string.Empty
            : validationResults.FirstOrDefault()?.ErrorMessage ?? "Введені дані некоректні.";

        return isValid;
    }

    private sealed class LoginInputModel
    {
        [Required(ErrorMessage = "Логін не може бути порожнім.")]
        [StringLength(MaxLoginLength, MinimumLength = MinLoginLength,
            ErrorMessage = "Логін має містити від 3 до 32 символів.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$",
            ErrorMessage = "Логін може містити лише латинські літери, цифри та символи ._-")]
        public string Value { get; init; } = string.Empty;
    }

    private sealed class PasswordInputModel
    {
        [Required(ErrorMessage = "Пароль не може бути порожнім.")]
        [StringLength(MaxPasswordLength, MinimumLength = MinPasswordLength,
            ErrorMessage = "Пароль має містити від 6 до 64 символів.")]
        [RegularExpression(@"^(?=.*\d).+$",
            ErrorMessage = "Пароль має містити щонайменше одну цифру.")]
        public string Value { get; init; } = string.Empty;
    }

    private sealed class StudentIdInputModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "ID має бути додатним числом.")]
        public int Value { get; init; }
    }

    private sealed class MaterialTitleInputModel
    {
        [Required(ErrorMessage = "Назва матеріалу не може бути порожньою.")]
        [StringLength(MaxMaterialTitleLength, MinimumLength = 1,
            ErrorMessage = "Назва матеріалу не може перевищувати 120 символів.")]
        public string Value { get; init; } = string.Empty;
    }

    private sealed class DescriptionInputModel
    {
        [StringLength(MaxDescriptionLength, ErrorMessage = "Опис не може перевищувати 500 символів.")]
        public string Value { get; init; } = string.Empty;
    }

    private sealed class AdminTokenInputModel
    {
        [Required(ErrorMessage = "Admin token модератора не може бути порожнім.")]
        [StringLength(128, MinimumLength = MinAdminTokenLength,
            ErrorMessage = "Admin token має містити щонайменше 6 символів.")]
        public string Value { get; init; } = string.Empty;
    }
}
