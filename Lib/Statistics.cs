using Logs;
using ScottPlot.Colormaps;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    public static class Statistics
    {
        /// <summary>
        /// Метод, считающий количество ошибок за выбранный диапазон.
        /// </summary>
        public static void ErrorCount()
        {
            int counter = 0;
            DateTime startDate;
            DateTime endDate;
            // Ввод начальной даты.
            while (true)
            {
                AnsiConsole.MarkupLine("[dodgerblue2]Введите начальную дату (гггг-мм-дд чч:мм:сс) или \"0\" для отмены: [/]");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                {
                    AnsiConsole.Clear();
                    return;
                }
                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate)) break;
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[red]Некорректная дата. Попробуйте снова.[/]");
            }

            // Ввод конечной даты
            while (true)
            {
                AnsiConsole.MarkupLine("[dodgerblue2]Введите конечную дату (гггг-мм-дд чч:мм:сс) или \"0\" для отмены: [/]");
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
            foreach (Log log in LogFilters._logs)
            {
                // Считаем ошибки в нужном диапазоне.
                if (log.Timestamp >= startDate && log.Timestamp <= endDate && log.ImportanceLevel.ToUpper() == "ERROR") counter++;
            }
            AnsiConsole.MarkupLine($"[yellow]Количество ошибок (ERROR) за указанный временной диапазон: {counter}[/]");
            AnsiConsole.MarkupLine("[dim grey]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }

        /// <summary>
        /// Метод выбора аномалий через меню.
        /// </summary>
        public static void AnomaliesFinder()
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[invert]Меню поиска аномалий:[/]")
                        .AddChoices(["Одновременные логи", "Повторяющиеся сообщения", "Долгое отсутствие логов", "[italic underline]Назад[/]"])
                        .HighlightStyle(Color.DodgerBlue1));
                switch (choice)
                {
                    case "Одновременные логи":
                        AnsiConsole.Clear();
                        SameSecondAnomaly();
                        break;
                    case "Повторяющиеся сообщения":
                        AnsiConsole.Clear();
                        SameMessageAnomaly();
                        break;
                    case "Долгое отсутствие логов":
                        AnsiConsole.Clear();
                        LongTimeGapAnomaly();
                        break;
                    case "[italic underline]Назад[/]":
                        AnsiConsole.Clear();
                        return;
                }
            }
        }

        public static void SameSecondAnomaly()
        {
            // Ищем, написано ли несколько логов в одну и ту же секунду.
            var logsPerSecond = LogFilters._logs
            .GroupBy(log => log.Timestamp)
            .Where(group => group.Count() > 1);

            foreach (var group in logsPerSecond)
            {
                AnsiConsole.MarkupLine($"[yellow]Несколько логов за {group.Key}. Количество: {group.Count()}.[/]");
            }
            AnsiConsole.MarkupLine("[dim grey]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }

        public static void SameMessageAnomaly()
        {
            // Ищем, повторялись ли сообщения в логах.
            var repeatedMessages = LogFilters._logs
            .GroupBy(log => log.Message)
            .Where(group => group.Count() > 1);

            foreach (var group in repeatedMessages)
            {
                AnsiConsole.MarkupLine($"[yellow]Повторяющееся сообщение: \"{group.Key}\". Количество повторений: {group.Count()}.[/]");
            }
            AnsiConsole.MarkupLine("[dim grey]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }

        public static void LongTimeGapAnomaly()
        {
            // Ищем, давно ли у нас нет логов.
            var sortedLogs = LogFilters._logs.OrderBy(log => log.Timestamp).ToList();
            for (int i = 1; i < sortedLogs.Count; i++)
            {
                var timeGap = sortedLogs[i].Timestamp - sortedLogs[i - 1].Timestamp;
                // Порог: более 52 часов между логами.
                if (timeGap.TotalMinutes >= 52)
                {
                    AnsiConsole.MarkupLine($"[yellow]Большой промежуток времени между логами. Длительность: {(int)timeGap.TotalMinutes} минут.[/]");
                }
            }
            AnsiConsole.MarkupLine("[dim grey]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }

        /// <summary>
        /// Метод подсчета самых часто встречающихся слов.
        /// </summary>
        public static void WordsCount()
        {
            string n = AnsiConsole.Ask<string>("[dodgerblue2]Введите число N или \"0\" для отмены: [/]");
            if (n == "0")
            {
                AnsiConsole.Clear();
                return;
            }
            if (!int.TryParse(n, out int N) || N < 0)
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[red]Введите корректное число[/]");
                return;
            }
            // Объединяем все сообщения в одну строку, очищая от знаков препинания.
            string allMessages = Regex.Replace(string.Join(" ", LogFilters._logs.Select(log => log.Message)), @"[^\w\s]", "");
            // Убираем служебные слова.
            var stopWords = new HashSet<string> { "в", "к", "нa", "из", "от", "за" };
            // Разделяем текст на слова и приводим их к нижнему регистру.
            var words = allMessages.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToLower()).Where(word => !stopWords.Contains(word));
            var wordFrequency = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (wordFrequency.ContainsKey(word))
                {
                    wordFrequency[word]++;
                }
                else
                {
                    wordFrequency[word] = 1;
                }
            }

            // Сортируем слова по убыванию частоты и выбираем N первых слов.
            var mostFrequentWords = wordFrequency
                .OrderByDescending(pair => pair.Value)
                .Take(N)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            AnsiConsole.Clear();
            foreach (var word in mostFrequentWords)
            {
                AnsiConsole.MarkupLine($"[yellow]{word.Key}: {word.Value}[/]");
            }
            AnsiConsole.MarkupLine("[dim grey]Нажмите Enter для выхода.[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }

        /// <summary>
        /// Метод, вызывающий меню статистики.
        /// </summary>
        public static void StatisticsMenu()
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
                        .Title("[invert]Расширенная статистика:[/]")
                        .AddChoices(["Количество ошибок за промежуток времени", "Поиск аномалий", "N самых часто встречающихся слов", "[italic underline]Назад[/]"])
                        .HighlightStyle(Color.DodgerBlue1));
                switch (choice)
                {
                    case "Количество ошибок за промежуток времени":
                        AnsiConsole.Clear();
                        ErrorCount();
                        break;
                    case "Поиск аномалий":
                        AnsiConsole.Clear();
                        AnomaliesFinder();
                        break;
                    case "N самых часто встречающихся слов":
                        AnsiConsole.Clear();
                        WordsCount();
                        break;
                    case "[italic underline]Назад[/]":
                        AnsiConsole.Clear();
                        return;
                }
            }
        }
        /// <summary>
        /// Метод, для получения статистики по логам, нужно для GET запроса /logs/statistics.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static object GETStatistics(string from, string to, List<Log> filteredLogs)
        {
            // Получаем множество всех уровней важности в файле.
            var uniqueLevels = filteredLogs
                .Select(log => log.ImportanceLevel) 
                .Distinct() // .Distinct - убирает дубликаты.
                .ToList();
            var logLevelsCount = new Dictionary<string, int>();
            // Группируем логи по уровням важности и подсчитываем количество.
            foreach (var level in uniqueLevels)
            {
                var count = filteredLogs.Count(LogFilters.FilterByLevel(level));
                logLevelsCount[level] = count;
            }
            // Сразу выведем оба пункта нужной статистики: и общее количество лог-сообщений и словарь с уровнями важности и их количеством.
            return new
            {
                totalMessages = filteredLogs.Count,
                logLevelsCount
            };
        }
    }
}
