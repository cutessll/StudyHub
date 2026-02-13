using System;
using System.Text;

namespace StudyHubPrototype
{
    
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Прототип системи StudyHub v1.0 ===");
            Console.WriteLine("--------------------------------------");

            // Емуляція роботи студента 
            Student myStudent = new Student("Andrii_NPU", "1234567", 2405);
            Console.WriteLine($"Авторизовано: {myStudent.Login}");
            
            myStudent.SaveToFavorites();
            myStudent.SearchMaterial();      
            myStudent.UploadFile();  
            myStudent.AddMaterial(new StudyMaterial("ООП на C#", SubjectCategory.Programming));
            myStudent.AddMaterial(new StudyMaterial("Вища Математика", SubjectCategory.Mathematics));
            
            myStudent.DisplayInfo();
            foreach (var item in myStudent.MyMaterials)
            {
                Console.WriteLine($"- [{item.Subject}] {item.Title}");
            }


            Console.WriteLine("--------------------------------------");

            // Емуляція роботи модератора
            Moderator myAdmin = new Moderator("Olena_Admin", "123456", 1776, "AAAAA:aaaaaa");
            Console.WriteLine($"Авторизовано: {myAdmin.Login}");
            
            myAdmin.UploadFile();    
            myAdmin.DeleteFile();    

            Console.WriteLine("\nПрограма відпрацювала коректно. Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
    public enum SubjectCategory
    {
        Programming,
        Mathematics,
        Physics,
        History,
        ForeignLanguage
    }
}