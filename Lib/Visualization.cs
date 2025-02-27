using Logs;
using Spectre.Console;
using System.Globalization;

namespace ServiceLibrary
{
    public static class Visualization
    {
        // Количество записей на странице
        private const int PageSize = 10;
        /// <summary>
        /// Метод для визуализации данных, используя таблицу.
        /// </summary>
        /// <param name="logs">Список с логами.</param>
        public static void Table(List<Log> logs)
        {
            if (logs == null || logs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            int page = 0;
            while (true)
            {
                AnsiConsole.Clear();
                /* Вычисление записей для текущей страницы
                 * 
                 * Т.к. у нас PageSize логов на одной странице, значит для n-ой страницы мы первые n-1 * PageSize 
                 * логов пропускаем и берем следующие PageSize, которые и будут на этой странице.
                 */
                List<Log> pageLogs = logs.Skip(page * PageSize).Take(PageSize).ToList();

                Table table = new Table();
                table.AddColumn("Дата");
                table.AddColumn("Уровень важности");
                table.AddColumn("Сообщение");
                foreach (var log in pageLogs)
                {
                    table.AddRow(log.dateTime.ToString(), log.level, log.message);
                }
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dodgerblue3]Страница {page + 1} из {GetTotalPages(logs.Count)}[/]");
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Выберите действие:")
                        .AddChoices(["Следующая страница", "Предыдущая страница", "Выход"]));
                switch (choice)
                {
                    case "Следующая страница":
                        if ((page + 1) * PageSize < logs.Count)
                        {
                            page++;
                        }
                        break;
                    case "Предыдущая страница":
                        if (page > 0)
                        {
                            page--;
                        }
                        break;
                    case "Выход":
                        AnsiConsole.Clear();
                        return;
                }
            }
        }

        /// <summary>
        /// Метод для вычисления общего количества страниц таблицы.
        /// </summary>
        /// <param name="totalItems">Количество элементов, которые нужно разместить в таблице.</param>
        /// <returns>Количество страниц.</returns>
        private static int GetTotalPages(int totalItems)
        {
            return (int)Math.Ceiling((double)totalItems / PageSize);
        }

        /// <summary>
        /// Метод, выводящий данные в виде диаграммы.
        /// </summary>
        /// <param name="logs">Список логов, которые будем визуализировать.</param>
        public static void BreakdownChart(List<Log> logs)
        {
            if (LogFilters._logs == null)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }

            DateTime startDate;
            DateTime endDate;

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Вывести данные за всё время или за период?")
                .AddChoices(["За всё время", "Выбрать период"]));
            if (choice == "Выбрать период")
            {
                // Ввод начальной даты.
                while (true)
                {
                    AnsiConsole.MarkupLine("Введите начальную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: ");
                    var input = Console.ReadLine();
                    if (input?.ToLower() == "0")
                        return;

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
                        return;

                    if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                        break;
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("Некорректная дата. Попробуйте снова.");
                }
            }
            else
            {
                // Выбираем записи за всё время.
                startDate = DateTime.MinValue;
                endDate = DateTime.MaxValue;
            }


            AnsiConsole.Clear();
            // Фильтрация логов, с выбором попавших в диапазон.
            var filteredLogs = logs
                .Where(log => log.dateTime >= startDate && log.dateTime <= endDate)
                .ToList();

            if (filteredLogs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет данных для отображения за выбранный период.[/]");
                return;
            }

            // Группировка логов по уровням важности для дальнейшего вывода.
            var logGroups = filteredLogs
                .GroupBy(log => log.level)
                .Select(group => new
                {
                    Level = group.Key,
                    Count = group.Count()
                })
                .ToList();

            // Создание диаграммы.
            var chart = new BreakdownChart()
                .FullSize()
                .ShowPercentage();

            int colorMixer = 1;
            // Добавление данных в диаграмму.
            foreach (var group in logGroups)
            {
                colorMixer = (colorMixer * group.Count) % 255;
                chart.AddItem(group.Level, group.Count, colorMixer);
                colorMixer *= 3;
            }
            AnsiConsole.Write(chart);
            AnsiConsole.MarkupLine("[dim gray]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
        }


        /// <summary>
        /// Метод, выводящий данные о логах, отображая их в календаре.
        /// </summary>
        /// <param name="logs">Список обрабатываемых логов.</param>
        public static void Calendar(List<Log> logs)
        {
            if (logs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет данных для отображения.[/]");
                return;
            }

            // Находим минимальную и максимальную даты.
            var minDate = logs.Min(log => log.dateTime);
            var maxDate = logs.Max(log => log.dateTime);

            // Группировка логов по дням.
            var logsByDay = logs
                .GroupBy(log => log.dateTime.Date)
                .ToDictionary(group => group.Key, group => group.Count());

            // Отображение календарей для каждого месяца в диапазоне.
            for (var date = minDate; date <= maxDate; date = date.AddMonths(1))
            {
                int year = date.Year;
                int month = date.Month;
                var calendar = new Spectre.Console.Calendar(year, month)
                    .HighlightStyle(Style.Plain);

                // Добавление событий для дней с записями.
                foreach (var day in logsByDay)
                {
                    if (day.Key.Year == year && day.Key.Month == month)
                    {
                        int dayOfMonth = day.Key.Day;
                        int logCount = day.Value;

                        // Определение цвета в зависимости от количества записей.
                        Color color = GetColorForLogCount(logCount);

                        // Добавление события с настройкой стиля.
                        calendar.AddCalendarEvent(year, month, dayOfMonth)
                                .HighlightStyle(color);
                    }
                }

                AnsiConsole.Write(calendar);
            }
            AnsiConsole.MarkupLine("[dim grey]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }

        /// <summary>
        /// Метод, присваивающий цвет в соответствии с "интенсивностью" количества логов в указанный день.
        /// </summary>
        /// <param name="logCount">Количество логов в какой-то день</param>
        /// <returns></returns>
        private static Color GetColorForLogCount(int logCount)
        {
            if (logCount == 0) return Color.Default;

            return logCount switch
            {
                <= 5 => Color.Green1,
                <= 10 => Color.Green3,
                <= 20 => Color.Green4,
                _ => Color.DarkOliveGreen1
            };
        }

        /// <summary>
        /// Метод, выводящий меню с выбором визулизации.
        /// </summary>
        public static void VisualizationMenu()
        {
            if (LogFilters._logs == null)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Выберите способ визуализации:")
                        .AddChoices(["Таблицей", "Календарем", "Диаграммой", "Выход"]));
                switch (choice)
                {
                    case "Таблицей":
                        Table(LogFilters._logs);
                        break;
                    case "Календарем":
                        Calendar(LogFilters._logs);
                        break;
                    case "Диаграммой":
                        BreakdownChart(LogFilters._logs);
                        AnsiConsole.Clear();
                        break;
                    case "Выход":
                        return;
                }
            }
        }
    }
}
