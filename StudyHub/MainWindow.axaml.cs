using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace StudyHub;

public partial class MainWindow : Window
{
    private enum AppRole
    {
        Guest,
        Student,
        Moderator
    }

    private readonly IStudyHubService _service;
    private readonly ObservableCollection<string> _users = new();
    private readonly ObservableCollection<string> _materials = new();
    private readonly ObservableCollection<string> _searchResults = new();
    private readonly ObservableCollection<string> _studentMaterials = new();
    private readonly ObservableCollection<string> _favorites = new();

    private readonly Student? _mockStudent;
    private User? _activeUser;
    private Student? _activeStudent;
    private Moderator? _activeModerator;
    private AppRole _currentRole = AppRole.Guest;

    public MainWindow(IStudyHubService service)
    {
        InitializeComponent();
        _service = service;

        UsersList.ItemsSource = _users;
        MaterialsList.ItemsSource = _materials;
        SearchResultsList.ItemsSource = _searchResults;
        StudentMaterialsList.ItemsSource = _studentMaterials;
        FavoritesList.ItemsSource = _favorites;

        _mockStudent = _service.GetMockStudent();

        RefreshAllLists();
        SetLoginMode();
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var login = LoginInput.Text?.Trim() ?? string.Empty;
        var password = PasswordInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            LoginHintText.Text = "Вкажіть логін і пароль.";
            return;
        }

        var user = _service.Authenticate(login, password);
        if (user is null)
        {
            LoginHintText.Text = "Невірні дані для входу або користувача заблоковано.";
            return;
        }

        _activeUser = user;
        _activeStudent = user as Student;
        _activeModerator = user as Moderator;

        _currentRole = user switch
        {
            Moderator => AppRole.Moderator,
            Student => AppRole.Student,
            _ => AppRole.Guest
        };

