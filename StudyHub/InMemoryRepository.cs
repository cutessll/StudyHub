namespace StudyHubPrototype;

// Проста реалізація репозиторію в оперативній пам'яті.
public class InMemoryRepository<T> : IRepository<T>
{
    private readonly List<T> _items = new();

    // Додаємо елемент у внутрішній список.
    public void Add(T item) => _items.Add(item);

    // Віддаємо тільки для читання.
    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    // Видаляємо перший збіг.
    public bool Remove(T item) => _items.Remove(item);
}
