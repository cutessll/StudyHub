namespace StudyHub;

public class Moderator : Student
{
    private string _adminToken = string.Empty;

    public string AdminToken
    {
        get { return string.IsNullOrWhiteSpace(_adminToken) ? "No_Token" : "Token_Active"; }
        set { _adminToken = value; }
    }

    public Moderator(string login, string password, int studentId, string adminToken) : base(login, password, studentId)
    {
        AdminToken = adminToken;
    }

    public enum ModerationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public bool ValidateAdminToken(string token) =>
        !string.IsNullOrWhiteSpace(token) && _adminToken == token;

    public bool DeleteFile(ICollection<StudyMaterial> materials, StudyMaterial material)
    {
        return materials.Remove(material);
    }

    public bool BlockUser(User user, ISet<string> blockedUsers)
    {
        if (user.Login.Equals(Login, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return blockedUsers.Add(user.Login);
    }

    public override string DisplayInfo() => $"Модератор: {Login}, ID: {StudentID}, Token: {AdminToken}";
}