        SetAppMode(user.Login, $"Успішний вхід: {user.DisplayInfo()}");
    }

    private void OnLogoutClick(object? sender, RoutedEventArgs e)
    {
        _activeUser = null;
        _activeStudent = null;
        _activeModerator = null;
        SetLoginMode();
        SetStatus("Сесію завершено.");
    }

    private void SetLoginMode()
    {
        LoginView.IsVisible = true;
        AppView.IsVisible = false;

        LoginInput.Text = string.Empty;
        PasswordInput.Text = string.Empty;
        LoginHintText.Text = "Введіть логін і пароль.";

        RoleHeaderText.Text = string.Empty;
        SessionUserText.Text = "Не авторизовано";
    }

    private void SetAppMode(string userDisplay, string message)
    {
        LoginView.IsVisible = false;
        AppView.IsVisible = true;

        SessionUserText.Text = userDisplay;
        RoleHeaderText.Text = _currentRole switch
        {
            AppRole.Guest => "Роль: user",
            AppRole.Student => "Роль: student",
            AppRole.Moderator => "Роль: moderator",
            _ => "Роль: невідома"
        };

        GuestMenuPanel.IsVisible = _currentRole == AppRole.Guest;
        StudentMenuPanel.IsVisible = _currentRole == AppRole.Student;
        ModeratorMenuPanel.IsVisible = _currentRole == AppRole.Moderator;

        MaterialAddPanel.IsVisible = _currentRole is AppRole.Student or AppRole.Moderator;
        MaterialRemovePanel.IsVisible = _currentRole == AppRole.Moderator;
        AddMaterialButton.IsEnabled = _currentRole is AppRole.Student or AppRole.Moderator;
        RemoveMaterialButton.IsEnabled = _currentRole == AppRole.Moderator;
        CreateUserButton.IsEnabled = _currentRole == AppRole.Moderator;
        UserCreateRoleComboBox.IsEnabled = _currentRole == AppRole.Moderator;
        UserCreateLoginInput.IsEnabled = _currentRole == AppRole.Moderator;
        UserCreatePasswordInput.IsEnabled = _currentRole == AppRole.Moderator;
        UserCreateIdInput.IsEnabled = _currentRole == AppRole.Moderator;
        UserCreateTokenInput.IsEnabled = _currentRole == AppRole.Moderator;
        UserRemoveInput.IsEnabled = _currentRole == AppRole.Moderator;

        ShowStartSection();
        RefreshAllLists();
        SetStatus(message);
    }

    private void ShowStartSection() =>
        ShowOnlySections(start: true, materials: false, search: false, student: false, favorites: false, users: false);

    private void OnShowMaterialsSectionClick(object? sender, RoutedEventArgs e) =>
        ShowOnlySections(start: false, materials: true, search: false, student: false, favorites: false, users: false);

    private void OnShowSearchSectionClick(object? sender, RoutedEventArgs e) =>
        ShowOnlySections(start: false, materials: false, search: true, student: false, favorites: false, users: false);

    private void OnShowStudentSectionClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Student)
        {
            SetStatus("Розділ доступний лише студенту.");
            return;
        }

        ShowOnlySections(start: false, materials: false, search: false, student: true, favorites: false, users: false);
    }

    private void OnShowFavoritesSectionClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Student)
        {
            SetStatus("Розділ обраного доступний лише студенту.");
            return;
        }

        ShowOnlySections(start: false, materials: false, search: false, student: false, favorites: true, users: false);
    }

    private void OnShowUsersSectionClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Moderator)
        {
            SetStatus("Розділ користувачів доступний лише модератору.");
            return;
        }

        ShowOnlySections(start: false, materials: false, search: false, student: false, favorites: false, users: true);
    }

    private void ShowOnlySections(bool start, bool materials, bool search, bool student, bool favorites, bool users)
    {
        StartSection.IsVisible = start;
        MaterialsSection.IsVisible = materials;
        SearchSection.IsVisible = search;
        StudentSection.IsVisible = student;
        FavoritesSection.IsVisible = favorites;
        UsersSection.IsVisible = users;
    }

    private void OnAddMaterialClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole == AppRole.Guest)
        {
            SetStatus("Користувач role User не може додавати матеріали.");
            return;
        }

        if (string.IsNullOrWhiteSpace(MaterialTitleInput.Text))
        {
            SetStatus("Вкажіть назву матеріалу.");
            return;
        }

        if (!TryGetSelectedSubject(out var subject))
        {
            SetStatus("Оберіть категорію матеріалу.");
            return;
        }

        var title = MaterialTitleInput.Text.Trim();

        if (_currentRole == AppRole.Student)
        {
            if (_activeStudent is null)
            {
                SetStatus("Активного студента не знайдено.");
                return;
            }

            var material = _service.AddMaterialToStudent(_activeStudent, title, subject);
            SetStatus($"Студент додав матеріал '{material.Title}'.");
        }
        else
        {
            if (_activeModerator is null || _mockStudent is null)
            {
                SetStatus("Модератор або студент для модерації не знайдені.");
                return;
            }

            // Модератор діє як користувач із розширеними правами.
            _activeModerator.UploadFile(new StudyMaterial(title, subject));
            var material = _service.AddMaterialToStudent(_mockStudent, title, subject);
            SetStatus($"Модератор додав матеріал '{material.Title}' у профіль студента {_mockStudent.Login}.");
        }

        MaterialTitleInput.Text = string.Empty;
        RefreshAllLists();
    }

    private void OnRemoveMaterialClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Moderator || _activeModerator is null)
        {
            SetStatus("Видалення матеріалів доступне лише модератору.");
            return;
        }

        var title = MaterialRemoveInput.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            SetStatus("Вкажіть назву матеріалу для видалення.");
            return;
        }

        var material = _service.FindMaterials(title)
            .FirstOrDefault(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (material is null)
        {
            SetStatus("Матеріал не знайдено.");
            return;
        }

        var removed = _service.RemoveMaterial(title);
        if (removed)
        {
            _activeModerator.DeleteFile(_activeModerator.MyMaterials, material);
        }

        SetStatus(removed ? $"Матеріал '{title}' видалено." : "Матеріал не знайдено.");
        MaterialRemoveInput.Text = string.Empty;
        RefreshAllLists();
    }

    private void OnSearchMaterialsClick(object? sender, RoutedEventArgs e)
    {
        var query = MaterialSearchInput.Text?.Trim() ?? string.Empty;
        var allMaterials = Enum.GetValues<SubjectCategory>()
            .SelectMany(_service.GetMaterialsBySubject)
            .ToList();

        var found = _activeUser is null
            ? allMaterials.Where(m => m.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList()
            : _activeUser.SearchMaterial(allMaterials, query).ToList();

        _searchResults.Clear();
        foreach (var material in found)
        {
            _searchResults.Add($"[{material.Subject}] {material.Title}");
        }

        if (_currentRole == AppRole.Guest && _activeUser is not null && found.Count > 0)
        {
            _activeUser.DownloadFile(found[0]);
        }

        SetStatus($"Знайдено матеріалів: {_searchResults.Count}.");
    }

    private void OnResetMaterialsClick(object? sender, RoutedEventArgs e)
    {
        MaterialSearchInput.Text = string.Empty;
        _searchResults.Clear();
        SetStatus("Результати пошуку очищено.");
    }

    private void OnAddFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Student || _activeStudent is null)
        {
            SetStatus("Обране доступне лише студенту.");
            return;
        }

        var material = FindStudentMaterialByTitle(FavoriteTitleInput.Text);
        if (material is null)
        {
            SetStatus("Матеріал не знайдено серед ваших матеріалів.");
            return;
        }

        var added = _service.AddToFavorites(_activeStudent, material);
        SetStatus(added ? "Додано в обране." : "Матеріал уже в обраному.");
        RefreshStudentData();
    }

    private void OnRemoveFavoriteClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Student || _activeStudent is null)
        {
            SetStatus("Обране доступне лише студенту.");
            return;
        }

        var material = FindStudentMaterialByTitle(FavoriteTitleInput.Text);
        if (material is null)
        {
            SetStatus("Матеріал для видалення не знайдено.");
            return;
        }

        var removed = _service.RemoveFromFavorites(_activeStudent, material);
        SetStatus(removed ? "Видалено з обраного." : "Цього матеріалу не було в обраному.");
        RefreshStudentData();
    }

    private void OnSearchUsersClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Moderator)
        {
            SetStatus("Пошук користувачів доступний лише модератору.");
            return;
        }

        var query = UserSearchInput.Text?.Trim() ?? string.Empty;
        var found = string.IsNullOrWhiteSpace(query) ? _service.GetUsers() : _service.FindUsers(query);

        _users.Clear();
        foreach (var user in found)
        {
            _users.Add(user.DisplayInfo());
        }

        SetStatus($"Знайдено користувачів: {_users.Count}.");
    }

    private void OnResetUsersClick(object? sender, RoutedEventArgs e)
    {
        UserSearchInput.Text = string.Empty;
        RefreshUsers();
        SetStatus("Список користувачів оновлено.");
    }

    private void OnCreateUserClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Moderator || _activeModerator is null)
        {
            SetStatus("Створення користувачів доступне лише модератору.");
            return;
        }

        if (!TryGetSelectedUserRole(out var role))
        {
            SetStatus("Оберіть роль нового користувача.");
            return;
        }

        var login = UserCreateLoginInput.Text?.Trim() ?? string.Empty;
        var password = UserCreatePasswordInput.Text ?? string.Empty;
        var idRaw = UserCreateIdInput.Text?.Trim() ?? string.Empty;
        var token = UserCreateTokenInput.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(login))
        {
            SetStatus("Вкажіть логін нового користувача.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            SetStatus("Пароль має містити щонайменше 6 символів.");
            return;
        }

        if (_service.UserExists(login))
        {
            SetStatus($"Користувач з логіном '{login}' вже існує.");
            return;
        }

        try
        {
            switch (role)
            {
                case AppRole.Guest:
                    _service.RegisterUser(login, password);
                    SetStatus($"Створено користувача '{login}' з роллю User.");
                    break;
                case AppRole.Student:
                    if (!TryParsePositiveId(idRaw, out var studentId))
                    {
                        SetStatus("Для студента вкажіть додатний числовий ID.");
                        return;
                    }

                    _service.RegisterStudent(login, password, studentId);
                    SetStatus($"Створено студента '{login}' (ID: {studentId}).");
                    break;
                case AppRole.Moderator:
                    if (!TryParsePositiveId(idRaw, out var moderatorId))
                    {
                        SetStatus("Для модератора вкажіть додатний числовий ID.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        SetStatus("Для модератора вкажіть admin token.");
                        return;
                    }

                    _service.RegisterModerator(login, password, moderatorId, token);
                    SetStatus($"Створено модератора '{login}' (ID: {moderatorId}).");
                    break;
                default:
                    SetStatus("Невідома роль.");
                    return;
            }
        }
        catch (InvalidOperationException ex)
        {
            SetStatus(ex.Message);
            return;
        }

        UserCreateLoginInput.Text = string.Empty;
        UserCreatePasswordInput.Text = string.Empty;
        UserCreateIdInput.Text = string.Empty;
        UserCreateTokenInput.Text = string.Empty;

        RefreshAllLists();
    }

    private void OnRemoveUserClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole != AppRole.Moderator || _activeModerator is null)
        {
            SetStatus("Видалення користувачів доступне лише модератору.");
            return;
        }

        var login = UserRemoveInput.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(login))
        {
            SetStatus("Вкажіть логін користувача для видалення.");
            return;
        }

        var targetUser = _service.GetUsers()
            .FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

        if (targetUser is null)
        {
            SetStatus("Користувача не знайдено.");
            return;
        }

        _service.BlockUser(_activeModerator, targetUser);
        var removed = _service.RemoveUser(login);

        SetStatus(removed
            ? $"Користувача '{login}' заблоковано та видалено."
            : "Користувача не вдалося видалити.");

        if (removed && _mockStudent is not null &&
            login.Equals(_mockStudent.Login, StringComparison.OrdinalIgnoreCase))
        {
            _activeStudent = null;
        }

        UserRemoveInput.Text = string.Empty;
        RefreshAllLists();
    }

    private void RefreshAllLists()
    {
        RefreshUsers();
        RefreshMaterials();
        RefreshStudentData();
        UpdateStats();
    }

    private void RefreshUsers()
    {
        _users.Clear();
        foreach (var user in _service.GetUsers())
        {
            _users.Add(user.DisplayInfo());
        }
    }

    private void RefreshMaterials()
    {
        _materials.Clear();
        foreach (var subject in Enum.GetValues<SubjectCategory>())
        {
            foreach (var material in _service.GetMaterialsBySubject(subject))
            {
                _materials.Add($"[{material.Subject}] {material.Title}");
            }
        }
    }

    private void RefreshStudentData()
    {
        _studentMaterials.Clear();
        _favorites.Clear();

        if (_activeStudent is null)
        {
            return;
        }

        foreach (var material in _service.GetStudentMaterials(_activeStudent))
        {
            _studentMaterials.Add($"[{material.Subject}] {material.Title}");
        }

        foreach (var material in _service.GetFavoriteMaterials(_activeStudent))
        {
            _favorites.Add($"[{material.Subject}] {material.Title}");
        }
    }

    private StudyMaterial? FindStudentMaterialByTitle(string? title)
    {
        if (_activeStudent is null || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        return _service.GetStudentMaterials(_activeStudent)
            .FirstOrDefault(m => m.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private bool TryGetSelectedSubject(out SubjectCategory subject)
    {
        subject = SubjectCategory.Programming;

        if (SubjectComboBox.SelectedItem is not ComboBoxItem item)
        {
            return false;
        }

        var value = item.Content?.ToString();
        return Enum.TryParse(value, out subject);
    }

    private bool TryGetSelectedUserRole(out AppRole role)
    {
        role = AppRole.Guest;

        if (UserCreateRoleComboBox.SelectedItem is not ComboBoxItem item)
        {
            return false;
        }

        var value = item.Content?.ToString();
        return value switch
        {
            "User" => SetRole(AppRole.Guest, out role),
            "Student" => SetRole(AppRole.Student, out role),
            "Moderator" => SetRole(AppRole.Moderator, out role),
            _ => false
        };
    }

    private static bool TryParsePositiveId(string idRaw, out int id)
    {
        return int.TryParse(idRaw, out id) && id > 0;
    }

    private static bool SetRole(AppRole selectedRole, out AppRole role)
    {
        role = selectedRole;
        return true;
    }

    private void SetStatus(string message)
    {
        StatusText.Text = message;
        UpdateStats();
    }

    private void UpdateStats()
    {
        StatsText.Text = $"Користувачів: {_service.GetUsersCount()} | Матеріалів: {_service.GetMaterialsCount()}";
    }
}
