using System;
using System.Linq;

namespace WpfApp1.Logic
{
    /// <summary>
    /// Конвертирует строку из файла задания 1 в объект класса.
    /// Наследникам абстрактного класса необходимо определить каким образом хранить и использовать данные из строки
    /// (например целое число "458782" использовать как string или int)
    /// </summary>
    /// <typeparam name="dateFormat">DateTime или string</typeparam>
    /// <typeparam name="symbolsFormat">char[] или string</typeparam>
    /// <typeparam name="numeralFormat">int инт string</typeparam>
    /// <typeparam name="floatFormat">double или string</typeparam>
    public abstract class Task1DataFormat<dateFormat, symbolsFormat, numeralFormat, floatFormat>
    {
        public abstract dateFormat RndDate { get; }
        public abstract symbolsFormat RndLatSymbols { get; }
        public abstract symbolsFormat RndRusSymbols { get; }
        public abstract numeralFormat RndEvenNumber { get; }
        public abstract floatFormat RndFloatNumber { get; }

        public Task1DataFormat(string source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
        }

        public override abstract string ToString();
    }

    /// <summary>
    /// Конвертирует строку из задания 1 в объект класса.
    /// Все данные хранятся как string
    /// </summary>
    public class Task1DataFormatAllString : Task1DataFormat<string, string, string, string>
    {
        public override string RndDate { get; }

        public override string RndLatSymbols { get; }

        public override string RndRusSymbols { get; }

        public override string RndEvenNumber { get; }

        public override string RndFloatNumber { get; }

        public Task1DataFormatAllString(string source) : base(source)
        {
            string[] parts = source.Split("||");
            if (parts.Length != 5)
            {
                throw new ArgumentException("Неверный формат строки", nameof(source));
            }

            this.RndDate = string.Join('.', parts[0].Split('.').Reverse());
            this.RndLatSymbols = parts[1];
            this.RndRusSymbols = parts[2];
            this.RndEvenNumber = parts[3];
            this.RndFloatNumber = parts[4].Replace(',', '.');
        }

        public override string ToString() =>
            $"{RndDate}||{RndLatSymbols}||{RndRusSymbols}||{RndEvenNumber}||{RndFloatNumber}";
    }
}
