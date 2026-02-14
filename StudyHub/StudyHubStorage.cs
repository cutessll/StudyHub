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

    public IReadOnlyList<StudyMaterial> GetMaterialsBySubject(SubjectCategory subject)
    {
        if (_materialsBySubject.TryGetValue(subject, out var list))
        {
            return list.AsReadOnly();
        }

        return Array.Empty<StudyMaterial>();
    }
}
