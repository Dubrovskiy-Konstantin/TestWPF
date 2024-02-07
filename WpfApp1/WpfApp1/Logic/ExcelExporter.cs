using System;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections.Generic;

namespace WpfApp1.Logic
{
    /// <summary>
    /// Получает импортированные из excel-файлов данные из базы данных
    /// </summary>
    public class ExcelExporter
    {
        protected readonly string _connectionString;

        /// <summary>
        /// Конструктор объекта
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        /// <exception cref="IOException">Подключение к БД не состоялось</exception>
        public ExcelExporter(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                throw new IOException("Connection is not opened", innerException: ex);
            }

            connection.Close();
            this._connectionString = connectionString;
        }

        /// <summary>
        /// Получает имена файлов, хранящихся в базе данных
        /// </summary>
        /// <returns>Список имен файлов</returns>
        public List<string> GetFileNames()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand sqlCommand = null;
            SqlDataReader reader = null;
            List<string> result = new List<string>();
            try
            {
                connection.Open();
                sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "SELECT * FROM [dbo].[Sheets]";
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader["Location"].ToString());
                }
                reader.Close();
                connection.Close();
                return result;
            }
            catch
            {
                connection?.Close();
                reader?.Close();
                throw;
            }
        }

        /// <summary>
        /// Возвращает все счета по эаданному файлу из базы данных
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <returns>Список элементов для DataGrid</returns>
        public List<Bill> GetBills(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Ошибка названия файла", nameof(filename));
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand sqlCommand = null;
            SqlDataReader reader = null;
            List<Bill> result = new List<Bill>();
            try
            {
                connection.Open();
                sqlCommand = connection.CreateCommand();

                string cmd = $"SELECT TOP 1 [Id_Bank], [Id_Period] FROM [dbo].[Sheets] WHERE [Location] LIKE N'{filename}';";
                sqlCommand.CommandText = cmd;
                reader = sqlCommand.ExecuteReader();
                reader.Read();
                long idBank = long.Parse(reader[0].ToString());
                long idPeriod = long.Parse(reader[1].ToString());
                reader.Close();

                cmd = $"SELECT * FROM [dbo].[Bills] WHERE [Id_Bank] = '{idBank}' AND [Id_Period] = '{idPeriod}';";
                sqlCommand.CommandText = cmd;
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new Bill()
                    {
                        col0 = reader["Bill_Number"].ToString(),
                        col1 = reader["Opening_Balance_Asset"].ToString(),
                        col2 = reader["Opening_Balance_Liability"].ToString(),
                        col3 = reader["Turnover_Debit"].ToString(),
                        col4 = reader["Turnover_Credit"].ToString(),
                        col5 = reader["Closing_Balance_Asset"].ToString(),
                        col6 = reader["Closing_Balance_Liability"].ToString(),
                    });
                }
                reader.Close();
                connection.Close();
                return result;
            }
            catch
            {
                connection?.Close();
                reader?.Close();
                throw;
            }
        }
    }
}
