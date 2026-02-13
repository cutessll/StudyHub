namespace StudyHubPrototype;

public class Student:User
{
    // Клас Student - успадковує User
    public int StudentID { get; set; }

    public void UploadFile()
    {
        Console.WriteLine($"[Student] Студент (ID: {StudentID}) додає новий файл.");
    }
    public void SaveToFavorites()
    {
        Console.WriteLine("[Student] Матеріал успішно додано до обраного.");
    }
}