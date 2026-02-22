namespace StudyHub;

// Порівняння матеріалів потрібне для коректної роботи HashSet в обраному
public class StudyMaterial : IEquatable<StudyMaterial>
{
    public string Title { get; set; }
    public SubjectCategory Subject { get; set; }
    public string UploadedByLogin { get; private set; }
    public string Description { get; private set; }

    public StudyMaterial(string title, SubjectCategory subject, string uploadedByLogin = "system", string description = "")
    {
        Title = title;
        Subject = subject;
        UploadedByLogin = string.IsNullOrWhiteSpace(uploadedByLogin) ? "system" : uploadedByLogin.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public bool TryUpdateDescription(string newDescription)
    {
        if (newDescription is null)
        {
            return false;
        }

        Description = newDescription.Trim();
        return true;
    }

    public bool TryUpdateUploaderLogin(string newLogin)
    {
        if (string.IsNullOrWhiteSpace(newLogin))
        {
            return false;
        }

        UploadedByLogin = newLogin.Trim();
        return true;
    }

    // Матеріали вважаємо однаковими за назвою та предметом.
    public bool Equals(StudyMaterial? other)
    {
        if (other is null) return false;
        return Title == other.Title && Subject == other.Subject;
    }

    public override bool Equals(object? obj) => Equals(obj as StudyMaterial);

    public override int GetHashCode() => HashCode.Combine(Title, Subject);
}
