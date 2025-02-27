using Library;
using Logs;
using Spectre.Console;
using System.Text.Json;

public static class MenuHandler
{
    /// <summary>
    /// Метод, запускающий и выводящий на экран главное меню, реализованное через Spectre.Console
    /// </summary>
    /// <returns>Меню.</returns>
    public static async Task RunMenu()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[italic]Добро пожаловать![/]")
                    .AddChoices(new[] { "Загрузить данные", "Фильтрация данных", "Вывести логи", "Выход" }));
            switch (choice)
            {
                case "Загрузить данные":
                    AnsiConsole.Clear();
                    await LoadData();
                    break;
                case "Фильтрация данных":
                    AnsiConsole.Clear();
                    await LogFilters.FilterMenu();
                    break;
                case "Вывести логи":
                    PrintLogs(LogFilters._logs);
                    break;
                case "Выход":
                    return;
            }
        }
    }
    
    /// <summary>
    /// Метод, загружающий данные из файла.
    /// </summary>
    /// <returns>Список с прочитанными логами.</returns>
    private static async Task LoadData()
    {
        var path = AnsiConsole.Ask<string>("Введите путь к файлу или \"0\" для отмены:");
        AnsiConsole.Clear();
        if (path == "0") return;
        LogFilters._logs = await PathChecker.isCorrectTxt(path);
        if (LogFilters._logs.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Данные успешно загружены.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Не удалось загрузить данные.[/]");
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
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
            return;
        }
        else
        {
            AnsiConsole.Clear();
            foreach (var log in logs)
            {
                AnsiConsole.WriteLine(log.ToString());
            }
        }
    }
}