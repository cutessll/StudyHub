namespace StudyHub;

public class Student : User
{
    protected int _studentId;

    public int StudentID
    {
        get { return _studentId; }
        set
        {
            if (value > 0)
            {
                _studentId = value;
            }
        }
    }

    public List<StudyMaterial> MyMaterials { get; private set; }
    public HashSet<StudyMaterial> FavoriteMaterials { get; private set; }

    public Student(string login, string password, int studentId) : base(login, password)
    {
        StudentID = studentId;
        MyMaterials = new List<StudyMaterial>();
        FavoriteMaterials = new HashSet<StudyMaterial>();
    }

    public bool AddMaterial(StudyMaterial material)
    {
        if (MyMaterials.Any(m => m.Equals(material)))
        {
            return false;
        }

        MyMaterials.Add(material);
        return true;
    }

    public bool UploadFile(StudyMaterial material)
    {
        return AddMaterial(material);
    }

    public bool SaveToFavorites(StudyMaterial material)
    {
        return FavoriteMaterials.Add(material);
    }

    public bool RemoveFromFavorites(StudyMaterial material)
    {
        return FavoriteMaterials.Remove(material);
    }

    public override string DisplayInfo()
    {
        return $"Студент: {Login}, ID: {StudentID}";
    }
}
