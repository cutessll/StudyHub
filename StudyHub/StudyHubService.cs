namespace StudyHub;

// сервіс інкапсулює роботу зі сховищем для будь-якого UI (консоль/GUI).
public class StudyHubService : IStudyHubService
{
    private readonly StudyHubStorage _storage;

    public StudyHubService(StudyHubStorage storage)
    {
        _storage = storage;
    }

    public User? Authenticate(string login, string password) => _storage.Authenticate(login, password);

    public bool UserExists(string login) => _storage.UserExists(login);

    public User RegisterUser(string login, string password)
    {
        EnsureUserDoesNotExist(login);
        var user = new User(login, password);
        _storage.AddUser(user);
        return user;
    }

    public Student RegisterStudent(string login, string password, int studentId)
    {
        EnsureUserDoesNotExist(login);
        var student = new Student(login, password, studentId);
        _storage.AddUser(student);
        return student;
    }

    public Moderator RegisterModerator(string login, string password, int studentId, string adminToken)
    {
        EnsureUserDoesNotExist(login);
        var moderator = new Moderator(login, password, studentId, adminToken);
        _storage.AddUser(moderator);
        return moderator;
    }

    public StudyMaterial AddMaterialToStudent(Student student, string title, SubjectCategory subject)
    {
        var material = new StudyMaterial(title, subject);
        var added = student.UploadFile(material);
        if (added)
        {
            _storage.AddMaterial(material);
            return material;
        }

        return student.MyMaterials
            .First(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && m.Subject == subject);
    }

    public bool AddToFavorites(Student student, StudyMaterial material)
    {
        var added = student.SaveToFavorites(material);
        if (added)
        {
            _storage.Persist();
        }

        return added;
    }

    public IReadOnlyList<User> GetUsers() => _storage.GetUsers();

    public IReadOnlyList<User> FindUsers(string loginPart) => _storage.FindUsers(loginPart);

    public IReadOnlyList<StudyMaterial> FindMaterials(string titlePart) => _storage.FindMaterials(titlePart);

    public IReadOnlyList<StudyMaterial> GetMaterialsBySubject(SubjectCategory subject) =>
        _storage.GetMaterialsBySubject(subject);

    public int GetUsersCount() => _storage.GetUsers().Count;

    public int GetMaterialsCount() => _storage.GetMaterials().Count;

    public IReadOnlyList<StudyMaterial> GetStudentMaterials(Student student) =>
        student.MyMaterials.AsReadOnly();

    public IReadOnlyList<StudyMaterial> GetFavoriteMaterials(Student student) =>
        student.FavoriteMaterials.ToList().AsReadOnly();

    public bool RemoveFromFavorites(Student student, StudyMaterial material)
    {
        var removed = student.RemoveFromFavorites(material);
        if (removed)
        {
            _storage.Persist();
        }

        return removed;
    }

    public bool RemoveMaterialFromStudent(Student student, string title)
    {
        var material = student.MyMaterials
            .FirstOrDefault(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (material is null)
        {
            return false;
        }

        student.MyMaterials.Remove(material);
        student.FavoriteMaterials.Remove(material);

        var isUsedByOtherStudents = _storage.GetUsers()
            .OfType<Student>()
            .Any(s => !ReferenceEquals(s, student) && s.MyMaterials.Any(m => m.Equals(material)));

        if (!isUsedByOtherStudents)
        {
            _storage.RemoveMaterial(material);
        }
        else
        {
            _storage.Persist();
        }

        return true;
    }

    public bool BlockUser(Moderator moderator, User user) => _storage.BlockUser(moderator, user);

    public bool RemoveUser(string login) => _storage.RemoveUser(login);

    public bool RemoveMaterial(string title)
    {
        var removed = _storage.RemoveMaterial(title);
        if (!removed) return false;

        // Зміна: синхронізуємо видалення матеріалу зі списками студентів та обраним.
        foreach (var student in _storage.GetUsers().OfType<Student>())
        {
            student.MyMaterials.RemoveAll(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            student.FavoriteMaterials.RemoveWhere(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        _storage.Persist();
        return true;
    }

    private void EnsureUserDoesNotExist(string login)
    {
        if (_storage.UserExists(login))
        {
            throw new InvalidOperationException($"Користувач з логіном '{login}' вже існує.");
        }
    }
}
