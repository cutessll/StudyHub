namespace StudyHubPrototype;

public class User
{
    // Клас User - верхній рівень ієрархії
    
    public string Login { get; set; }
    private string _password;
    
    // Конструктор класу User
    public User(string login, string password)
    {
        Login = login;
        _password = password; 
    }

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