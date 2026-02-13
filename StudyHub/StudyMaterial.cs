namespace StudyHubPrototype;

public class StudyMaterial
{
    // Новий клас для реалізації зв'язків
    public string Title { get; set; }
    public string Subject { get; set; }

    public StudyMaterial(string title, string subject)
    {
        Title = title;
        Subject = subject;
    }
}