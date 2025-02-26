using Library;
using Logs;
using Spectre.Console;
using System.Text.Json;

public class MenuHandler
{
    private List<Log> _logs = new List<Log>();

    // Главное меню
    public async Task RunMenu()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Главное меню:")
                    .AddChoices(new[] { "Загрузить данные", "Фильтрация данных", "Визуализация данных", "Сохранение данных", "Выход" }));

            switch (choice)
            {
                case "Загрузить данные":
                    await LoadData();
                    break;
                case "Фильтрация данных":
                    await FilterMenu();
                    break;
                case "Визуализация данных":
                    await VisualizeData();
                    break;
                case "Сохранение данных":
                    await SaveData();
                    break;
                case "Выход":
                    return;
            }
        }
    }
    
    // Загрузка данных
    private async Task LoadData()
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

    // Меню фильтрации
    private async Task FilterMenu()
    {
        var filters = new List<Func<Log, bool>>();

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Меню фильтрации:")
                    .AddChoices(new[] { "Добавить фильтр", "Удалить фильтр", "Назад" }));

            switch (choice)
            {
                case "Добавить фильтр":
                    await AddFilter(filters);
                    break;
                case "Удалить фильтр":
                    RemoveFilter(filters);
                    break;
                case "Назад":
                    return;
            }
        }
    }

    // Добавление фильтра
    private async Task AddFilter(List<Func<Log, bool>> filters)
    {
        var filterType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите тип фильтра:")
                .AddChoices(new[] { "По дате", "По уровню важности", "По ключевому слову" }));

        switch (filterType)
        {
            case "По дате":
                var dateFilter = LogFilters.GetDateFilter();
                if (dateFilter != null)
                {
                    filters.Add(dateFilter);
                    AnsiConsole.MarkupLine("[green]Фильтр по дате добавлен.[/]");
                }
                break;
            case "По уровню важности":
                var level = AnsiConsole.Ask<string>("Введите уровень важности (INFO, WARNING, ERROR):");
                filters.Add(LogFilters.FilterByLevel(level));
                AnsiConsole.MarkupLine("[green]Фильтр по уровню важности добавлен.[/]");
                break;
            case "По ключевому слову":
                var keyword = AnsiConsole.Ask<string>("Введите ключевое слово:");
                filters.Add(LogFilters.FilterByMessage(keyword));
                AnsiConsole.MarkupLine("[green]Фильтр по ключевому слову добавлен.[/]");
                break;
        }
    }

    // Удаление фильтра
    private void RemoveFilter(List<Func<Log, bool>> filters)
    {
        if (filters.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Нет фильтров для удаления.[/]");
            return;
        }

        var filterDescriptions = new List<string>();
        foreach (var filter in filters)
        {
            filterDescriptions.Add(LogFilters.GetFilterDescription(filter));
        }

        var filterToRemove = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите фильтр для удаления:")
                .AddChoices(filterDescriptions));

        var index = filterDescriptions.IndexOf(filterToRemove);
        filters.RemoveAt(index);
        AnsiConsole.MarkupLine("[green]Фильтр удалён.[/]");
    }

    // Визуализация данных
    private async Task VisualizeData()
    {
        var logs = await LogReader.GetLogs();
        if (logs == null || logs.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Нет данных для визуализации.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Дата");
        table.AddColumn("Уровень");
        table.AddColumn("Сообщение");

        foreach (var log in logs)
        {
            table.AddRow(log.dateTime.ToString(), log.level, log.message);
        }

        AnsiConsole.Write(table);
    }

    // Сохранение данных
    private async Task SaveData()
    {
        var logs = await LogReader.GetLogs();
        if (logs == null || logs.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Нет данных для сохранения.[/]");
            return;
        }

        var fileName = AnsiConsole.Ask<string>("Введите имя файла для сохранения (без расширения):");
        var filePath = $"{fileName}.json";

        var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);

        AnsiConsole.MarkupLine($"[green]Данные сохранены в файл {filePath}.[/]");
    }
}