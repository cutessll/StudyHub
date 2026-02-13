namespace StudyHubPrototype;

public class User
{
    // Клас User - верхній рівень ієрархії
    public string Login { get; set; }
    public string Password { get; set; }

    public void SearchMaterial()
    {
        Console.WriteLine($"[User] {Login} шукає матеріали...");
    }

    public void DownloadFile()
    {
        Console.WriteLine($"[User] {Login} завантажує файл.");
    }
}