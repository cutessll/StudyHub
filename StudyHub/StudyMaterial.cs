namespace StudyHubPrototype;

public class StudyMaterial
{
    // Новий клас для реалізації зв'язків
    public string Title { get; set; }
    public SubjectCategory Subject { get; set; }
    
    
    public StudyMaterial(string title, SubjectCategory subject)
    {
        Title = title;
        Subject = subject;
    }
}