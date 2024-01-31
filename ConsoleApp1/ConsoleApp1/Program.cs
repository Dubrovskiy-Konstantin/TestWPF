using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    public static class Program
    {
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
                combinator.Combine(directory, deletePattern, out deletedLines);
                Console.WriteLine(deletedLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
