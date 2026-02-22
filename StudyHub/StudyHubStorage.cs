using System.Text.Json;

namespace StudyHub;

public class StudyHubStorage
{
    private readonly IRepository<User> _userRepository = new InMemoryRepository<User>();
    private readonly IRepository<StudyMaterial> _materialRepository = new InMemoryRepository<StudyMaterial>();
    private readonly Dictionary<SubjectCategory, List<StudyMaterial>> _materialsBySubject = new();
    private readonly HashSet<string> _blockedUsers = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _stateFilePath;

    public StudyHubStorage()
    {
        _stateFilePath = Path.Combine(AppContext.BaseDirectory, "studyhub-state.json");

        if (!TryLoadState())
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
            Users = _userRepository.GetAll().Select(MapUserToRecord).ToList(),
            Materials = _materialRepository.GetAll().Select(MapMaterialToRecord).ToList(),
            BlockedUsers = _blockedUsers.ToList()
        };

        var directory = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
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
            var state = JsonSerializer.Deserialize<PersistedState>(json);
            if (state is null)
            {
                return false;
            }

            foreach (var materialRecord in state.Materials)
            {
                AddMaterialInternal(new StudyMaterial(materialRecord.Title, materialRecord.Subject));
            }

            foreach (var userRecord in state.Users)
            {
                var user = MapRecordToUser(userRecord);
                AddUserInternal(user);

                if (user is Student student)
                {
                    foreach (var myMaterial in userRecord.MyMaterials ?? [])
                    {
                        student.AddMaterial(GetOrCreateMaterial(myMaterial));
                    }

                    foreach (var favorite in userRecord.FavoriteMaterials ?? [])
                    {
                        student.SaveToFavorites(GetOrCreateMaterial(favorite));
                    }
                }
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

        var csharpMaterial = new StudyMaterial("C# Collections Deep Dive", SubjectCategory.Programming);
        var algebraMaterial = new StudyMaterial("Linear Algebra Basics", SubjectCategory.Mathematics);
        var physicsMaterial = new StudyMaterial("Physics Labs Intro", SubjectCategory.Physics);

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

    private StudyMaterial GetOrCreateMaterial(MaterialRecord materialRecord)
    {
        var material = _materialRepository
            .Find(m => m.Title.Equals(materialRecord.Title, StringComparison.OrdinalIgnoreCase) && m.Subject == materialRecord.Subject)
            .FirstOrDefault();

        if (material is not null)
        {
            return material;
        }

        material = new StudyMaterial(materialRecord.Title, materialRecord.Subject);
        AddMaterialInternal(material);
        return material;
    }

    private static UserRecord MapUserToRecord(User user)
    {
        if (user is Moderator moderator)
        {
            return new UserRecord
            {
                Role = "Moderator",
                Login = moderator.Login,
                Password = moderator.RawPassword,
                StudentId = moderator.StudentID,
                AdminToken = moderator.RawAdminToken,
                MyMaterials = moderator.MyMaterials.Select(MapMaterialToRecord).ToList(),
                FavoriteMaterials = moderator.FavoriteMaterials.Select(MapMaterialToRecord).ToList()
            };
        }

        if (user is Student student)
        {
            return new UserRecord
            {
                Role = "Student",
                Login = student.Login,
                Password = student.RawPassword,
                StudentId = student.StudentID,
                MyMaterials = student.MyMaterials.Select(MapMaterialToRecord).ToList(),
                FavoriteMaterials = student.FavoriteMaterials.Select(MapMaterialToRecord).ToList()
            };
        }

        return new UserRecord
        {
            Role = "User",
            Login = user.Login,
            Password = user.RawPassword
        };
    }

    private static User MapRecordToUser(UserRecord userRecord)
    {
        return userRecord.Role switch
        {
            "Moderator" => new Moderator(
                userRecord.Login,
                userRecord.Password,
                userRecord.StudentId ?? 1,
                userRecord.AdminToken ?? "MOD-TOKEN"),
            "Student" => new Student(
                userRecord.Login,
                userRecord.Password,
                userRecord.StudentId ?? 1),
            _ => new User(userRecord.Login, userRecord.Password)
        };
    }

    private static MaterialRecord MapMaterialToRecord(StudyMaterial material) =>
        new()
        {
            Title = material.Title,
            Subject = material.Subject
        };

    private sealed class PersistedState
    {
        public List<UserRecord> Users { get; set; } = [];
        public List<MaterialRecord> Materials { get; set; } = [];
        public List<string> BlockedUsers { get; set; } = [];
    }

    private sealed class UserRecord
    {
        public string Role { get; set; } = "User";
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public string? AdminToken { get; set; }
        public List<MaterialRecord>? MyMaterials { get; set; }
        public List<MaterialRecord>? FavoriteMaterials { get; set; }
    }

    private sealed class MaterialRecord
    {
        public string Title { get; set; } = string.Empty;
        public SubjectCategory Subject { get; set; } = SubjectCategory.Programming;
    }
}
