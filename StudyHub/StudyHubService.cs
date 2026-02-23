namespace StudyHub;

// сервіс інкапсулює роботу зі сховищем для будь-якого UI (консоль/GUI).
public class StudyHubService : IStudyHubService
{
    private readonly StudyHubStorage _storage;
    public string? LastError { get; private set; }

    public StudyHubService(StudyHubStorage storage)
    {
        _storage = storage;
    }

    public User? Authenticate(string login, string password) => _storage.Authenticate(login, password);

    public bool UserExists(string login) => _storage.UserExists(login);

    public User RegisterUser(string login, string password)
    {
        ClearError();
        var normalizedLogin = RequireValidLogin(login);
        var normalizedPassword = RequireValidPassword(password);
        EnsureUserDoesNotExist(normalizedLogin);
        var user = new User(normalizedLogin, normalizedPassword);
        _storage.AddUser(user);
        return user;
    }

    public Student RegisterStudent(string login, string password, int studentId)
    {
        ClearError();
        var normalizedLogin = RequireValidLogin(login);
        var normalizedPassword = RequireValidPassword(password);
        RequireValidStudentId(studentId);
        EnsureUserDoesNotExist(normalizedLogin);
        var student = new Student(normalizedLogin, normalizedPassword, studentId);
        _storage.AddUser(student);
        return student;
    }

    public Moderator RegisterModerator(string login, string password, int studentId, string adminToken)
    {
        ClearError();
        var normalizedLogin = RequireValidLogin(login);
        var normalizedPassword = RequireValidPassword(password);
        RequireValidStudentId(studentId);
        var normalizedToken = RequireValidAdminToken(adminToken);

        EnsureUserDoesNotExist(normalizedLogin);
        var moderator = new Moderator(normalizedLogin, normalizedPassword, studentId, normalizedToken);
        _storage.AddUser(moderator);
        return moderator;
    }

    public StudyMaterial AddMaterialToStudent(
        Student student,
        string title,
        SubjectCategory subject,
        string description = "")
    {
        ClearError();
        var normalizedTitle = RequireValidMaterialTitle(title);
        var normalizedDescription = RequireValidDescription(description);

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
        ClearError();
        var materialExists = _storage.GetMaterials().Any(m => m.Equals(material));
        if (!materialExists)
        {
            return Fail("Матеріал не існує в системі.");
        }

        var added = student.SaveToFavorites(material);
        if (added)
        {
            _storage.Persist();
            return true;
        }

        return Fail("Матеріал уже в обраному.");
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
        ClearError();
        var removed = student.RemoveFromFavorites(material);
        if (removed)
        {
            _storage.Persist();
            return true;
        }

        return Fail("Цього матеріалу немає в обраному.");
    }

