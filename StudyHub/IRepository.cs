namespace StudyHub;

public interface IRepository<T>
{
    // Додати елемент.
    void Add(T item);
    // Повернути всі елементи.
    IReadOnlyList<T> GetAll();
    // додано універсальний пошук за предикатом.
    IReadOnlyList<T> Find(Func<T, bool> predicate);
    // Видалити елемент.
    bool Remove(T item);
}
