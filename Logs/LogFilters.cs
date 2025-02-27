using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

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
        public static Func<Log, bool> FilterByLevel(string level)
        {
            return log => log.level.Equals(level, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Метод-фильтр для сортировки по ключевому слову.
        /// </summary>
        /// <param name="keyword">Ключевое слово.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        public static Func<Log, bool> FilterByMessage(string keyword)
        {
            return log => log.message.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Метод для получения корректных значений диапазона дат, для дальнейшей фильтрации.
        /// </summary>
        /// <returns>Вызывает метод-фильтр фильтрации по диапазону дат.</returns>
        public static Func<Log, bool>? GetDateFilter()
        {
            DateTime startDate, endDate;

            // Ввод начальной даты
            while (true)
            {
                AnsiConsole.MarkupLine("Введите начальную дату (гггг-мм-дд чч-мм-сс) или 'отмена' для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "отмена")
                    return null;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                    break;

                AnsiConsole.MarkupLine("Некорректная дата. Попробуйте снова.");
            }

            // Ввод конечной даты
            while (true)
            {
                AnsiConsole.MarkupLine("Введите конечную дату (гггг-мм-дд чч-мм-сс) или 'отмена' для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "отмена")
                    return null;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    break;

                AnsiConsole.MarkupLine("Некорректная дата. Попробуйте снова.");
            }

            return FilterByDate(startDate, endDate);
        }

        /// <summary>
        /// Метод, позволяющий удалить фильтрации из списка фильтраций, которые будут выполнены.
        /// </summary>
        private static void RemoveFilter(List<Func<Log, bool>> filters)
        {
            if (filters.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет фильтров для удаления.[/]");
                return;
            }

            var filterDescriptions = new List<string>();
            foreach (var filter in filters)
            {
                filterDescriptions.Add(LogFilters.GetFilterDescription(filter));
            }

            var filterToRemove = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Выберите фильтр для удаления:")
                    .AddChoices(filterDescriptions));

            var index = filterDescriptions.IndexOf(filterToRemove);
            filters.RemoveAt(index);
            AnsiConsole.MarkupLine("[green]Фильтр удалён.[/]");
        }

        /// <summary>
        /// Метод, возвращающий имя метода-фильтра.
        /// </summary>
        /// <param name="filter">Метод-фильтр.</param>
        /// <returns>Строка с названием.</returns>
        public static string GetFilterDescription(Func<Log, bool> filter)
        {
            if (filter.Method == FilterByDate(default, default).Method)
                return "Фильтр по дате";
            if (filter.Method == FilterByLevel(default).Method)
                return "Фильтр по уровню важности";
            if (filter.Method == FilterByMessage(default).Method)
                return "Фильтр по ключевому слову";
            return "Неизвестный фильтр";
        }

        /// <summary>
        /// Метод, позволяющий выбрать набор фильтраций
        /// </summary>
        /// <param name="filters">Список с методами-фильтрами.</param>
        public static async Task AddFilter(List<Func<Log, bool>> filters)
        {
            var filterType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Выберите тип фильтра:")
                    .AddChoices(new[] { "По дате", "По уровню важности", "По ключевому слову" }));

            switch (filterType)
            {
                case "По дате":
                    var dateFilter = LogFilters.GetDateFilter();
                    if (dateFilter != null)
                    {
                        filters.Add(dateFilter);
                        AnsiConsole.MarkupLine("[green]Фильтр по дате добавлен.[/]");
                    }
                    break;
                case "По уровню важности":
                    var level = AnsiConsole.Ask<string>("Введите уровень важности (INFO, WARNING, ERROR):");
                    filters.Add(LogFilters.FilterByLevel(level));
                    AnsiConsole.MarkupLine("[green]Фильтр по уровню важности добавлен.[/]");
                    break;
                case "По ключевому слову":
                    var keyword = AnsiConsole.Ask<string>("Введите ключевое слово:");
                    filters.Add(LogFilters.FilterByMessage(keyword));
                    AnsiConsole.MarkupLine("[green]Фильтр по ключевому слову добавлен.[/]");
                    break;
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

        /// <summary>
        /// Метод, выводящий на экран все логи.
        /// </summary>
        /// <param name="logs">Список с логами.</param>
        public static void PrintLogs(List<Log> logs)
        {
            if (logs == null)
            {
                Console.WriteLine("Сперва введите данные в программу");
                return;
            }
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
            Console.WriteLine();
        }

        // Меню фильтрации
        public static async Task FilterMenu()
        {
            var filters = new List<Func<Log, bool>>();

            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Меню фильтрации:")
                        .AddChoices(new[] { "Добавить фильтр", "Удалить фильтр", "Назад" }));

                switch (choice)
                {
                    case "Добавить фильтр":
                        AnsiConsole.Clear();
                        await AddFilter(filters);
                        break;
                    case "Удалить фильтр":
                        AnsiConsole.Clear();
                        RemoveFilter(filters);
                        break;
                    case "Назад":
                        return;
                }
            }
        }
    }
}