    public bool RemoveMaterialFromStudent(Student student, string title)
    {
        ClearError();
        if (!StudyHubInputValidator.TryNormalizeMaterialTitle(title, out var normalizedTitle, out _))
        {
            return Fail("Некоректна назва матеріалу.");
        }

        var material = FindStudentOwnedMaterial(student, normalizedTitle);

        if (material is null)
        {
            return Fail("Матеріал не знайдено або не належить поточному студенту.");
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
        ClearError();
        if (!StudyHubInputValidator.TryNormalizeMaterialTitle(title, out var normalizedTitle, out _))
        {
            return Fail("Некоректна назва матеріалу.");
        }

        if (!StudyHubInputValidator.TryNormalizeDescription(newDescription, out var normalizedDescription, out _))
        {
            return Fail("Опис матеріалу некоректний.");
        }

        var material = FindStudentOwnedMaterial(student, normalizedTitle);

        if (material is null)
        {
            return Fail("Матеріал не знайдено або не належить поточному студенту.");
        }

        var updated = material.TryUpdateDescription(normalizedDescription);
        if (updated)
        {
            _storage.Persist();
            return true;
        }

        return Fail("Не вдалося оновити опис матеріалу.");
    }

    public bool BlockUser(Moderator moderator, User user)
    {
        ClearError();
        var blocked = _storage.BlockUser(moderator, user);
        return blocked ? true : Fail("Не вдалося заблокувати користувача.");
    }

    public bool RemoveUser(Moderator moderator, User user) => RemoveUser(moderator, user.Login);

    public bool RemoveUser(Moderator moderator, string login)
    {
        ClearError();
        if (!CanModerate(moderator))
        {
            return Fail("Операція доступна лише модератору.");
        }

        if (!StudyHubInputValidator.TryNormalizeLogin(login, out var normalizedLogin, out _))
        {
            return Fail("Некоректний логін користувача.");
        }

        if (normalizedLogin.Equals(moderator.Login, StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Модератор не може видалити самого себе.");
        }

        var removed = _storage.RemoveUser(normalizedLogin);
        return removed ? true : Fail("Користувача не знайдено або видалення заборонено.");
    }

    public bool RemoveMaterial(Moderator moderator, StudyMaterial material)
    {
        ClearError();
        if (!CanModerate(moderator))
        {
            return Fail("Операція доступна лише модератору.");
        }

        var removed = _storage.RemoveMaterial(material);
        if (!removed) return Fail("Матеріал не знайдено.");

        RemoveMaterialReferences(m => m.Equals(material));

        _storage.Persist();
        return true;
    }

    public bool RemoveMaterial(Moderator moderator, string title)
    {
        ClearError();
        if (!CanModerate(moderator))
        {
            return Fail("Операція доступна лише модератору.");
        }

        if (!StudyHubInputValidator.TryNormalizeMaterialTitle(title, out var normalizedTitle, out _))
        {
            return Fail("Некоректна назва матеріалу.");
        }

        var removed = _storage.RemoveMaterial(normalizedTitle);
        if (!removed) return Fail("Матеріал не знайдено.");

        RemoveMaterialReferences(m => m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase));

        _storage.Persist();
        return true;
    }

    public bool UpdateUser(Moderator moderator, User user, string newLogin, string newPassword)
    {
        ClearError();
        if (!CanModerate(moderator))
        {
            return Fail("Операція доступна лише модератору.");
        }

        if (ReferenceEquals(user, moderator))
        {
            return Fail("Модератор не може редагувати власний акаунт.");
        }

        if (!StudyHubInputValidator.TryNormalizeLogin(newLogin, out var normalizedLogin, out _))
        {
            return Fail("Некоректний новий логін.");
        }

        if (!StudyHubInputValidator.TryValidatePassword(newPassword, out var normalizedPassword, out _))
        {
            return Fail("Некоректний новий пароль.");
        }

        var loginChanged = !user.Login.Equals(normalizedLogin, StringComparison.OrdinalIgnoreCase);
        if (loginChanged && _storage.UserExists(normalizedLogin))
        {
            return Fail("Користувач із таким логіном уже існує.");
        }

        var oldLogin = user.Login;
        user.Login = normalizedLogin;
        user.Password = normalizedPassword;

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
        ClearError();
        if (!CanModerate(moderator))
        {
            return Fail("Операція доступна лише модератору.");
        }

        if (!StudyHubInputValidator.TryNormalizeMaterialTitle(newTitle, out var normalizedTitle, out _))
        {
            return Fail("Некоректна нова назва матеріалу.");
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
            return Fail("Матеріал із такою назвою та категорією вже існує.");
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

    private static string RequireValidLogin(string login)
    {
        if (!StudyHubInputValidator.TryNormalizeLogin(login, out var normalized, out var error))
        {
            throw new InvalidOperationException(error);
        }

        return normalized;
    }

    private static string RequireValidPassword(string password)
    {
        if (!StudyHubInputValidator.TryValidatePassword(password, out var normalized, out var error))
        {
            throw new InvalidOperationException(error);
        }

        return normalized;
    }

    private static void RequireValidStudentId(int studentId)
    {
        if (!StudyHubInputValidator.TryValidateStudentId(studentId, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    private static string RequireValidMaterialTitle(string title)
    {
        if (!StudyHubInputValidator.TryNormalizeMaterialTitle(title, out var normalized, out var error))
        {
            throw new InvalidOperationException(error);
        }

        return normalized;
    }

    private static string RequireValidDescription(string description)
    {
        if (!StudyHubInputValidator.TryNormalizeDescription(description, out var normalized, out var error))
        {
            throw new InvalidOperationException(error);
        }

        return normalized;
    }

    private static string RequireValidAdminToken(string token)
    {
        if (!StudyHubInputValidator.TryValidateAdminToken(token, out var normalized, out var error))
        {
            throw new InvalidOperationException(error);
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

    private bool Fail(string message)
    {
        LastError = message;
        return false;
    }

    private StudyMaterial? FindStudentOwnedMaterial(Student student, string normalizedTitle) =>
        student.MyMaterials.FirstOrDefault(m =>
            m.Title.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
            m.UploadedByLogin.Equals(student.Login, StringComparison.OrdinalIgnoreCase));

    private void RemoveMaterialReferences(Predicate<StudyMaterial> predicate)
    {
        foreach (var user in _storage.GetUsers())
        {
            user.DownloadedMaterials.RemoveAll(predicate);
        }

        foreach (var student in _storage.GetUsers().OfType<Student>())
        {
            student.MyMaterials.RemoveAll(predicate);
            student.FavoriteMaterials.RemoveWhere(m => predicate(m));
        }
    }

    private void ClearError() => LastError = null;
}
