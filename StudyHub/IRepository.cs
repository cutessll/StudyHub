namespace StudyHubPrototype;

public interface IRepository<T>
{
    // Додати елемент.
    void Add(T item);
    // Повернути всі елементи.
    IReadOnlyList<T> GetAll();
    // Видалити елемент.
    bool Remove(T item);
}
