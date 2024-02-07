using System;
using System.IO;
using System.Data.SqlClient;

namespace WpfApp1.Logic
{
    /// <summary>
    /// Задание 1.3. Импортировать данные из объединённого файла в БД. 
    /// При импорте должен выводиться ход процесса (сколько строк импортировано, сколько осталось
    /// </summary>
    public class DBImporter
    {
        protected readonly string _connectionString;

        /// <param name="connectionString">Строка подключения к БД</param>
        public DBImporter(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Функция вывода прогресса записанных строк.
        /// Вызывается в процессе записи. Передаёт строку с текущим прогрессом в функцию
        /// </summary>
        /// <param name="str">Вывод</param>
        /// <example>Импортировано 120 из 100000 строк</example>
        public delegate void PrintProgress(string str);

        /// <summary>
        /// Импортирует данные из файла в БД с возможностью
        /// вывода хода процесса (сколько строк импортировано, сколько осталось)
        /// </summary>
        /// <param name="sourceFile">Файл с данными</param>
        /// <param name="printProgress">Функция вывода хода процесса</param>
        public void ImportFromFile(FileInfo sourceFile, PrintProgress printProgress = null)
        {
            if (sourceFile is null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (!sourceFile.Exists)
            {
                throw new ArgumentException("Файл не существует", nameof(sourceFile));
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                throw new IOException("Connection is not opened", innerException: ex);
            }

            SqlCommand sqlCommand = connection.CreateCommand();
            try
            {
                sqlCommand.CommandText = "BEGIN TRANSACTION";
                sqlCommand.ExecuteNonQuery();
                using (StreamReader reader = new StreamReader(sourceFile.OpenRead()))
                {
                    string line = reader.ReadLine();
                    int linesCounter = 0;
                    while (!string.IsNullOrEmpty(line))
                    {
                        Task1DataFormatAllString data = new Task1DataFormatAllString(line);
                        string command = "INSERT INTO [dbo].[Task1] ([RndDate],[RndLatSymbols],[RndRusSymbols],[RndEvenNumber],[RndFloatNumber]) VALUES (" +
                                            $"'{data.RndDate}', " +
                                            $"'{data.RndLatSymbols}', " +
                                            $"N'{data.RndRusSymbols}', " +
                                            $"'{data.RndEvenNumber}', " +
                                            $"'{data.RndFloatNumber}' ); ";
                        sqlCommand.CommandText = command;
                        sqlCommand.ExecuteNonQuery();
                        linesCounter++;
                        if (!(printProgress is null))
                        {
                            printProgress($"импортировано {linesCounter} строк");
                        }

                        line = reader.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                sqlCommand.CommandText = "ROLLBACK TRANSACTION";
                sqlCommand.ExecuteNonQuery();
                connection.Close();
                throw new IOException("Ошибка, транзакция не закончена", innerException: ex);
            }
            finally
            {
                sqlCommand.CommandText = "COMMIT TRANSACTION";
                sqlCommand.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
