namespace StudyHubPrototype;

// сервіс інкапсулює всю роботу зі сховищем для консолі.
public class StudyHubService : IStudyHubService
{
    private readonly StudyHubStorage _storage;

    public StudyHubService(StudyHubStorage storage)
    {
        _storage = storage;
    }

    public Student RegisterStudent(string login, string password, int studentId)
    {
        var student = new Student(login, password, studentId);
        _storage.AddUser(student);
        return student;
    }

    public Moderator RegisterModerator(string login, string password, int studentId, string adminToken)
    {
        var moderator = new Moderator(login, password, studentId, adminToken);
        _storage.AddUser(moderator);
        return moderator;
    }

    public StudyMaterial AddMaterialToStudent(Student student, string title, SubjectCategory subject)
    {
        var material = new StudyMaterial(title, subject);
        student.AddMaterial(material);
        _storage.AddMaterial(material);
        return material;
    }

    public bool AddToFavorites(Student student, StudyMaterial material)
    {
        var countBefore = student.FavoriteMaterials.Count;
        student.SaveToFavorites(material);
        return student.FavoriteMaterials.Count > countBefore;
    }

    public IReadOnlyList<User> GetUsers() => _storage.GetUsers();

    public IReadOnlyList<User> FindUsers(string loginPart) => _storage.FindUsers(loginPart);

    public IReadOnlyList<StudyMaterial> FindMaterials(string titlePart) => _storage.FindMaterials(titlePart);

    public IReadOnlyList<StudyMaterial> GetMaterialsBySubject(SubjectCategory subject) =>
        _storage.GetMaterialsBySubject(subject);

    public bool RemoveUser(string login) => _storage.RemoveUser(login);

    public bool RemoveMaterial(string title) => _storage.RemoveMaterial(title);
}
