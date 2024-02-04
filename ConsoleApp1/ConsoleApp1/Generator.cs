using System;
using System.IO;

namespace ConsoleApp1
{
    /// <summary>
    /// Задание 1.1
    /// Генерирует файлы с шаблонными строками
    /// </summary>
    public class Generator
    {
        protected Random _random;

        public Generator()
        {
            _random = new Random();
        }

        /// <summary>
        /// Генерирует случайную дату за последние 5 лет
        /// </summary>
        /// <returns>Строка с датой в формате "dd.MM.yyyy"</returns>
        protected string GenerateRndDate()
        {
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - new DateTime(
                                                year: (now.Year - 5), 
                                                month: now.Month, 
                                                day: now.Day);
            int days = timeSpan.Days;
            int rndValue = _random.Next(0, days);
            DateTime res = now.AddDays(-rndValue);

            return res.ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// Генерирует случайный набор из 10 латинских символов
        /// </summary>
        /// <returns>Строка с 10-ю латинскими символами</returns>
        protected string GenerateRndLatSymbols()
        {
            char[] res = new char[10];
            for (int i = 0; i < res.Length; i++)
            {
                bool upper = _random.Next(0, 2) == 1;
                int rnd = _random.Next(0, 26);
                res[i] = upper ? (char)('Z' - rnd) : (char)('z' - rnd);
            }

            return new string(res);
        }

        /// <summary>
        /// Генерирует случайный набор из 10 русских символов
        /// </summary>
        /// <returns>Строка с 10-ю русскими символами</returns>
        protected string GenerateRndRusSymbols()
        {
            char[] res = new char[10];
            for (int i = 0; i < res.Length; i++)
            {
                bool upper = _random.Next(0, 2) == 1;
                int rnd = _random.Next(0, 32);
                res[i] = upper ? (char)('Я' - rnd) : (char)('я' - rnd);
            }

            return new string(res);
        }


        /// <summary>
        /// Генерирует случайное положительное четное целочисленное число в диапазоне от 1 до 100 000 000
        /// </summary>
        /// <returns>Положительное четное целочисленное число</returns>
        protected int GenerateRndEvenNumber()
        {
            return _random.Next(1, 50_000_000) * 2;
        }

        /// <summary>
        /// Генерирует случайное положительное число с 8 знаками после запятой в диапазоне от 1 до 20
        /// </summary>
        /// <returns>Положительное число с 8 знаками после запятой</returns>
        protected double GenerateRndFloatNumber()
        {
            const int e8 = 100_000_000;

            return (double)(_random.Next(1 * e8, 20 * e8)) / e8;
        }

        /// <summary>
        /// Генерирует строку в заданном шаблоне: 
        /// случайная дата за последние 5 лет || 
        /// случайный набор 10 латинских символов || 
        /// случайный набор 10 русских символов || 
        /// случайное положительное четное целочисленное число в диапазоне от 1 до 100 000 000   || 
        /// случайное положительное число с 8 знаками после запятой в диапазоне от 1 до 20 
        /// </summary>
        /// <returns>Строка по шаблону</returns>
        /// <example>"03.03.2015||ZAwRbpGUiK||мДМЮаНкуКД||14152932||7,87742021||"</example>
        protected string GenerateLine()
        {
            return 
                $"{GenerateRndDate()}||{GenerateRndLatSymbols()}||{GenerateRndRusSymbols()}||{GenerateRndEvenNumber()}||{GenerateRndFloatNumber().ToString("f8")}";
        }

        /// <summary>
        /// Генерирует 100 текстовых файлов со 100_000 строками каждый
        /// </summary>
        /// <param name="directory">Директория, где будут создаваться файлы</param>
        /// <exception cref="ArgumentException">Директория не пуста</exception>
        /// <exception cref="ArgumentException">Директория не может быть создана</exception>
        /// <exception cref="IOException">Ошибка при создании/записи файлов</exception>
        /// <returns>Коллекция файлов</returns>
        public FileInfo[] GenereFiles(DirectoryInfo directory)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!directory.Exists)
            {
                try
                {
                    directory.Create();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(message: "Директория не может быть создана", nameof(directory), innerException: ex);
                }
            }
            else
            {
                if (directory.GetFiles().Length != 0 || directory.GetDirectories().Length != 0)
                {
                    throw new ArgumentException("Директория не пуста", nameof(directory));
                }
            }

            #warning Поменять на 100 и 100_000 (будет работать секунд 20)
            int numberOfFiles = 10;
            int numberOfLines = 1000;
            FileInfo[] resultFiles = new FileInfo[numberOfFiles];

            try
            {
                for (int fileNumber = 0; fileNumber < numberOfFiles; fileNumber++)
                {
                    FileInfo file = new FileInfo(Path.Combine(directory.FullName, $"{fileNumber}.txt"));
                    using (StreamWriter writer = new StreamWriter(file.Create()))
                    {
                        for (int lineNumber = 0; lineNumber < numberOfLines; lineNumber++)
                            writer.WriteLine(this.GenerateLine());
                    }

                    resultFiles[fileNumber] = file;
                }

                return resultFiles;
            }
            catch(Exception ex) //Удалить уже созданние файлы (т.к. директория была изначально пуста)
            {
                foreach(FileInfo file in directory.EnumerateFiles())
                    file.Delete();

                throw new IOException(message: "Ошибка при создании/записи файлов", innerException: ex);
            }
        }
    }
}
