namespace StudyHubPrototype;

public enum SubjectCategory
{
    Programming,
    Mathematics,
    Physics,
    History,
    ForeignLanguage
}
public class StudyMaterial
{
    // Новий клас для реалізації зв'язків
    public string Title { get; set; }
    public string Subject { get; set; }
    
    public string SubjectCategory { get; set; }

    public StudyMaterial(string title, string subject)
    {
        Title = title;
        Subject = subject;
    }
}