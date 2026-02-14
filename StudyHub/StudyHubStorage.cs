namespace StudyHubPrototype;

public class StudyHubStorage
{
    // Репозиторій користувачів.
    private readonly IRepository<User> _userRepository = new InMemoryRepository<User>();
    // Репозиторій матеріалів.
    private readonly IRepository<StudyMaterial> _materialRepository = new InMemoryRepository<StudyMaterial>();

    // Швидкий індекс матеріалів за категорією.
    private readonly Dictionary<SubjectCategory, List<StudyMaterial>> _materialsBySubject = new();

    // Додаємо користувача.
    public void AddUser(User user) => _userRepository.Add(user);

    public IReadOnlyList<User> GetUsers() => _userRepository.GetAll();

    // додано пошук користувачів за частиною логіна.
    public IReadOnlyList<User> FindUsers(string loginPart) =>
        _userRepository.Find(u => u.Login.Contains(loginPart, StringComparison.OrdinalIgnoreCase));

    // додано видалення користувача за логіном.
    public bool RemoveUser(string login)
    {
        var user = _userRepository.Find(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (user is null) return false;
        return _userRepository.Remove(user);
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

    // Зміна: додано пошук матеріалів за частиною назви.
    public IReadOnlyList<StudyMaterial> FindMaterials(string titlePart) =>
        _materialRepository.Find(m => m.Title.Contains(titlePart, StringComparison.OrdinalIgnoreCase));

    // Зміна: додано видалення матеріалу + прибирання з індексу категорій.
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
