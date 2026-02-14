namespace StudyHubPrototype;

public class StudyHubStorage
{
    // Загальний список користувачів, щоб мати єдину точку доступу
    private readonly List<User> _users = new();
    // Матеріали групуємо за предметом, щоб швидко вибирати потрібну категорію
    private readonly Dictionary<SubjectCategory, List<StudyMaterial>> _materialsBySubject = new();

    // Реєструємо користувача в пам'яті застосунку
    public void AddUser(User user) => _users.Add(user);

    public IReadOnlyList<User> GetUsers() => _users.AsReadOnly();

    public void AddMaterial(StudyMaterial material)
    {
        // Якщо для предмета ще немає кошика - створюємо
        if (!_materialsBySubject.TryGetValue(material.Subject, out var list))
        {
            list = new List<StudyMaterial>();
            _materialsBySubject[material.Subject] = list;
        }

        // Додаємо матеріал у відповідну категорію
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
