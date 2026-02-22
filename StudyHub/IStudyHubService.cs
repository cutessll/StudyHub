namespace StudyHub;

// окремий контракт сервісу між UI та шаром доступу до даних.
public interface IStudyHubService
{
    User? Authenticate(string login, string password);
    bool UserExists(string login);
    User RegisterUser(string login, string password);
    Student RegisterStudent(string login, string password, int studentId);
    Moderator RegisterModerator(string login, string password, int studentId, string adminToken);
    StudyMaterial AddMaterialToStudent(Student student, string title, SubjectCategory subject);
    bool AddToFavorites(Student student, StudyMaterial material);
    IReadOnlyList<User> GetUsers();
    IReadOnlyList<User> FindUsers(string loginPart);
    IReadOnlyList<StudyMaterial> FindMaterials(string titlePart);
    IReadOnlyList<StudyMaterial> GetMaterialsBySubject(SubjectCategory subject);
    // методи для демонстрації агрегованих даних у UI.
    int GetUsersCount();
    int GetMaterialsCount();
    IReadOnlyList<StudyMaterial> GetStudentMaterials(Student student);
    IReadOnlyList<StudyMaterial> GetFavoriteMaterials(Student student);
    bool RemoveFromFavorites(Student student, StudyMaterial material);
    bool RemoveMaterialFromStudent(Student student, string title);
    bool BlockUser(Moderator moderator, User user);
    bool RemoveUser(string login);
    bool RemoveMaterial(string title);
}
