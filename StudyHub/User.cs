namespace StudyHub;

public class User
{
    public string Login { get; set; }
    private string _password;
    public List<StudyMaterial> DownloadedMaterials { get; } = new();

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
            if (!string.IsNullOrWhiteSpace(value) && value.Length >= 6)
            {
                _password = value;
            }
        }
    }

    public bool VerifyPassword(string password) =>
        !string.IsNullOrWhiteSpace(password) && _password == password;

    public virtual IReadOnlyList<StudyMaterial> SearchMaterial(
        IEnumerable<StudyMaterial> materials,
        string? query)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return materials.ToList().AsReadOnly();
        }

        return materials
            .Where(m => m.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public virtual bool DownloadFile(StudyMaterial material)
    {
        if (DownloadedMaterials.Any(m => m.Equals(material)))
        {
            return false;
        }

        DownloadedMaterials.Add(material);
        return true;
    }

    public virtual string DisplayInfo() => $"Користувач: {Login}";
}
