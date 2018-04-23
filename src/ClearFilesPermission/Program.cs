using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ClearFilesPermission
{
    public class Program
    {
        static void Main()
        {
            Console.WriteLine($"Welcome to Clear Files Permission v{Assembly.GetEntryAssembly().GetName().Version.ToString(3)} application\n");
            Start();
            Console.Read();
        }

        public static async void Start()
        {
            bool again;
            do
            {
                try
                {
                    Console.Write("\n\nPlease enter the target directory path: ");
                    var path = Console.ReadLine();
                    again = false;
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    {
                        throw new DirectoryNotFoundException("Entire path is not exist! Please try again...");
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    await Task.Run(() =>
                    {
                        var folder = new DirectoryInfo(path);
                        folder.NormalAttributer();
                    });
                }
                catch (Exception ex)
                {
                    ex.Catch();
                    again = true;
                }

            } while (again);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n\nThe given path is OK!");
        }
    }
}