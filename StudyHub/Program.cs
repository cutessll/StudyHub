using System;
using System.Text;

namespace StudyHubPrototype
{
    
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.OutputEncoding = Encoding.UTF8;
            // UI працює через сервіс, без прямого доступу до сховища.
            IStudyHubService service = new StudyHubService(new StudyHubStorage());

            Console.WriteLine("=== Прототип системи StudyHub v1.0 ===");
            Console.WriteLine("--------------------------------------");

            // Емуляція роботи студента 
            // створення і збереження студента виконує сервіс.
            Student myStudent = service.RegisterStudent("Andrii_NPU", "1234567", 2405);
            Console.WriteLine($"Авторизовано: {myStudent.Login}");
            
            myStudent.SearchMaterial();      
            myStudent.UploadFile();  
            
            // матеріали додаються через сервісний шар.
            var oopMaterial = service.AddMaterialToStudent(myStudent, "ООП на C#", SubjectCategory.Programming);
            var mathMaterial = service.AddMaterialToStudent(myStudent, "Вища Математика", SubjectCategory.Mathematics);
            
            // Другий виклик показує, що дублікати в обраному не проходять
            service.AddToFavorites(myStudent, oopMaterial);
            service.AddToFavorites(myStudent, oopMaterial);

            // демонстрація агрегованих методів сервісу.
            Console.WriteLine($"\nСтатистика: користувачів = {service.GetUsersCount()}, матеріалів = {service.GetMaterialsCount()}");

            // UI отримує дані студента тільки через сервіс.
            Console.WriteLine("\nМатеріали студента (через сервіс):");
            foreach (var item in service.GetStudentMaterials(myStudent))
            {
                Console.WriteLine($"- [{item.Subject}] {item.Title}");
            }

            Console.WriteLine("\nОбране студента (до видалення):");
            foreach (var item in service.GetFavoriteMaterials(myStudent))
            {
                Console.WriteLine($"- [{item.Subject}] {item.Title}");
            }

            Console.WriteLine($"Видалення з обраного '{oopMaterial.Title}': " +
                              (service.RemoveFromFavorites(myStudent, oopMaterial) ? "успішно" : "не знайдено"));

            Console.WriteLine("Обране студента (після видалення):");
            foreach (var item in service.GetFavoriteMaterials(myStudent))
            {
                Console.WriteLine($"- [{item.Subject}] {item.Title}");
            }
            
            myStudent.DisplayInfo();
            foreach (var item in myStudent.MyMaterials)
            {
                Console.WriteLine($"- [{item.Subject}] {item.Title}");
            }


            Console.WriteLine("--------------------------------------");

            // Емуляція роботи модератора
            // реєстрація модератора теж через сервіс.
            Moderator myAdmin = service.RegisterModerator("Olena_Admin", "123456", 1776, "AAAAA:aaaaaa");
            Console.WriteLine($"Авторизовано: {myAdmin.Login}");
            
            myAdmin.UploadFile();    
            myAdmin.DeleteFile();

            Console.WriteLine("\nКористувачі в системі:");
            foreach (var user in service.GetUsers())
            {
                Console.WriteLine($"- {user.Login}");
            }

            Console.WriteLine("\nМатеріали з програмування:");
            foreach (var material in service.GetMaterialsBySubject(SubjectCategory.Programming))
            {
                Console.WriteLine($"- {material.Title}");
            }

            // пошук виконується через сервіс.
            Console.WriteLine("\nПошук користувачів за 'admin':");
            foreach (var user in service.FindUsers("admin"))
            {
                Console.WriteLine($"- {user.Login}");
            }

            // пошук матеріалів теж через сервіс.
            Console.WriteLine("\nПошук матеріалів за 'мат':");
            foreach (var material in service.FindMaterials("мат"))
            {
                Console.WriteLine($"- {material.Title}");
            }

            // видалення виконується через сервісний шар.
            Console.WriteLine($"\nВидалення матеріалу '{mathMaterial.Title}': " +
                              (service.RemoveMaterial(mathMaterial.Title) ? "успішно" : "не знайдено"));
            Console.WriteLine($"Видалення користувача '{myAdmin.Login}': " +
                              (service.RemoveUser(myAdmin.Login) ? "успішно" : "не знайдено"));

            Console.WriteLine("\nКористувачі після видалення:");
            foreach (var user in service.GetUsers())
            {
                Console.WriteLine($"- {user.Login}");
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
