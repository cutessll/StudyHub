using System;
using System.Text;

namespace StudyHubPrototype
{
    
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.OutputEncoding = Encoding.UTF8;
            // Централізоване зберігання користувачів і матеріалів
            var storage = new StudyHubStorage();

            Console.WriteLine("=== Прототип системи StudyHub v1.0 ===");
            Console.WriteLine("--------------------------------------");

            // Емуляція роботи студента 
            Student myStudent = new Student("Andrii_NPU", "1234567", 2405);
            // Додаємо студента в загальну колекцію користувачів
            storage.AddUser(myStudent);
            Console.WriteLine($"Авторизовано: {myStudent.Login}");
            
            myStudent.SearchMaterial();      
            myStudent.UploadFile();  
            
            var oopMaterial = new StudyMaterial("ООП на C#", SubjectCategory.Programming);
            var mathMaterial = new StudyMaterial("Вища Математика", SubjectCategory.Mathematics);
            
            myStudent.AddMaterial(oopMaterial);
            myStudent.AddMaterial(mathMaterial);
            // Другий виклик показує, що дублікати в обраному не проходять
            myStudent.SaveToFavorites(oopMaterial);
            myStudent.SaveToFavorites(oopMaterial);
            
            // Складаємо матеріали в сховище з групуванням за предметами
            storage.AddMaterial(oopMaterial);
            storage.AddMaterial(mathMaterial);
            
            myStudent.DisplayInfo();
            foreach (var item in myStudent.MyMaterials)
            {
                Console.WriteLine($"- [{item.Subject}] {item.Title}");
            }


            Console.WriteLine("--------------------------------------");

            // Емуляція роботи модератора
            Moderator myAdmin = new Moderator("Olena_Admin", "123456", 1776, "AAAAA:aaaaaa");
            storage.AddUser(myAdmin);
            Console.WriteLine($"Авторизовано: {myAdmin.Login}");
            
            myAdmin.UploadFile();    
            myAdmin.DeleteFile();

            Console.WriteLine("\nКористувачі в системі:");
            foreach (var user in storage.GetUsers())
            {
                Console.WriteLine($"- {user.Login}");
            }

            Console.WriteLine("\nМатеріали з програмування:");
            foreach (var material in storage.GetMaterialsBySubject(SubjectCategory.Programming))
            {
                Console.WriteLine($"- {material.Title}");
            }

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
