namespace StudyHubPrototype;

public class User
{
    // Клас User - верхній рівень ієрархії
    
    private string _password;
    public string Login { get; set; }

    public string Password
    {
        get { return "**********"; }
        set
        {
            if (value.Length >= 6) _password = value;
            else Console.WriteLine("Система: Пароль занадто короткий");
        }
    }

    public void SearchMaterial()
    {
        Console.WriteLine($"[User] {Login} шукає матеріали...");
    }

    public void DownloadFile()
    {
        Console.WriteLine($"[User] {Login} завантажує файл.");
    }
}