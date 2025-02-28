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
                AnsiConsole.MarkupLine("Введите начальную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                {
                    return;
                }

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                {
                    break;
                }
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("Некорректная дата. Попробуйте снова.");
            }

            // Ввод конечной даты
            while (true)
            {
                AnsiConsole.MarkupLine("Введите конечную дату (гггг-мм-дд чч:мм:сс) или \"0\" для выхода: ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "0")
                {
                    return;
                }

                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                {
                    break;
                }
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("Некорректная дата. Попробуйте снова.");
            }
            foreach (Log log in LogFilters._logs)
            {
                if (log.dateTime >= startDate && log.dateTime <= endDate && log.level.ToUpper() == "ERROR") counter++;
            }
            AnsiConsole.MarkupLine($"[yellow]Количество ошибок (ERROR) за указанный временной диапазон: {counter}[/]");
        }

        /// <summary>
        /// Метод поиска аномалий в логах.
        /// </summary>
        public static void AnomaliesFinder()
        {
            // Ищем, написано ли несколько логов в одну и ту же секунду.
            var logsPerSecond = LogFilters._logs
            .GroupBy(log => log.dateTime) 
            .Where(group => group.Count() > 1);
            
            foreach (var group in logsPerSecond)
            {
                AnsiConsole.WriteLine($"[yellow]Аномалия: слишком много логов в секунду {group.Key}. Количество: {group.Count()}.[/]");
            }

            // Ищем, повторялись ли сообщения в логах.
            var repeatedMessages = LogFilters._logs
            .GroupBy(log => log.message)
            .Where(group => group.Count() > 1);

            foreach (var group in repeatedMessages)
            {
                AnsiConsole.WriteLine($"[yellow]Аномалия: повторяющееся сообщение '{group.Key}'. Количество: {group.Count()}.[/]");
            }

            // Ищем, давно ли у нас нет логов.
            var sortedLogs = LogFilters._logs.OrderBy(log => log.dateTime).ToList();
            for (int i = 1; i < sortedLogs.Count; i++)
            {
                var timeGap = sortedLogs[i].dateTime - sortedLogs[i - 1].dateTime;
                if (timeGap.TotalMinutes >= 52) // Порог: более 1 минуты между логами
                {
                    AnsiConsole.WriteLine($"[yellow]Аномалия: большой промежуток времени между логами. Длительность: {timeGap.TotalMinutes} минут.[/]");
                }
            }
        }

        public static void WordsCounter()
        {
            string n = AnsiConsole.Ask<string>("Введите число N: ");
            if (!int.TryParse(n, out int N))
            {
                AnsiConsole.MarkupLine("[red]Введите корректное число[/]");
                return;
            }
            // Объединяем все сообщения в одну строку, очищая от знаков препинания.
            string allMessages = Regex.Replace(string.Join(" ", LogFilters._logs.Select(log => log.message)), @"[^\w\s]", "");
            var stopWords = new HashSet<string> { "в", "к", "нa", "из", "от" };
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

            // Сортируем слова по убыванию частоты и выбираем 10.
            var mostFrequentWords = wordFrequency
                .OrderByDescending(pair => pair.Value)
                .Take(N)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            AnsiConsole.Clear();
            foreach (var word in mostFrequentWords)
            {
                AnsiConsole.MarkupLine($"[yellow]{word.Key}: {word.Value}[/]");
            }
        }

        /// <summary>
        /// Метод, вызывающий меню статистики.
        /// </summary>
        public static void Menu()
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Расширенная статистика:")
                        .AddChoices(["Количество ошибок за промежуток времени", "Поиск аномалий", "N самых часто встречающихся слов", "Выход"]));
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
                        WordsCounter();
                        break;
                    case "Выход":
                        AnsiConsole.Clear();
                        return;
                }
            }
        }
    }
}
