using StudyHub;

namespace StudyHub.Tests;

public static class Program
{
    public static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("InputValidator accepts valid login", InputValidator_ValidLogin),
            ("InputValidator rejects short login", InputValidator_InvalidLogin),
            ("RegisterStudent creates unique user", RegisterStudent_CreatesUser),
            ("AddToFavorites adds and removes material", Favorites_AddAndRemove),
            ("UpdateUser updates uploader login", UpdateUser_UpdatesUploaderLogin),
            ("RemoveMaterial cleans student references", RemoveMaterial_CleansReferences)
        };

        var failed = 0;
        foreach (var test in tests)
        {
            try
            {
                test.Run();
                Console.WriteLine($"[PASS] {test.Name}");
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine($"[FAIL] {test.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"Executed: {tests.Length}, Passed: {tests.Length - failed}, Failed: {failed}");
        return failed == 0 ? 0 : 1;
    }

    private static void InputValidator_ValidLogin()
    {
        var isValid = StudyHubInputValidator.TryNormalizeLogin("student.user-01", out var normalized, out _);
        AssertTrue(isValid, "Expected login to be valid.");
        AssertEqual("student.user-01", normalized, "Normalized login mismatch.");
    }

    private static void InputValidator_InvalidLogin()
    {
        var isValid = StudyHubInputValidator.TryNormalizeLogin("ab", out _, out var error);
        AssertFalse(isValid, "Expected login to be invalid.");
        AssertTrue(!string.IsNullOrWhiteSpace(error), "Validation error should be returned.");
    }

    private static void RegisterStudent_CreatesUser()
    {
        UseIsolatedService((service, _) =>
        {
            var student = service.RegisterStudent("stud1", "pass123", 101);
            AssertEqual("stud1", student.Login, "Student login mismatch.");
            AssertEqual(1, service.GetUsersCount(), "Unexpected users count.");

            var duplicateThrown = false;
            try
            {
                service.RegisterStudent("stud1", "pass123", 102);
            }
            catch (InvalidOperationException)
            {
                duplicateThrown = true;
            }

            AssertTrue(duplicateThrown, "Expected duplicate registration to throw.");
        });
    }

    private static void Favorites_AddAndRemove()
    {
        UseIsolatedService((service, _) =>
        {
            var student = service.RegisterStudent("stud2", "pass123", 102);
            var material = service.AddMaterialToStudent(student, "Algebra 101", SubjectCategory.Mathematics);

            var added = service.AddToFavorites(student, material);
            AssertTrue(added, "Expected material to be added to favorites.");
            AssertEqual(1, service.GetFavoriteMaterials(student).Count, "Favorites count mismatch after add.");

            var removed = service.RemoveFromFavorites(student, material);
            AssertTrue(removed, "Expected material to be removed from favorites.");
            AssertEqual(0, service.GetFavoriteMaterials(student).Count, "Favorites count mismatch after remove.");
        });
    }

    private static void UpdateUser_UpdatesUploaderLogin()
    {
        UseIsolatedService((service, _) =>
        {
            var moderator = service.RegisterModerator("mod1", "pass123", 201, "TOKEN-123");
            var student = service.RegisterStudent("stud3", "pass123", 103);
            service.AddMaterialToStudent(student, "Physics Intro", SubjectCategory.Physics);

            var updated = service.UpdateUser(moderator, student, "stud3_new", "pass999");
            AssertTrue(updated, "Expected student update to succeed.");

            var material = service.FindMaterials("Physics Intro").Single();
            AssertEqual("stud3_new", material.UploadedByLogin, "Uploader login was not updated.");
        });
    }

    private static void RemoveMaterial_CleansReferences()
    {
        UseIsolatedService((service, _) =>
        {
            var moderator = service.RegisterModerator("mod2", "pass123", 202, "TOKEN-456");
            var student = service.RegisterStudent("stud4", "pass123", 104);
            var material = service.AddMaterialToStudent(student, "History Notes", SubjectCategory.History);
            service.AddToFavorites(student, material);

            var removed = service.RemoveMaterial(moderator, material);
            AssertTrue(removed, "Expected material removal to succeed.");
            AssertEqual(0, service.GetStudentMaterials(student).Count, "Student materials were not cleaned.");
            AssertEqual(0, service.GetFavoriteMaterials(student).Count, "Favorites were not cleaned.");
        });
    }

    private static void UseIsolatedService(Action<StudyHubService, StudyHubStorage> run)
    {
        var stateFile = Path.Combine(Path.GetTempPath(), $"studyhub-tests-{Guid.NewGuid():N}.json");
        try
        {
            var storage = new StudyHubStorage(stateFile, seedDefaultsOnEmptyState: false);
            var service = new StudyHubService(storage);
            run(service, storage);
        }
        finally
        {
            if (File.Exists(stateFile))
            {
                File.Delete(stateFile);
            }
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertFalse(bool condition, string message) => AssertTrue(!condition, message);

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Expected: {expected}, Actual: {actual}");
        }
    }
}
