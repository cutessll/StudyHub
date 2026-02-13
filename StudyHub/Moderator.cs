namespace StudyHubPrototype;

public class Moderator:Student
{
    // Клас Moderator-успадковує Student
    private string _adminToken;

    public string AdminToken
    {
        get { return _adminToken != null ? "Token_Active" : "No_Token"; }
        set { _adminToken = value; }
    }

    public void DeleteFile()
    {
        Console.WriteLine($"[Moderator] Файл видалено (Адмін-токен: {AdminToken}).");
    }

    public void BlockUser()
    {
        Console.WriteLine("[Moderator] Користувача заблоковано модератором.");
    }
}