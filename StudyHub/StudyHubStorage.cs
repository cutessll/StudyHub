namespace StudyHub;

public class StudyHubStorage
{
    private readonly IRepository<User> _userRepository = new InMemoryRepository<User>();
    private readonly IRepository<StudyMaterial> _materialRepository = new InMemoryRepository<StudyMaterial>();
    private readonly Dictionary<SubjectCategory, List<StudyMaterial>> _materialsBySubject = new();
    private readonly HashSet<string> _blockedUsers = new(StringComparer.OrdinalIgnoreCase);

    private readonly User _mockGuest;
    private readonly Student _mockStudent;
    private readonly Moderator _mockModerator;

    public StudyHubStorage()
    {
        _mockGuest = new User("guest", "guest123");
        _mockStudent = new Student("student", "student123", 1001);
        _mockModerator = new Moderator("moderator", "moderator123", 9001, "MOD-TOKEN-01");

        AddUser(_mockGuest);
        AddUser(_mockStudent);
        AddUser(_mockModerator);

        var csharpMaterial = new StudyMaterial("C# Collections Deep Dive", SubjectCategory.Programming);
        var algebraMaterial = new StudyMaterial("Linear Algebra Basics", SubjectCategory.Mathematics);
        var physicsMaterial = new StudyMaterial("Physics Labs Intro", SubjectCategory.Physics);

        AddMaterial(csharpMaterial);
        AddMaterial(algebraMaterial);
        AddMaterial(physicsMaterial);

        _mockStudent.AddMaterial(csharpMaterial);
        _mockStudent.AddMaterial(algebraMaterial);
        _mockStudent.AddMaterial(physicsMaterial);
        _mockStudent.SaveToFavorites(csharpMaterial);
    }

    public User GetMockGuest() => _mockGuest;

    public Student GetMockStudent() => _mockStudent;

    public Moderator GetMockModerator() => _mockModerator;

    public void AddUser(User user) => _userRepository.Add(user);

    public IReadOnlyList<User> GetUsers() => _userRepository.GetAll();

    public bool UserExists(string login) =>
        _userRepository
            .Find(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase))
            .Count > 0;

    public User? Authenticate(string login, string password)
    {
        var user = _userRepository
            .Find(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (user is null || _blockedUsers.Contains(user.Login))
        {
            return null;
        }

        return user.VerifyPassword(password) ? user : null;
    }

    public IReadOnlyList<User> FindUsers(string loginPart) =>
        _userRepository.Find(u => u.Login.Contains(loginPart, StringComparison.OrdinalIgnoreCase));

    public bool RemoveUser(string login)
    {
        if (_mockModerator.Login.Equals(login, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var user = _userRepository.Find(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (user is null) return false;
        return _userRepository.Remove(user);
    }

    public bool BlockUser(Moderator moderator, User user)
    {
        return moderator.BlockUser(user, _blockedUsers);
    }

    public IReadOnlyList<StudyMaterial> GetMaterials() => _materialRepository.GetAll();

    public void AddMaterial(StudyMaterial material)
    {
        _materialRepository.Add(material);

        // Створюємо список для категорії за потреби.
        if (!_materialsBySubject.TryGetValue(material.Subject, out var list))
        {
            list = new List<StudyMaterial>();
            _materialsBySubject[material.Subject] = list;
        }

        // Додаємо матеріал у категорію.
        list.Add(material);
    }

    public IReadOnlyList<StudyMaterial> FindMaterials(string titlePart) =>
        _materialRepository.Find(m => m.Title.Contains(titlePart, StringComparison.OrdinalIgnoreCase));

    public bool RemoveMaterial(string title)
    {
        var material = _materialRepository
            .Find(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (material is null) return false;

        var removed = _materialRepository.Remove(material);
        if (!removed) return false;

        if (_materialsBySubject.TryGetValue(material.Subject, out var list))
        {
            list.Remove(material);
            if (list.Count == 0)
            {
                _materialsBySubject.Remove(material.Subject);
            }
        }

        return true;
    }

    public IReadOnlyList<StudyMaterial> GetMaterialsBySubject(SubjectCategory subject)
    {
        if (_materialsBySubject.TryGetValue(subject, out var list))
        {
            return list.AsReadOnly();
        }

        return Array.Empty<StudyMaterial>();
    }
}
