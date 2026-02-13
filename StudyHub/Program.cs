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

            Console.WriteLine("--------------------------------------");

            // Емуляція роботи модератора
            Moderator myAdmin = new Moderator("Andrii_NPU", "123456", 1776, "dfdffadasda");
            Console.WriteLine($"Авторизовано: {myAdmin.Login}");
            
            myAdmin.UploadFile();    
            myAdmin.DeleteFile();    

            Console.WriteLine("\nКаркас програми запущено успішно. Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}