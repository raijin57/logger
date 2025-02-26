using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logs
{
    public static class LogFilters
    {
        private static List<Func<Log, bool>> filters = new List<Func<Log, bool>>();
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

                if (DateTime.TryParseExact(input, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
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

                if (DateTime.TryParseExact(input, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    break;

                Console.WriteLine("Некорректная дата. Попробуйте снова.");
            }

            return FilterByDate(startDate, endDate);
        }

        /// <summary>
        /// Метод, позволяющий удалить фильтрации из списка фильтраций, которые будут выполнены.
        /// </summary>
        /// <param name="filters">Список с методами-фильтрами.</param>
        public static void ReorderFilters()
        {
            if (filters.Count == 0)
            {
                Console.WriteLine("Нет фильтров для изменения.");
                return;
            }

            Console.WriteLine("Текущие фильтры:");
            for (int i = 0; i < filters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {GetFilterDescription(filters[i])}");
            }

            Console.Write("Введите индекс фильтра для удаления (или 0 для завершения): ");
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "0") break;

                if (int.TryParse(input, out int index) && index > 0 && index <= filters.Count)
                {
                    filters.RemoveAt(index - 1);
                    Console.WriteLine("Фильтр удалён.");
                }
                else
                {
                    Console.WriteLine("Неверный индекс. Попробуйте снова.");
                }

                Console.WriteLine("Текущие фильтры:");
                for (int i = 0; i < filters.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {GetFilterDescription(filters[i])}");
                }

                Console.Write("Введите индекс фильтра для удаления (или 0 для завершения): ");
            }
        }

        /// <summary>
        /// Метод, возвращающий имя метода-фильтра.
        /// </summary>
        /// <param name="filter">Метод-фильтр.</param>
        /// <returns>Строка с названием.</returns>
        internal static string GetFilterDescription(Func<Log, bool> filter)
        {
            if (filter == FilterByDate(default, default))
                return "Фильтр по дате";
            if (filter == FilterByLevel(default))
                return "Фильтр по уровню важности";
            if (filter == FilterByMessage(default))
                return "Фильтр по ключевому слову";
            return "Неизвестный фильтр";
        }

        /// <summary>
        /// Метод, позволяющий выбрать набор фильтраций
        /// </summary>
        /// <param name="filters">Список с методами-фильтрами.</param>
        public static void SelectFilters()
        {
            Console.WriteLine("Доступные фильтры:");
            Console.WriteLine("1. Фильтр по дате");
            Console.WriteLine("2. Фильтр по уровню важности");
            Console.WriteLine("3. Фильтр по ключевому слову в сообщении");
            Console.Write("Выберите фильтр (или 0 для завершения): ");

            while (true)
            {
                var choice = Console.ReadLine();
                if (choice == "0") break;

                switch (choice)
                {
                    case "1":
                        var dateFilter = GetDateFilter();
                        if (dateFilter != null)
                        {
                            filters.Add(dateFilter);
                            Console.WriteLine("Фильтр по дате добавлен.");
                        }
                        break;
                    case "2":
                        Console.Write("Введите уровень важности (INFO, WARNING, ERROR): ");
                        var level = Console.ReadLine();
                        filters.Add(FilterByLevel(level));
                        Console.WriteLine("Фильтр по уровню важности добавлен.");
                        break;
                    case "3":
                        Console.Write("Введите ключевое слово: ");
                        var keyword = Console.ReadLine();
                        filters.Add(FilterByMessage(keyword));
                        Console.WriteLine("Фильтр по ключевому слову добавлен.");
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }

                Console.WriteLine("Выберите следующий фильтр (или 0 для завершения): ");
            }
        }

        /// <summary>
        /// Метод, применяющий выбранные фильтры.
        /// </summary>
        /// <param name="logs">Список с логами для фильтрации.</param>
        /// <param name="filters">Список с методами-фильтрами.</param>
        public static void ApplyFilters(List<Log> logs)
        {
            foreach (var filter in filters)
            {
                // Удаляем элементы, которые НЕ удовлетворяют фильтру.
                logs.RemoveAll(log => !filter(log)); 
            }
        }

        public static void PrintLogs(List<Log> logs)
        {
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
            Console.WriteLine();
        }
    }
}
