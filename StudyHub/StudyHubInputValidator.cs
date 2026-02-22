using System.Text.RegularExpressions;

namespace StudyHub;

public static partial class StudyHubInputValidator
{
    private const int MinLoginLength = 3;
    private const int MaxLoginLength = 32;
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 64;
    private const int MaxMaterialTitleLength = 120;
    private const int MaxDescriptionLength = 500;
    private const int MinAdminTokenLength = 6;

    [GeneratedRegex("^[a-zA-Z0-9._-]+$")]
    private static partial Regex LoginRegex();

    public static bool TryNormalizeLogin(string? login, out string normalized, out string error)
    {
        normalized = login?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            error = "Логін не може бути порожнім.";
            return false;
        }

        if (normalized.Length < MinLoginLength || normalized.Length > MaxLoginLength)
        {
            error = $"Логін має містити від {MinLoginLength} до {MaxLoginLength} символів.";
            return false;
        }

        if (!LoginRegex().IsMatch(normalized))
        {
            error = "Логін може містити лише латинські літери, цифри та символи ._-";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryValidatePassword(string? password, out string normalized, out string error)
    {
        normalized = password?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            error = "Пароль не може бути порожнім.";
            return false;
        }

        if (normalized.Length < MinPasswordLength || normalized.Length > MaxPasswordLength)
        {
            error = $"Пароль має містити від {MinPasswordLength} до {MaxPasswordLength} символів.";
            return false;
        }

        if (!normalized.Any(char.IsDigit))
        {
            error = "Пароль має містити щонайменше одну цифру.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryValidateStudentId(int studentId, out string error)
    {
        if (studentId <= 0)
        {
            error = "ID має бути додатним числом.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryNormalizeMaterialTitle(string? title, out string normalized, out string error)
    {
        normalized = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            error = "Назва матеріалу не може бути порожньою.";
            return false;
        }

        if (normalized.Length > MaxMaterialTitleLength)
        {
            error = $"Назва матеріалу не може перевищувати {MaxMaterialTitleLength} символів.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryNormalizeDescription(string? description, out string normalized, out string error)
    {
        normalized = description?.Trim() ?? string.Empty;
        if (normalized.Length > MaxDescriptionLength)
        {
            error = $"Опис не може перевищувати {MaxDescriptionLength} символів.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryValidateAdminToken(string? token, out string normalized, out string error)
    {
        normalized = token?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            error = "Admin token модератора не може бути порожнім.";
            return false;
        }

        if (normalized.Length < MinAdminTokenLength)
        {
            error = $"Admin token має містити щонайменше {MinAdminTokenLength} символів.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
