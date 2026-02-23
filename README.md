# StudyHub

StudyHub - навчальний десктопний застосунок (Avalonia) для керування навчальними матеріалами з ролевим доступом.

## Основні можливості
- `guest`: перегляд списків і пошук матеріалів.
- `student`: додавання власних матеріалів, керування обраним.
- `moderator`: керування користувачами та матеріалами (редагування, видалення, блокування).

## Швидкий старт
1. Зібрати застосунок:
   - `dotnet build StudyHub.sln`
2. Запустити UI:
   - `dotnet run --project StudyHub/StudyHub.csproj`
3. Запустити базові модульні тести:
   - `dotnet run --project StudyHub.Tests/StudyHub.Tests.csproj`

## Демо-акаунти за замовчуванням
- `student / student123`
- `moderator / moderator123` (admin token: `MOD-TOKEN-01`)

Стан застосунку зберігається у файлі `studyhub-state.json` у директорії виконання.
