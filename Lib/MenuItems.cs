using Library;
using Logs;
using Spectre.Console;
using System.Text.Json;

public static class MenuHandler
{
    private static List<Log> _logs = new List<Log>();

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
                    .Title("=== Добро пожаловать! ===")
                    .AddChoices(new[] { "Загрузить данные", "Фильтрация данных", "Выход" }));

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
        var path = AnsiConsole.Ask<string>("Введите путь к файлу:");
        _logs = await PathChecker.isCorrectTxt(path);

        if (_logs.Count > 0)
        {
            AnsiConsole.MarkupLine("[green]Данные успешно загружены.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Не удалось загрузить данные.[/]");
        }
    }
}