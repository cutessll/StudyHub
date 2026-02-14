namespace StudyHubPrototype;

// Порівняння матеріалів потрібне для коректної роботи HashSet в обраному
public class StudyMaterial : IEquatable<StudyMaterial>
{
    // Новий клас для реалізації зв'язків
    public string Title { get; set; }
    public SubjectCategory Subject { get; set; }
    
    
    public StudyMaterial(string title, SubjectCategory subject)
    {
        Title = title;
        Subject = subject;
    }

    // Матеріали вважаємо однаковими за назвою та предметом
    public bool Equals(StudyMaterial? other)
    {
        if (other is null) return false;
        return Title == other.Title && Subject == other.Subject;
    }

    public override bool Equals(object? obj) => Equals(obj as StudyMaterial);

    public override int GetHashCode() => HashCode.Combine(Title, Subject);
}
