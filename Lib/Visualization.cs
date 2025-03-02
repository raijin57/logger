using Logs;
using ScottPlot;
using Spectre.Console;
using System.Collections.Generic;
using System.Globalization;
namespace ServiceLibrary
{
    public static class Visualization
    {
        // Количество записей на одной странице табличного вида.
        private const int PageSize = 10;
        /// <summary>
        /// Метод для визуализации данных, используя таблицу.
        /// </summary>
        /// <param name="logs">Список с логами.</param>
        public static void TableVisualization()
        {
            int page = 0;
            while (true)
            {
                AnsiConsole.Clear();

                /* 
                 * Вычисление записей для текущей страницы: 
                 * Т.к. у нас PageSize логов на одной странице, значит для n-ой страницы мы первые n-1 * PageSize 
                 * логов пропускаем и берем следующие PageSize, которые и будут на этой странице.
                 */

                List<Log> pageLogs = LogFilters._logs.Skip(page * PageSize).Take(PageSize).ToList();
                Table table = new Table();
                table.AddColumn("Дата");
                table.AddColumn("Уровень важности");
                table.AddColumn("Сообщение");
                foreach (var log in pageLogs)
                {
                    table.AddRow(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), log.ImportanceLevel, log.Message);
                }
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dodgerblue2]Страница {page + 1} из {GetTotalTablePages(LogFilters._logs.Count)}[/]");
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Выберите действие:")
                        .AddChoices(["Следующая страница", "Предыдущая страница", "[italic underline]Назад[/]"])
                        .HighlightStyle(Spectre.Console.Color.DodgerBlue1));
                switch (choice)
                {
                    case "Следующая страница":
                        if ((page + 1) * PageSize < LogFilters._logs.Count)
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
                    case "[italic underline]Назад[/]":
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
        private static int GetTotalTablePages(int totalItems)
        {
            // Думал как-то пояснить, но логически должно быть понятно почему именно столько страниц.
            return totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / PageSize);
        }

        /// <summary>
        /// Метод, выводящий данные в виде диаграммы.
        /// </summary>
        /// <param name="logs">Список логов, которые будем визуализировать.</param>
        public static void BreakdownChartVisualization()
        {
            DateTime startDate;
            DateTime endDate;
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[invert]Вывести данные за всё время или за период?[/]")
                .AddChoices(["За всё время", "Выбрать период", "[italic underline]Назад[/]"])
                 .HighlightStyle(Spectre.Console.Color.DodgerBlue1));
            if (choice == "[italic underline]Назад[/]")
            {
                AnsiConsole.Clear();
                return;
            }
            else if (choice == "Выбрать период")
            {
                // Ввод начальной даты.
                while (true)
                {
                    AnsiConsole.MarkupLine("[dodgerblue2]Введите начальную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: [/]");
                    var input = Console.ReadLine();
                    if (input?.ToLower() == "0")
                    {
                        AnsiConsole.Clear();
                        return;
                    }

                    if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                    {
                        break;
                    }
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("[red]Некорректная дата. Попробуйте снова.[/]");
                }

                // Ввод конечной даты.
                while (true)
                {
                    AnsiConsole.MarkupLine("[dodgerblue2]Введите конечную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: [/]");
                    var input = Console.ReadLine();
                    if (input?.ToLower() == "0")
                    {
                        AnsiConsole.Clear();
                        return;
                    }
                    if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate)) break;
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("[red]Некорректная дата. Попробуйте снова.[/]");
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
            var filteredLogs = LogFilters._logs
                .Where(LogFilters.FilterByDate(startDate, endDate))
                .ToList();
            if (filteredLogs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет данных для отображения за выбранный период.[/]");
                return;
            }
            // Группировка логов по уровням важности для дальнейшего вывода.
            var logGroups = filteredLogs
                .GroupBy(log => log.ImportanceLevel)
                .Select(group => new
                {
                    Level = group.Key,
                    Count = group.Count()
                })
                .ToList();
            // Переменная общего количества логов в выбранном диапазоне.
            int intervalLogsCounter = 0;
            foreach (var group in logGroups)
            {
                intervalLogsCounter += group.Count;
            }
            // Создание диаграммы.
            var chart = new BreakdownChart()
                .FullSize()
                .ShowPercentage();

