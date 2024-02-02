using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    public static class Program
    {
        #warning Можно перенести в .config
        public static string ConnectionString { get; } = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=TestWPF;Integrated Security=False";

        static void Main()
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo(@"C:\work\testWPF\ConsoleApp1\Files");
                foreach (FileInfo file in directory.EnumerateFiles())
                    file.Delete();

                //Задание 1.1
                Generator generator = new Generator();
                generator.GenereFiles(directory);
                //Задание 1.2
                Combinator combinator = new Combinator();
                int deletedLines;
                string deletePattern = "";
                FileInfo combinedFile = combinator.Combine(directory, deletePattern, out deletedLines);
                Console.WriteLine(deletedLines);
                Console.ReadKey();
                //Задание 1.3
                DBImporter importer = new DBImporter(ConnectionString);
                importer.ImportFromFile(combinedFile, Print);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static int _counter = 0;
        public static void Print(string str)
        {
            _counter++;
            if (_counter % 500 == 0)
            {
                Console.Clear();
                Console.WriteLine(str);
            }
        }
    }
}
