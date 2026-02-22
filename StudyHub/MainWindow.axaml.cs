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

        RefreshAllLists();
        EnterGuestMode("Гостьовий режим активовано.");
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

        if (user is not Student && user is not Moderator)
        {
            LoginHintText.Text = "Для guest-режиму вхід не потрібен. Увійдіть як студент або модератор.";
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

    private void OnAuthButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_activeUser is null)
        {
            ShowLoginMode();
            return;
        }

        _activeUser = null;
        _activeStudent = null;
        _activeModerator = null;
        EnterGuestMode("Сесію завершено. Активовано гостьовий режим.");
    }

    private void OnGuestModeClick(object? sender, RoutedEventArgs e)
    {
        EnterGuestMode("Активовано гостьовий режим.");
    }

    private void ShowLoginMode()
    {
        LoginView.IsVisible = true;
        AppView.IsVisible = false;

        LoginInput.Text = string.Empty;
        PasswordInput.Text = string.Empty;
        LoginHintText.Text = "Введіть логін студента або модератора.";
    }

    private void EnterGuestMode(string message)
    {
        _currentRole = AppRole.Guest;
        SetAppMode("Guest", message);
    }

    private void SetAppMode(string userDisplay, string message)
    {
        LoginView.IsVisible = false;
        AppView.IsVisible = true;

        SessionUserText.Text = userDisplay;
        RoleHeaderText.Text = _currentRole switch
        {
            AppRole.Guest => "Роль: guest",
            AppRole.Student => "Роль: student",
            AppRole.Moderator => "Роль: moderator",
            _ => "Роль: невідома"
        };
        AuthButton.Content = _activeUser is null ? "Логін" : "Вийти";

        GuestMenuPanel.IsVisible = _currentRole == AppRole.Guest;
        StudentMenuPanel.IsVisible = _currentRole == AppRole.Student;
        ModeratorMenuPanel.IsVisible = _currentRole == AppRole.Moderator;

        MaterialAddPanel.IsVisible = _currentRole is AppRole.Student or AppRole.Moderator;
        MaterialRemovePanel.IsVisible = _currentRole is AppRole.Student or AppRole.Moderator;
        AddMaterialButton.IsEnabled = _currentRole is AppRole.Student or AppRole.Moderator;
        RemoveMaterialButton.IsEnabled = _currentRole is AppRole.Student or AppRole.Moderator;
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
            SetStatus("Гість не може додавати матеріали. Увійдіть як студент або модератор.");
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
            if (_activeModerator is null)
            {
                SetStatus("Активного модератора не знайдено.");
                return;
            }

            var material = _service.AddMaterialToStudent(_activeModerator, title, subject);
            SetStatus($"Модератор додав матеріал '{material.Title}'.");
        }

        MaterialTitleInput.Text = string.Empty;
        RefreshAllLists();
    }

    private void OnRemoveMaterialClick(object? sender, RoutedEventArgs e)
    {
        if (_currentRole == AppRole.Guest)
        {
            SetStatus("Гість не може видаляти матеріали.");
            return;
        }

        if (_currentRole == AppRole.Student)
        {
            if (_activeStudent is null)
            {
                SetStatus("Активного студента не знайдено.");
                return;
            }

            var studentTitle = MaterialRemoveInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(studentTitle))
            {
                SetStatus("Вкажіть назву матеріалу для видалення.");
                return;
            }

            var removedOwn = _service.RemoveMaterialFromStudent(_activeStudent, studentTitle);
            SetStatus(removedOwn
                ? $"Матеріал '{studentTitle}' видалено з ваших матеріалів."
                : "Матеріал не знайдено серед ваших.");

            MaterialRemoveInput.Text = string.Empty;
            RefreshAllLists();
            return;
        }

        if (_activeModerator is null)
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

        SetStatus($"Знайдено матеріалів: {_searchResults.Count}.");
    }

    private void OnResetMaterialsClick(object? sender, RoutedEventArgs e)
    {
        MaterialSearchInput.Text = string.Empty;
        _searchResults.Clear();
        SetStatus("Результати пошуку очищено.");
    }

    private void OnAddSelectedMaterialToFavoritesClick(object? sender, RoutedEventArgs e)
    {
        AddSelectedListItemToFavorites(MaterialsList.SelectedItem);
    }

    private void OnAddSelectedSearchResultToFavoritesClick(object? sender, RoutedEventArgs e)
    {
        AddSelectedListItemToFavorites(SearchResultsList.SelectedItem);
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

    private void AddSelectedListItemToFavorites(object? selectedItem)
    {
        if (_currentRole != AppRole.Student || _activeStudent is null)
        {
            SetStatus("Додавання в обране доступне лише студенту.");
            return;
        }

        if (selectedItem is not string selectedText)
        {
            SetStatus("Оберіть матеріал зі списку.");
            return;
        }

        if (!TryParseMaterialDisplay(selectedText, out var title, out var subject))
        {
            SetStatus("Не вдалося розпізнати вибраний матеріал.");
            return;
        }

        var material = _service.GetMaterialsBySubject(subject)
            .FirstOrDefault(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (material is null)
        {
            SetStatus("Матеріал не знайдено.");
            return;
        }

        var added = _service.AddToFavorites(_activeStudent, material);
        SetStatus(added ? "Матеріал додано в обране." : "Матеріал уже в обраному.");
        RefreshStudentData();
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
        role = AppRole.Student;

        if (UserCreateRoleComboBox.SelectedItem is not ComboBoxItem item)
        {
            return false;
        }

        var value = item.Content?.ToString();
        return value switch
        {
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

    private static bool TryParseMaterialDisplay(string display, out string title, out SubjectCategory subject)
    {
        title = string.Empty;
        subject = SubjectCategory.Programming;

        if (string.IsNullOrWhiteSpace(display))
        {
            return false;
        }

        var open = display.IndexOf('[');
        var close = display.IndexOf(']');
        if (open < 0 || close <= open + 1 || close >= display.Length - 1)
        {
            return false;
        }

        var subjectRaw = display.Substring(open + 1, close - open - 1).Trim();
        title = display[(close + 1)..].Trim();

        return !string.IsNullOrWhiteSpace(title) &&
               Enum.TryParse(subjectRaw, ignoreCase: true, out subject);
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
