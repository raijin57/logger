﻿using Spectre.Console;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Logs
{
    public static class LogFilters
    {
        // Список, где будут храниться все прочитанные данные из файла.
        public static List<Log> _logs;
        // Список, сохраняющий все выбранные фильтры.
        public static List<Func<Log, bool>> filters = new List<Func<Log, bool>>();
        /// <summary>
        /// Метод-фильтр для сортировки по диапазону дат.
        /// </summary>
        /// <param name="startDate">Начальная дата диапазона.</param>
        /// <param name="endDate">Конечная дата диапазона.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        public static Func<Log, bool> FilterByDate(DateTime startDate, DateTime endDate)
        {
            return log => log.Timestamp >= startDate && log.Timestamp <= endDate;
        }

        /// <summary>
        /// Метод-фильтр для сортировки по уровню важности.
        /// </summary>
        /// <param name="level">Уровень важности.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        public static Func<Log, bool> FilterByLevel(string level)
        {
            return log => log.ImportanceLevel.Equals(level, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Метод-фильтр для сортировки по ключевому слову.
        /// </summary>
        /// <param name="keyword">Ключевое слово.</param>
        /// <returns>Метод-фильтр для сортировки.</returns>
        public static Func<Log, bool> FilterByMessage(string keyword)
        {
            return log => log.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Метод для получения корректных значений диапазона дат, для дальнейшей фильтрации.
        /// </summary>
        /// <returns>Вызывает метод-фильтр фильтрации по диапазону дат.</returns>
        public static Func<Log, bool>? GetDateFilter()
        {
            DateTime startDate, endDate;

            // Ввод начальной даты.
            while (true)
            {
                AnsiConsole.MarkupLine("[dodgerblue3]Введите начальную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: [/]");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                {
                    AnsiConsole.Clear();
                    return null;
                }
                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate)) break;
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[red]Некорректная дата. Попробуйте снова.[/]");
            }

            // Ввод конечной даты.
            while (true)
            {
                AnsiConsole.MarkupLine("[dodgerblue3]Введите конечную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: [/]");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                {
                    AnsiConsole.Clear();
                    return null;
                }

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate)) break;
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[red]Некорректная дата. Попробуйте снова.[/]");
            }
            return FilterByDate(startDate, endDate);
        }

        /// <summary>
        /// Метод, позволяющий удалить фильтрации из списка фильтраций, которые будут выполнены.
        /// </summary>
        private static void RemoveFilter(List<Func<Log, bool>> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет фильтров для удаления.[/]");
                return;
            }
            // Список, с описаниями уже созданных фильтров (чтобы понимать какой удаляешь).
            List<string> filterDescriptions = new List<string>();
            foreach (var filter in filters)
            {
                filterDescriptions.Add(GetFilterDescription(filter));
            }
            string filterToRemove = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[invert]Выберите фильтр для удаления или введите \"0\" для отмены:[/]")
                    .AddChoices(filterDescriptions)
                    .AddChoices("[italic underline]Выход[/]")
                    .HighlightStyle(Color.DodgerBlue1));
            if (filterToRemove == "[italic underline]Выход[/]")
            {
                AnsiConsole.Clear();
                return;
            }
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
                return $"Фильтр по дате #{filters.IndexOf(filter) + 1}";
            if (filter.Method == FilterByLevel(default).Method)
                return $"Фильтр по уровню важности #{filters.IndexOf(filter) + 1}";
            if (filter.Method == FilterByMessage(default).Method)
                return $"Фильтр по ключевому слову #{filters.IndexOf(filter) + 1}";
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
                    .Title("[invert]Выберите тип фильтра:[/]")
                    .AddChoices(new[] { "По дате", "По уровню важности", "По ключевому слову", "[italic underline]Назад[/]" })
                    .HighlightStyle(Color.DodgerBlue1));

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
                case "[italic underline]Назад[/]":
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
            filters = null;
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
            if (_logs == null || _logs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[invert]Меню фильтрации:[/]")
                        .AddChoices(["Добавить фильтр", "Удалить фильтр", "Применить фильтры", "[italic underline]Назад[/]"])
                        .HighlightStyle(Color.DodgerBlue1));
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
                        AnsiConsole.Clear();
                        ApplyFilters(_logs);
                        return;
                    case "[italic underline]Назад[/]":
                        AnsiConsole.Clear();
                        return;
                }
            }
        }
    }
}
