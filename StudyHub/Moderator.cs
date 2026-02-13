namespace StudyHubPrototype;

public class Moderator:Student
{
    // Клас Moderator-успадковує Student
    public string AdminToken { get; set; }

    public void DeleteFile()
    {
        Console.WriteLine($"[Moderator] Файл видалено (Адмін-токен: {AdminToken}).");
    }

    public void BlockUser()
    {
        Console.WriteLine("[Moderator] Користувача заблоковано модератором.");
    }
}