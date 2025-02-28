using ServiceLibrary;
using Logs;
using Spectre.Console;

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
                    .Title("[italic slowblink red]Анализатор логов[/]")
                    .AddChoices(["Загрузить данные", "Экспортировать данные", "Фильтрация данных", "Расширенная статистика", "Визуализация", "Выход"]));
            switch (choice)
            {
                case "Загрузить данные":
                    AnsiConsole.Clear();
                    await LoadData(); 
                    break;
                case "Экспортировать данные":
                    AnsiConsole.Clear();
                    TXTWriter.Write();
                    break;
                case "Фильтрация данных":
                    AnsiConsole.Clear();
                    LogFilters.FilterMenu();
                    break;
                case "Расширенная статистика":
                    AnsiConsole.Clear();
                    Statistics.Menu();
                    break;
                case "Визуализация":
                    AnsiConsole.Clear();
                    Visualization.VisualizationMenu();
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
            AnsiConsole.MarkupLine("[dodgerblue3]Данные успешно загружены.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Не удалось загрузить данные.[/]");
        }
    }
}