            /*
             * В диаграмме разные уровни важности должны отображаться разными цветами, отсюда возникла задача:
             * Как, имея только количество групп, элементов в нём и не зная сколько всего групп (пусть x) указать
             * x различных цветов для диаграммы?
             * Решение:
             * Переменная colorMixer - как старт. Умножаю её на количество элементов в группе (которая взята циклом ниже)
             * и беру остаток от 255 (всего столько id цветов), а в конце каждого цикла умножаю переменную на 11 (почему? хочется!).
             * Так мы и получаем различные цвета. Конечно, есть вероятность совпадений и выпадения чёрного, не отличимого от фона консоли,
             * но я считаю своё решение достойным.
             */

            int colorMixer = 1;
            // Добавление данных в диаграмму.
            foreach (var group in logGroups)
            {
                colorMixer = (colorMixer * group.Count) % 255;
                chart.AddItem(group.Level, Math.Round((double)group.Count / (double)intervalLogsCounter * 100), colorMixer);
                colorMixer *= 11;
            }
            AnsiConsole.Write(chart);
            AnsiConsole.MarkupLine("[dim gray]Нажмите Enter для выхода.[/]");
            AnsiConsole.Clear();
            Console.ReadLine();
        }


        /// <summary>
        /// Метод, выводящий данные о логах, отображая их в календаре.
        /// </summary>
        /// <param name="logs">Список обрабатываемых логов.</param>
        public static void CalendarVisualization()
        {
            if (LogFilters._logs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет данных для отображения.[/]");
                return;
            }
            // Находим минимальную и максимальную даты из логов.
            var minDate = LogFilters._logs.Min(log => log.Timestamp);
            var maxDate = LogFilters._logs.Max(log => log.Timestamp);
            // Группировка логов по дням в словарь.
            Dictionary<DateTime, int> logsByDay = LogFilters._logs
                .GroupBy(log => log.Timestamp.Date)
                .ToDictionary(group => group.Key, group => group.Count());
            // Отображение календарей для каждого месяца в диапазоне.
            for (var date = minDate; date <= maxDate; date = date.AddMonths(1))
            {
                int year = date.Year;
                int month = date.Month;

                /*
                 * Создаем таблицу для календаря, т.к. Calendar не может 
                 * конкретному дню менять цвет (для интенсивности).
                 */

                var table = new Table();
                // Подписываем месяц.
                table.AddColumn($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).Replace(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)[0], char.ToUpper(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)[0]))} {year}");
                // Получаем дни месяца.
                var daysInMonth = DateTime.DaysInMonth(year, month);
                // Первый день месяца.
                var firstDayOfMonth = new DateTime(year, month, 1);
                // День недели, с которого месяц начинается.
                var startingDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
                // Создаем строки для календаря.
                var currentDay = 1;
                // В календаре максимум 6 строк.
                for (int i = 0; i < 6; i++)
                {
                    var week = new List<string>();
                    // В неделе 7 дней.
                    for (int j = 0; j < 7; j++)
                    {
                        if (i == 0 && j < startingDayOfWeek)
                        {
                            // До первого дня месяца пустые ячейки.
                            week.Add("   "); 
                        }
                        else if (currentDay > daysInMonth)
                        {
                            // Пустые ячейки после последнего дня.
                            week.Add("   "); 
                        }
                        else
                        {
                            var currentDate = new DateTime(year, month, currentDay);
                            // Если в этот день записан лог, то..
                            if (logsByDay.ContainsKey(currentDate))
                            {
                                int logCount = logsByDay[currentDate];
                                // ..получаем день в зависимости от интенсивности..
                                var color = GetColorForDay(logCount);
                                // .. и добавляем день с цветом в таблицу.
                                week.Add($"[{color}]{currentDay,2}[/]"); 
                            }
                            else
                            {
                                // Если в этот день логов нет, добавляем день с выравниванием по правому краю в 2 символа.
                                week.Add($"{currentDay,2}");
                            }
                            currentDay++;
                        }
                    }
                    // Добавляем получившуюся строку календаря.
                    table.AddRow(string.Join(" ", week));
                }
                // Выводим наш "календарь".
                AnsiConsole.Write(table);
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
        private static Spectre.Console.Color GetColorForDay(int logCount)
        {
            if (logCount == 0) return Spectre.Console.Color.Default;
            return logCount switch
            {
                <= 3 => Spectre.Console.Color.Lime,
                <= 5 => Spectre.Console.Color.Green3,
                <= 10 => Spectre.Console.Color.Green,
                _ => Spectre.Console.Color.DarkGreen
            };
        }

        /// <summary>
        /// Метод, выводящий меню с выбором визуализации.
        /// </summary>
        public static void VisualizationMenu()
        {
            if (LogFilters._logs == null || LogFilters._logs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[invert]Выберите способ визуализации:[/]")
                        .AddChoices(["Таблицей", "Календарем", "Диаграммой", "Создать и экспортировать гистограмму", "[italic underline]Назад[/]"])
                        .HighlightStyle(Spectre.Console.Color.DodgerBlue1));
                switch (choice)
                {
                    case "Таблицей":
                        AnsiConsole.Clear();
                        TableVisualization();
                        break;
                    case "Календарем":
                        AnsiConsole.Clear();
                        CalendarVisualization();
                        break;
                    case "Диаграммой":
                        AnsiConsole.Clear();
                        BreakdownChartVisualization();
                        break;
                    case "Создать и экспортировать гистограмму":
                        AnsiConsole.Clear();
                        LogsPlotHistogram();
                        break;
                    case "[italic underline]Назад[/]":
                        AnsiConsole.Clear();
                        return;
                }
            }
        }

        /// <summary>
        /// Метод создания гистограммы.
        /// </summary>
        /// <param name="logs">Список логов, для которых формируется гистограмма.</param>
        public static void LogsPlotHistogram()
        {
            // Группировка логов по дням.
            var logsByDay = LogFilters._logs
                .GroupBy(log => log.Timestamp.Date) // Группируем по дате (без времени).
                .OrderBy(group => group.Key)   // Сортируем по дате.
                .ToList();
            double[] dates = logsByDay.Select(group => group.Key.ToOADate()).ToArray(); // Даты в формате OLE (дней с 30го Декабря 1899).
            double[] counts = logsByDay.Select(group => (double)group.Count()).ToArray(); // Количество записей.

            var plt = new Plot();
            // Добавляем данные в гистограмму.
            var barPlot = plt.Add.Bars(dates, counts);
            plt.Axes.DateTimeTicksBottom();
            // Минимальное значение по оси Y - 0.
            plt.Axes.Left.Min = 0;
            // Отметки по оси Y только целые числа без отметок между целыми.
            ScottPlot.TickGenerators.EvenlySpacedMinorTickGenerator minorTickGen = new(0);
            plt.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
            {
                MinorTickGenerator = minorTickGen,
                IntegerTicksOnly = true
            };
            plt.XLabel("Дата");
            plt.YLabel("Количество записей");
            plt.Title("Количество записей логов по дням");
            string outputPath = AnsiConsole.Ask<string>("[dodgerblue2]Введите путь куда сохранить изображение с графиком (без имени файла и расширения) или \"0\" для отмены: [/]");
            if (!Checker.isCorrectPath(outputPath) || outputPath == "0")
            {
                return;
            }
            AnsiConsole.Clear();
            string fileName = AnsiConsole.Ask<string>("[dodgerblue2]Введите имя для файла изображения (без .png) или \"0\" для отмены: [/]");
            if (!Checker.ValidateFileName(fileName) || fileName == "0")
            {
                return;
            }
            AnsiConsole.Clear();

            /*
             * Если путь введён в конце с символом, используемым
             * для разделения элементов в пути, то удаляем его (чтобы
             * путь был корректен и не состоял из двух подряд разделителей)
             * и передаем "склеенный" корректный путь для создания файла.
             */
            string filePath = $"{(outputPath.EndsWith(Path.DirectorySeparatorChar) ? outputPath.Remove(outputPath.Length - 1) : outputPath)}{Path.DirectorySeparatorChar}{fileName}.png";
            if (Checker.DoFileExist(filePath))
            {
                AnsiConsole.Clear();
                return;
            }
            else
            {
                try
                {
                    File.Delete(filePath);
                    plt.SavePng(filePath, 1000, 1000);
                }
                catch (UnauthorizedAccessException)
                {
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("[red]Доступ к директории запрещён.[/]");
                    return;
                }
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"[dodgerblue2]График сохранён в файл: {(outputPath.EndsWith(Path.DirectorySeparatorChar) ? outputPath.Remove(outputPath.Length - 1) : outputPath)}{Path.DirectorySeparatorChar}{fileName}.png[/]");
            }
        }
    }
}
