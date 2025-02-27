using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace Logs
{
    public static class LogFilters
    {
        public static List<Log> _logs;

        public static List<Func<Log, bool>> filters = new List<Func<Log, bool>>();

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
                AnsiConsole.MarkupLine("Введите начальную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                    return null;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                    break;
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("Некорректная дата. Попробуйте снова.");
            }

            // Ввод конечной даты
            while (true)
            {
                AnsiConsole.MarkupLine("Введите конечную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                    return null;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    break;
                AnsiConsole.Clear();
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
                filterDescriptions.Add(GetFilterDescription(filter));
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
                    .AddChoices(new[] { "По дате", "По уровню важности", "По ключевому слову", "Назад" }));

            switch (filterType)
            {
                case "По дате":
                    var dateFilter = GetDateFilter();
                    if (dateFilter != null)
                    {
                        filters.Add(dateFilter);
                        AnsiConsole.Clear();
                        AnsiConsole.MarkupLine("[green]Фильтр по дате добавлен.[/]");
                    }
                    break;
                case "По уровню важности":
                    var level = AnsiConsole.Ask<string>("Введите уровень важности (или \"0\" для отмены):");
                    if (level == "0") break;
                    filters.Add(FilterByLevel(level));
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("[green]Фильтр по уровню важности добавлен.[/]");
                    break;
                case "По ключевому слову":
                    var keyword = AnsiConsole.Ask<string>("Введите ключевое слово (или \"0\" для отмены):");
                    if (keyword == "0") break;
                    filters.Add(FilterByMessage(keyword));
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("[green]Фильтр по ключевому слову добавлен.[/]");
                    break;
                case "Назад":
                    AnsiConsole.Clear();
                    return;
            }
        }

        /// <summary>
        /// Метод, применяющий выбранные фильтры.
        /// </summary>
        /// <param name="logs">Список с логами для фильтрации.</param>
        /// <param name="filters">Список с методами-фильтрами.</param>
        public static void ApplyFilters(List<Log> logs)
        {
            if (filters == null || filters.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            foreach (var filter in filters)
            {
                // Удаляем элементы, которые НЕ удовлетворяют фильтру.
                logs.RemoveAll(log => !filter(log)); 
            }
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[dodgerblue3]Фильтры были применены.[/]");
            return;
        }


        /// <summary>
        /// Меню применения фильтров.
        /// </summary>
        /// <returns>Меню с возможностью настройки фильтров.</returns>
        public static void FilterMenu()
        {
            if (_logs == null)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Меню фильтрации:")
                        .AddChoices(["Добавить фильтр", "Удалить фильтр", "Применить фильтры", "Назад"]));
                switch (choice)
                {
                    case "Добавить фильтр":
                        AnsiConsole.Clear();
                        AddFilter(filters);
                        break;
                    case "Удалить фильтр":
                        AnsiConsole.Clear();
                        RemoveFilter(filters);
                        break;
                    case "Применить фильтры":
                        ApplyFilters(_logs);
                        return;
                    case "Назад":
                        return;
                }
            }
        }
    }
}
