﻿using ServiceLibrary;
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
                    .Title("[bold invert]Анализатор логов[/]")
                    .AddChoices(["Загрузить данные", "Экспортировать данные", "Фильтрация данных", "Расширенная статистика", "Визуализация", "[italic underline]Выход из программы[/]"])
                    .HighlightStyle(Color.DodgerBlue1));
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
                    Statistics.StatisticsMenu();
                    break;
                case "Визуализация":
                    AnsiConsole.Clear();
                    Visualization.VisualizationMenu();
                    break;
                case "[italic underline]Выход из программы[/]":
                    AnsiConsole.Clear();
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
        var path = AnsiConsole.Ask<string>("[dodgerblue2]Введите путь к файлу или \"0\" для отмены: [/]");
        AnsiConsole.Clear();
        if (path == "0") return;
        /*
         * Этот метод находится в этой библиотеке, а не в Logs ибо нужен класс PathChecker 
         * (Зависимости Logs от ServiceLibrary быть не может, так как ServiceLibrary нужно знать о
         * структуре Log и мы не можем сделать петлю в зависимостях).
         */
        try
        {
            LogFilters._logs = await Checker.isCorrectTxt(path);
        }
        catch
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[red]Путь некорректен.[/]");
            return;
        }
        if (LogFilters._logs.Count > 0)
        {
            AnsiConsole.MarkupLine("[dodgerblue2]Данные успешно загружены.[/]");
            // Останавливаем сервер, если он был запущен до этого (например, если загрузили новый файл, не завершая программу).
            HTTPServer.Stop();
            // Запуск HTTP-сервера после прочтения файла.
            HTTPServer.Start();
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Не удалось загрузить данные.[/]");
        }
    }
}