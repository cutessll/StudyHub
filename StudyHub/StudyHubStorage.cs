namespace StudyHub;

public class StudyHubStorage
{
    private const string DefaultStateFileName = "studyhub-state.json";
    private readonly IRepository<User> _userRepository = new InMemoryRepository<User>();
    private readonly IRepository<StudyMaterial> _materialRepository = new InMemoryRepository<StudyMaterial>();
    private readonly Dictionary<SubjectCategory, List<StudyMaterial>> _materialsBySubject = new();
    private readonly HashSet<string> _blockedUsers = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _stateFilePath;

    public StudyHubStorage(string? stateFilePath = null, bool seedDefaultsOnEmptyState = true)
    {
        _stateFilePath = string.IsNullOrWhiteSpace(stateFilePath)
            ? Path.Combine(AppContext.BaseDirectory, DefaultStateFileName)
            : stateFilePath;

        if (!TryLoadState() && seedDefaultsOnEmptyState)
        {
            SeedDefaultData();
            Persist();
        }
    }

    public void AddUser(User user)
    {
        _userRepository.Add(user);
        Persist();
    }

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
        if (login.Equals("moderator", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var user = _userRepository.Find(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (user is null) return false;

        var removed = _userRepository.Remove(user);
        if (removed)
        {
            Persist();
        }

        return removed;
    }

    public bool BlockUser(Moderator moderator, User user)
    {
        var blocked = moderator.BlockUser(user, _blockedUsers);
        if (blocked)
        {
            Persist();
        }

        return blocked;
    }

    public IReadOnlyList<StudyMaterial> GetMaterials() => _materialRepository.GetAll();

    public void AddMaterial(StudyMaterial material)
    {
        if (ContainsMaterial(material))
        {
            return;
        }

        _materialRepository.Add(material);
        if (!_materialsBySubject.TryGetValue(material.Subject, out var list))
        {
            list = new List<StudyMaterial>();
            _materialsBySubject[material.Subject] = list;
        }

        list.Add(material);
        Persist();
    }

    public IReadOnlyList<StudyMaterial> FindMaterials(string titlePart) =>
        _materialRepository.Find(m => m.Title.Contains(titlePart, StringComparison.OrdinalIgnoreCase));

    public bool RemoveMaterial(string title)
    {
        var material = _materialRepository
            .Find(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        return material is not null && RemoveMaterial(material);
    }

    public bool RemoveMaterial(StudyMaterial material)
    {
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

        Persist();
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

    public void Persist()
    {
        var state = new PersistedState
        {
            Users = _userRepository.GetAll().ToList(),
            Materials = _materialRepository.GetAll().ToList(),
            BlockedUsers = _blockedUsers.ToList()
        };

        var directory = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = StudyHubJsonSerializer.Serialize(state, writeIndented: true);
        File.WriteAllText(_stateFilePath, json);
    }

    private bool TryLoadState()
    {
        if (!File.Exists(_stateFilePath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(_stateFilePath);
            var state = StudyHubJsonSerializer.Deserialize<PersistedState>(json);
            if (state is null)
            {
                return false;
            }

            foreach (var material in state.Materials)
            {
                AddMaterialInternal(new StudyMaterial(material.Title, material.Subject));
            }

            foreach (var user in state.Users)
            {
                RebindDownloadedMaterials(user);
                RebindStudentMaterials(user);
                AddUserInternal(user);
            }

            foreach (var blocked in state.BlockedUsers ?? [])
            {
                _blockedUsers.Add(blocked);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void SeedDefaultData()
    {
        var student = new Student("student", "student123", 1001);
        var moderator = new Moderator("moderator", "moderator123", 9001, "MOD-TOKEN-01");
        AddUserInternal(student);
        AddUserInternal(moderator);

        var csharpMaterial = new StudyMaterial("C# Collections Deep Dive", SubjectCategory.Programming, student.Login);
        var algebraMaterial = new StudyMaterial("Linear Algebra Basics", SubjectCategory.Mathematics, student.Login);
        var physicsMaterial = new StudyMaterial("Physics Labs Intro", SubjectCategory.Physics, student.Login);

        AddMaterialInternal(csharpMaterial);
        AddMaterialInternal(algebraMaterial);
        AddMaterialInternal(physicsMaterial);

        student.AddMaterial(csharpMaterial);
        student.AddMaterial(algebraMaterial);
        student.AddMaterial(physicsMaterial);
        student.SaveToFavorites(csharpMaterial);
    }

    private void AddUserInternal(User user)
    {
        if (UserExists(user.Login))
        {
            return;
        }

        _userRepository.Add(user);
    }

    private void AddMaterialInternal(StudyMaterial material)
    {
        if (ContainsMaterial(material))
        {
            return;
        }

        _materialRepository.Add(material);
        if (!_materialsBySubject.TryGetValue(material.Subject, out var list))
        {
            list = new List<StudyMaterial>();
            _materialsBySubject[material.Subject] = list;
        }

        list.Add(material);
    }

    private bool ContainsMaterial(StudyMaterial material) =>
        _materialRepository
            .Find(m => m.Title.Equals(material.Title, StringComparison.OrdinalIgnoreCase) && m.Subject == material.Subject)
            .Count > 0;

    private void RebindDownloadedMaterials(User user)
    {
        var downloadedMaterials = user.DownloadedMaterials.ToList();
        user.DownloadedMaterials.Clear();

        foreach (var material in downloadedMaterials)
        {
            user.DownloadedMaterials.Add(GetOrCreateMaterial(material.Title, material.Subject));
        }
    }

    private void RebindStudentMaterialsInternal(Student student)
    {
        var myMaterials = student.MyMaterials.ToList();
        student.MyMaterials.Clear();
        foreach (var material in myMaterials)
        {
            student.MyMaterials.Add(GetOrCreateMaterial(material.Title, material.Subject));
        }

        var favoriteMaterials = student.FavoriteMaterials.ToList();
        student.FavoriteMaterials.Clear();
        foreach (var material in favoriteMaterials)
        {
            student.FavoriteMaterials.Add(GetOrCreateMaterial(material.Title, material.Subject));
        }
    }

    private void RebindStudentMaterials(User user)
    {
        if (user is Student student)
        {
            RebindStudentMaterialsInternal(student);
        }
    }

    private StudyMaterial GetOrCreateMaterial(string title, SubjectCategory subject)
    {
        var material = _materialRepository
            .Find(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && m.Subject == subject)
            .FirstOrDefault();

        if (material is not null)
        {
            return material;
        }

        material = new StudyMaterial(title, subject);
        AddMaterialInternal(material);
        return material;
    }

    private sealed class PersistedState
    {
        public List<User> Users { get; set; } = [];
        public List<StudyMaterial> Materials { get; set; } = [];
        public List<string> BlockedUsers { get; set; } = [];
    }
}
