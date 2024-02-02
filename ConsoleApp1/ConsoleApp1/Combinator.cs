using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    /// <summary>
    /// Задание 1.2
    /// Объединяет сгенерированные файлы в один. 
    /// </summary>
    public class Combinator
    {
        /// <summary>
        /// Объединяет все сгенерированные файлы в директории в один файл 
        /// с возможностью удалить из всех файлов строки с заданным сочетанием символов
        /// </summary>
        /// <param name="directory">Директория с файлами</param>
        /// <param name="deletePattern">Сочетание символов для удаления строки</param>
        /// <param name="deletedLines">Количество удалённых строк</param>
        /// <exception cref="ArgumentException">Директория не найдена</exception>
        public FileInfo Combine(DirectoryInfo directory, string deletePattern, out int deletedLines)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!directory.Exists)
            {
                throw new ArgumentException("Директория не найдена", nameof(directory));
            }

            return this.Combine(
                            sourceFiles: directory.GetFiles("*.txt", SearchOption.TopDirectoryOnly),
                            deletePattern: deletePattern,
                            deletedLines: out deletedLines);
        }

        /// <summary>
        /// Объединяет все сгенерированные файлы в директории в один файл 
        /// с возможностью удалить из всех файлов строки с заданным сочетанием символов
        /// </summary>
        /// <param name="sourceFiles">Коллекция с файлами</param>
        /// <param name="deletePattern">Сочетание символов для удаления строки</param>
        /// <param name="deletedLines">Количество удалённых строк</param>
        /// <exception cref="ArgumentException">Коллекция не содержит файлов</exception>
        /// <exception cref="ArgumentException">Коллекция содержит несуществующие файлы</exception>
        /// <returns>Объединенный файл</returns>
        public FileInfo Combine(FileInfo[] sourceFiles, string deletePattern, out int deletedLines)
        {
            if (sourceFiles is null)
            {
                throw new ArgumentNullException(nameof(sourceFiles));
            }

            if (sourceFiles.Length == 0)
            {
                throw new ArgumentException("Коллекция не содержит файлов", nameof(sourceFiles));
            }

            if (!sourceFiles.All( x => x.Exists))
            {
                throw new ArgumentException("Коллекция содержит несуществующие файлы", nameof(sourceFiles));
            }

            deletedLines = 0;
            bool needsToDelete = !string.IsNullOrEmpty(deletePattern);
            FileInfo[] files;
            FileInfo combinedFile = null;
            try
            {
                files = new FileInfo[sourceFiles.Length];
                Array.Copy(
                    sourceArray: sourceFiles,
                    destinationArray: files,
                    length: sourceFiles.Length);
                combinedFile = new FileInfo(Path.Combine(sourceFiles[0].DirectoryName, $"combined_{DateTime.Now.ToFileTime()}.txt"));

                using (StreamWriter writer = new StreamWriter(combinedFile.Create()))
                {
                    foreach (FileInfo file in files)
                    {
                        using (StreamReader reader = new StreamReader(file.OpenRead()))
                        {
                            string line = reader.ReadLine();
                            while (!string.IsNullOrEmpty(line))
                            {
                                if (needsToDelete && line.Contains(deletePattern))
                                {
                                    deletedLines++;
                                }
                                else
                                {
                                    writer.WriteLine(line);
                                }

                                line = reader.ReadLine();
                            }
                        }
                    }
                }

                return combinedFile;
            }
            catch (Exception ex)
            {
                combinedFile?.Delete();
                throw new IOException("Ошибка в работе с файлами", innerException: ex);
            }
        }
    }
}
