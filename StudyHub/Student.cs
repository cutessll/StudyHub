namespace StudyHubPrototype;

public class Student:User
{
    // Клас Student - успадковує User

    protected int _studentID;

    public int StudentID
    {
        get { return _studentID; }
        set { if (value > 0) _studentID = value; }
    }

    public void UploadFile()
    {
        Console.WriteLine($"[Student] Студент (ID: {StudentID}) додає новий файл.");
    }
    public void SaveToFavorites()
    {
        Console.WriteLine("[Student] Матеріал успішно додано до обраного.");
    }
}