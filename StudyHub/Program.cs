using System;
using System.Text;

namespace StudyHubPrototype
{
    // Клас User
    public class User 
    {
        public string Login { get; set; }
        
        public void SearchMaterial() 
        {
            Console.WriteLine($"[User]: Користувач {Login} виконує пошук конспектів...");
        }

        public void DownloadFile()
        {
            Console.WriteLine($"[User]: Користувач {Login} виконує завантаження конспектів на свій пристрій...");
        }
    }

    // Клас Student
    public class Student : User 
    {
        public int StudentID { get; set; }

        public void UploadFile() 
        {
            Console.WriteLine($"[Student]: Студент (ID: {StudentID}) завантажує новий файл на StudyHub.");
        }
    }

    // Клас Moderator 
    public class Moderator : Student 
    {
        public void DeleteFile() 
        {
            Console.WriteLine("[Moderator]: Модератор видалив неактуальний матеріал.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Прототип системи StudyHub v1.0 ===");
            Console.WriteLine("--------------------------------------");

            // Емуляція роботи студента 
            Student myStudent = new Student { Login = "Andrii_NPU", StudentID = 2405 };
            Console.WriteLine($"Авторизовано: {myStudent.Login}");
            
            myStudent.SearchMaterial();      
            myStudent.UploadFile();  

            Console.WriteLine("--------------------------------------");

            // Емуляція роботи модератора
            Moderator myAdmin = new Moderator { Login = "Admin_Olena" };
            Console.WriteLine($"Авторизовано: {myAdmin.Login}");
            
            myAdmin.UploadFile();    
            myAdmin.DeleteFile();    

            Console.WriteLine("\nКаркас програми запущено успішно. Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}