using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyHub;

public static class StudyHubJsonSerializer
{
    private static readonly JsonSerializerOptions BaseOptions = CreateOptions();

    public static string Serialize<T>(T value, bool writeIndented = false)
    {
        var options = new JsonSerializerOptions(BaseOptions)
        {
            WriteIndented = writeIndented
        };

        return JsonSerializer.Serialize(value, options);
    }

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, BaseOptions);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UserJsonConverter());
        return options;
    }
}

internal sealed class UserJsonConverter : JsonConverter<User>
{
    public override User Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var role = ReadString(root, "role") ?? "User";
        var login = ReadString(root, "login") ?? throw new JsonException("User login is required.");
        var password = ReadString(root, "password") ?? throw new JsonException("User password is required.");

        var user = role switch
        {
            "Moderator" => new Moderator(
                login,
                password,
                ReadInt(root, "studentId") ?? 1,
                ReadString(root, "adminToken") ?? "MOD-TOKEN"),
            "Student" => new Student(
                login,
                password,
                ReadInt(root, "studentId") ?? 1),
            _ => new User(login, password)
        };

        foreach (var material in ReadMaterials(root, "downloadedMaterials", options))
        {
            user.DownloadedMaterials.Add(material);
        }

        if (user is Student student)
        {
            foreach (var material in ReadMaterials(root, "myMaterials", options))
            {
                student.AddMaterial(material);
            }

            foreach (var material in ReadMaterials(root, "favoriteMaterials", options))
            {
                student.SaveToFavorites(material);
            }
        }

        return user;
    }

    public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("role", value switch
        {
            Moderator => "Moderator",
            Student => "Student",
            _ => "User"
        });
        writer.WriteString("login", value.Login);
        writer.WriteString("password", value.RawPassword);
        WriteMaterials(writer, "downloadedMaterials", value.DownloadedMaterials, options);

        if (value is Student student)
        {
            writer.WriteNumber("studentId", student.StudentID);
            WriteMaterials(writer, "myMaterials", student.MyMaterials, options);
            WriteMaterials(writer, "favoriteMaterials", student.FavoriteMaterials, options);

            if (value is Moderator moderator)
            {
                writer.WriteString("adminToken", moderator.RawAdminToken);
            }
        }

        writer.WriteEndObject();
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static int? ReadInt(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt32(out var value))
        {
            return value;
        }

        return null;
    }

    private static List<StudyMaterial> ReadMaterials(JsonElement root, string propertyName, JsonSerializerOptions options)
    {
        var materials = new List<StudyMaterial>();
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return materials;
        }

        foreach (var item in property.EnumerateArray())
        {
            var material = item.Deserialize<StudyMaterial>(options);
            if (material is not null)
            {
                materials.Add(material);
            }
        }

        return materials;
    }

    private static void WriteMaterials(
        Utf8JsonWriter writer,
        string propertyName,
        IEnumerable<StudyMaterial> materials,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var material in materials)
        {
            JsonSerializer.Serialize(writer, material, options);
        }

        writer.WriteEndArray();
    }
}
