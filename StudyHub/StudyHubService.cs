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
        var normalizedLogin = NormalizeLogin(login);
        ValidatePassword(password);
        EnsureUserDoesNotExist(normalizedLogin);
        var user = new User(normalizedLogin, password);
        _storage.AddUser(user);
        return user;
    }

    public Student RegisterStudent(string login, string password, int studentId)
    {
        var normalizedLogin = NormalizeLogin(login);
        ValidatePassword(password);
        ValidateStudentId(studentId);
        EnsureUserDoesNotExist(normalizedLogin);
        var student = new Student(normalizedLogin, password, studentId);
        _storage.AddUser(student);
        return student;
    }

    public Moderator RegisterModerator(string login, string password, int studentId, string adminToken)
    {
        var normalizedLogin = NormalizeLogin(login);
        ValidatePassword(password);
        ValidateStudentId(studentId);
        if (string.IsNullOrWhiteSpace(adminToken))
        {
            throw new InvalidOperationException("Admin token модератора не може бути порожнім.");
        }

        EnsureUserDoesNotExist(normalizedLogin);
        var moderator = new Moderator(normalizedLogin, password, studentId, adminToken.Trim());
        _storage.AddUser(moderator);
        return moderator;
    }

    public StudyMaterial AddMaterialToStudent(
        Student student,
        string title,
        SubjectCategory subject,
        string description = "")
    {
        var normalizedTitle = NormalizeMaterialTitle(title);
        var normalizedDescription = description?.Trim() ?? string.Empty;

        var material = new StudyMaterial(normalizedTitle, subject, student.Login, normalizedDescription);
        var added = student.UploadFile(material);
        if (added)
        {
            _storage.AddMaterial(material);
            return material;
        }

        return student.MyMaterials
            .First(m => m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase) && m.Subject == subject);
    }

    public bool AddToFavorites(Student student, StudyMaterial material)
    {
        var materialExists = _storage.GetMaterials().Any(m => m.Equals(material));
        if (!materialExists)
        {
            return false;
        }

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
        var normalizedTitle = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return false;
        }

        var material = student.MyMaterials
            .FirstOrDefault(m =>
                m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
                m.UploadedByLogin.Equals(student.Login, StringComparison.OrdinalIgnoreCase));

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

    public bool UpdateStudentMaterialDescription(Student student, string title, string newDescription)
    {
        var normalizedTitle = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return false;
        }

        var material = student.MyMaterials
            .FirstOrDefault(m =>
                m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
                m.UploadedByLogin.Equals(student.Login, StringComparison.OrdinalIgnoreCase));

        if (material is null)
        {
            return false;
        }

        var updated = material.TryUpdateDescription(newDescription ?? string.Empty);
        if (updated)
        {
            _storage.Persist();
        }

        return updated;
    }

    public bool BlockUser(Moderator moderator, User user) => _storage.BlockUser(moderator, user);

    public bool RemoveUser(Moderator moderator, User user) => RemoveUser(moderator, user.Login);

    public bool RemoveUser(Moderator moderator, string login)
    {
        if (!CanModerate(moderator))
        {
            return false;
        }

        var normalizedLogin = login?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedLogin))
        {
            return false;
        }

        if (normalizedLogin.Equals(moderator.Login, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return _storage.RemoveUser(normalizedLogin);
    }

    public bool RemoveMaterial(Moderator moderator, StudyMaterial material)
    {
        if (!CanModerate(moderator))
        {
            return false;
        }

        var removed = _storage.RemoveMaterial(material);
        if (!removed) return false;

        foreach (var user in _storage.GetUsers())
        {
            user.DownloadedMaterials.RemoveAll(m => m.Equals(material));
        }

        foreach (var student in _storage.GetUsers().OfType<Student>())
        {
            student.MyMaterials.RemoveAll(m => m.Equals(material));
            student.FavoriteMaterials.RemoveWhere(m => m.Equals(material));
        }

        _storage.Persist();
        return true;
    }

    public bool RemoveMaterial(Moderator moderator, string title)
    {
        if (!CanModerate(moderator))
        {
            return false;
        }

        var normalizedTitle = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return false;
        }

        var removed = _storage.RemoveMaterial(normalizedTitle);
        if (!removed) return false;

        foreach (var user in _storage.GetUsers())
        {
            user.DownloadedMaterials.RemoveAll(m => m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var student in _storage.GetUsers().OfType<Student>())
        {
            student.MyMaterials.RemoveAll(m => m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase));
            student.FavoriteMaterials.RemoveWhere(m => m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase));
        }

        _storage.Persist();
        return true;
    }

    public bool UpdateUser(Moderator moderator, User user, string newLogin, string newPassword)
    {
        if (!CanModerate(moderator))
        {
            return false;
        }

        if (ReferenceEquals(user, moderator))
        {
            return false;
        }

        var normalizedLogin = newLogin?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedLogin))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return false;
        }

        var loginChanged = !user.Login.Equals(normalizedLogin, StringComparison.OrdinalIgnoreCase);
        if (loginChanged && _storage.UserExists(normalizedLogin))
        {
            return false;
        }

        var oldLogin = user.Login;
        user.Login = normalizedLogin;
        user.Password = newPassword;

        foreach (var material in _storage.GetMaterials())
        {
            if (material.UploadedByLogin.Equals(oldLogin, StringComparison.OrdinalIgnoreCase))
            {
                material.TryUpdateUploaderLogin(normalizedLogin);
            }
        }

        _storage.Persist();
        return true;
    }

    public bool UpdateMaterial(Moderator moderator, StudyMaterial material, string newTitle, SubjectCategory newSubject)
    {
        if (!CanModerate(moderator))
        {
            return false;
        }

        var normalizedTitle = newTitle?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return false;
        }

        var noChanges = material.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
                        material.Subject == newSubject;
        if (noChanges)
        {
            return true;
        }

        var duplicate = _storage.GetMaterials()
            .Any(m => !ReferenceEquals(m, material) &&
                      m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
                      m.Subject == newSubject);
        if (duplicate)
        {
            return false;
        }

        var replacement = new StudyMaterial(normalizedTitle, newSubject, material.UploadedByLogin, material.Description);

        foreach (var student in _storage.GetUsers().OfType<Student>())
        {
            for (var i = 0; i < student.MyMaterials.Count; i++)
            {
                if (student.MyMaterials[i].Equals(material))
                {
                    student.MyMaterials[i] = replacement;
                }
            }

            if (student.FavoriteMaterials.RemoveWhere(m => m.Equals(material)) > 0)
            {
                student.FavoriteMaterials.Add(replacement);
            }
        }

        foreach (var user in _storage.GetUsers())
        {
            for (var i = 0; i < user.DownloadedMaterials.Count; i++)
            {
                if (user.DownloadedMaterials[i].Equals(material))
                {
                    user.DownloadedMaterials[i] = replacement;
                }
            }
        }

        _storage.RemoveMaterial(material);
        _storage.AddMaterial(replacement);
        _storage.Persist();
        return true;
    }

    private static bool CanModerate(Moderator? moderator) => moderator is not null;

    private static string NormalizeLogin(string login)
    {
        var normalized = login?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Логін не може бути порожнім.");
        }

        return normalized;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new InvalidOperationException("Пароль має містити щонайменше 6 символів.");
        }
    }

    private static void ValidateStudentId(int studentId)
    {
        if (studentId <= 0)
        {
            throw new InvalidOperationException("ID має бути додатним числом.");
        }
    }

    private static string NormalizeMaterialTitle(string title)
    {
        var normalized = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Назва матеріалу не може бути порожньою.");
        }

        return normalized;
    }

    private void EnsureUserDoesNotExist(string login)
    {
        if (_storage.UserExists(login))
        {
            throw new InvalidOperationException($"Користувач з логіном '{login}' вже існує.");
        }
    }
}
