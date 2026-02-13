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
    
    // РЕАЛІЗАЦІЯ ЗВ'ЯЗКУ (Агрегація)
    public List<StudyMaterial> MyMaterials { get; private set; }
    
    // Конструктор Student передає login та password у клас User через base
    public Student(string login, string password, int studentId) : base(login, password)
    {
        StudentID = studentId;
        MyMaterials = new List<StudyMaterial>(); // Ініціалізація зв'язку
    }
    
    
    
    public void AddMaterial(StudyMaterial material)
    {
        MyMaterials.Add(material);
        Console.WriteLine($"[Система] До профілю {Login} додано матеріал: {material.Title}");
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