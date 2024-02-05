using System;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace WpfApp1.Logic
{
    /// <summary>
    /// Импортирует данные из excel-файла определенного формата в базу данных
    /// </summary>
    public class ExcelImporter
    {
        protected readonly string _connectionString;

        /// <summary>
        /// Конструктор объекта
        /// </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        /// <exception cref="IOException">Подключение к БД не состоялось</exception>
        public ExcelImporter(string connectionString)
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
        /// Метод, отвечающий за вставку новых записей в таблицы [Sheets], [Banks] и [Periods].
        /// Вызывает хранимую процедуру. В out-параметрах выдаёт id банка и id соответствующего периода.
        /// </summary>
        /// <param name="command">Объект SqlCommand с открытым подключением</param>
        /// <param name="fileLocation">Расположение файла на диске</param>
        /// <param name="bankName">Название банка</param>
        /// <param name="dateFrom">Период с</param>
        /// <param name="dateTo">Период по</param>
        /// <param name="idBank">Выходной параметр id банка</param>
        /// <param name="idPeriod">Выходной параметр id периода</param>
        protected void InsertFile(SqlCommand command, string fileLocation, string bankName, string dateFrom, string dateTo, out long idBank, out long idPeriod)
        {
            SqlDataReader reader = null;
            string cmd =
                $"EXEC IncertFile N'{fileLocation}', N'{bankName}', '{dateFrom}', '{dateTo}'; " +
                @"DECLARE @bank BIGINT, @period BIGINT; " +
                $"EXEC GetBankAndPeriodId N'{bankName}', '{dateFrom}', '{dateTo}', @bank OUTPUT, @period OUTPUT; " +
                $"SELECT @bank, @period; ";
            command.CommandText = cmd;
            try
            {
                reader = command.ExecuteReader();
                reader.Read();
                idBank = long.Parse(reader[0].ToString());
                idPeriod = long.Parse(reader[1].ToString());
                reader.Close();
            }
            catch
            {
                reader?.Close();
                throw;
            }
        }

        /// <summary>
        /// Метод, отвечающий за вставку записей в таблицу [Bills]
        /// </summary>
        /// <param name="command">Объект SqlCommand с открытым подключением</param>
        /// <param name="idBank">Id банка</param>
        /// <param name="idPeriod">Id периода</param>
        /// <param name="billNumber">Номер счета</param>
        /// <param name="OpeningBalanceAsset">Входящее сальдо, актив</param>
        /// <param name="OpeningBalanceLiability">Входящее сальдо, пассив</param>
        /// <param name="TurnoverDebit">Обороты, дебет</param>
        /// <param name="TurnoverCredit">Обороты, кредит</param>
        /// <param name="ClosingBalanceAsset">Исходящее сальдо, актив</param>
        /// <param name="ClosingBalanceLiability">Исходящее сальдо, пассив</param>
        protected void InsertBill(SqlCommand command, long idBank, long idPeriod, string billNumber,
                                    string OpeningBalanceAsset, string OpeningBalanceLiability,
                                    string TurnoverDebit, string TurnoverCredit,
                                    string ClosingBalanceAsset, string ClosingBalanceLiability)
        {
            string cmd = $"EXEC IncertBill '{idBank}', '{idPeriod}', '{billNumber}' " +
                         $", '{OpeningBalanceAsset}', '{OpeningBalanceLiability}' " +
                         $", '{TurnoverDebit}', '{TurnoverCredit}' " +
                         $", '{ClosingBalanceAsset}', '{ClosingBalanceLiability}'; ";
            command.CommandText = cmd;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Импортирует ".xls/.xlsx" файлы определённого формата в базу данных.
        /// Формат файла (из источника): 
        ///     A1 - Название банка, 
        ///     А3 - содержит 2 даты (с, по) формата "dd.MM.yyyy"
        ///     Начиная с А10-G10 - таблица с данными
        /// </summary>
        /// <param name="sourceFile">Файл с данными</param>
        public void ImportFile(FileInfo sourceFile)
        {
            if (sourceFile is null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (!sourceFile.Exists)
            {
                throw new ArgumentException("Файл не существует", nameof(sourceFile));
            }

            if (sourceFile.Extension != ".xls" && sourceFile.Extension != ".xlsx")
            {
                throw new ArgumentException("Расширение файла не \".xls\"/\".xlsx\"", nameof(sourceFile));
            }

            Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            Excel.Worksheet xlWorkSheet = null;
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand sqlCommand = null;
            try
            {
                connection.Open();
                sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "BEGIN TRANSACTION";
                sqlCommand.ExecuteNonQuery();

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(sourceFile.FullName, 0, true,
                                                                    5, "", "", true, Excel.XlPlatform.xlWindows,
                                                                    "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                //xlWorkSheet = (Excel.Worksheet)xlWorkBook.ActiveSheet;
                Excel.Range range = xlWorkSheet.UsedRange;

                // Название банка в ячейке А1
                string bankName = GetCellValue(xlWorkSheet, 1, 1);

                // Период (две даты формата dd.MM.yyyy) в ячейке А3
                string periodText = GetCellValue(xlWorkSheet, 3, 1);
                MatchCollection matches = new Regex(@"\d{2}.\d{2}.\d{4}").Matches(periodText);
                string dateFrom = string.Join('-', matches[0].Value.Split('.').Reverse());
                string dateTo = string.Join('-', matches[1].Value.Split('.').Reverse());
                this.InsertFile(
                    sqlCommand, sourceFile.FullName, bankName, dateFrom, dateTo, out long idBank, out long idPeriod);


                // Строки таблицы (начиная с 10й)
                for (int row = 10; row < range.Rows.Count; row++)
                {
                    int bill;
                    if (!int.TryParse(GetCellValue(xlWorkSheet, row, 1), out bill)
                        || bill < 1000 || bill > 9999)
                    {
                        continue;
                    }

                    this.InsertBill(
                        command: sqlCommand,
                        idBank: idBank,
                        idPeriod: idPeriod,
                        billNumber: GetCellValue(xlWorkSheet, row, 1),
                        OpeningBalanceAsset: GetCellValue(xlWorkSheet, row, 2),
                        OpeningBalanceLiability: GetCellValue(xlWorkSheet, row, 3),
                        TurnoverDebit: GetCellValue(xlWorkSheet, row, 4),
                        TurnoverCredit: GetCellValue(xlWorkSheet, row, 5),
                        ClosingBalanceAsset: GetCellValue(xlWorkSheet, row, 6),
                        ClosingBalanceLiability: GetCellValue(xlWorkSheet, row, 7));
                }

                sqlCommand.CommandText = "COMMIT TRANSACTION";
                sqlCommand.ExecuteNonQuery();
            }
            catch
            {
                xlWorkBook?.Close(false, null, null);
                xlApp?.Quit();
                Marshal.ReleaseComObject(xlWorkSheet);
                Marshal.ReleaseComObject(xlWorkBook);
                Marshal.ReleaseComObject(xlApp);
                sqlCommand.CommandText = "ROLLBACK TRANSACTION";
                sqlCommand.ExecuteNonQuery();
                connection?.Close();
                throw;
            }

            xlWorkBook?.Close(false, null, null);
            xlApp?.Quit();
            connection?.Close();
            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
        }

        protected string GetCellValue(Excel.Worksheet worksheet, int row, int column)
        {
            string value = (worksheet.Cells[row, column] as Excel.Range).Value.ToString();
            if (decimal.TryParse(value, out decimal res))
            {
                return Math.Round(res, 2).ToString(CultureInfo.InvariantCulture);
            }

            return value;
        }
    }
}
