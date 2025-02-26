using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logs
{
    public static class LogFilters
    {
        /// <summary>
        /// Метод-фильтр для сортировки по диапазону дат.
        /// </summary>
        /// <param name="startDate">Начальная дата диапазона.</param>
        /// <param name="endDate">Конечная дата диапазона.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        internal static Func<Log, bool> FilterByDate(DateTime startDate, DateTime endDate)
        {
            return log => log.dateTime >= startDate && log.dateTime <= endDate;
        }

        /// <summary>
        /// Метод-фильтр для сортировки по уровню важности.
        /// </summary>
        /// <param name="level">Уровень важности.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        internal static Func<Log, bool> FilterByLevel(string level)
        {
            return log => log.level.Equals(level, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Метод-фильтр для сортировки по ключевому слову.
        /// </summary>
        /// <param name="keyword">Ключевое слово.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        internal static Func<Log, bool> FilterByMessage(string keyword)
        {
            return log => log.message.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Метод для получения корректных значений диапазона дат, для дальнейшей фильтрации.
        /// </summary>
        /// <returns>Вызывает метод-фильтр фильтрации по диапазону дат.</returns>
        internal static Func<Log, bool>? GetDateFilter()
        {
            DateTime startDate, endDate;

            // Ввод начальной даты
            while (true)
            {
                Console.Write("Введите начальную дату (гггг-мм-дд чч-мм-сс) или 'отмена' для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "отмена")
                    return null;

                if (DateTime.TryParse(input, out startDate))
                    break;

                Console.WriteLine("Некорректная дата. Попробуйте снова.");
            }

            // Ввод конечной даты
            while (true)
            {
                Console.Write("Введите конечную дату (гггг-мм-дд чч-мм-сс) или 'отмена' для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "отмена")
                    return null;

                if (DateTime.TryParse(input, out endDate))
                    break;

                Console.WriteLine("Некорректная дата. Попробуйте снова.");
            }

            return FilterByDate(startDate, endDate);
        }
    }
}
