using Library;
using Logs;
using Spectre.Console;

namespace Lib
{
    class Program
    {
        /// <summary>
        /// Основной метод программы. Реализован как асинхронный, для корректной работы методов.
        /// </summary>
        /// <returns></returns>
        static async Task Main()
        {
            while (true)
            {
                Console.WriteLine("===Добро пожаловать в анализатор логов.===\n1. Загрузить данные\n2. Добавить фильтр\n3. Изменить порядок фильтров\n4. Применить фильтры\n5. Вывести список логов\n10. Выйти из программы");
                string userChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Главное меню:")
                    .AddChoices(new[] { "Загрузить данные", "Фильтрация данных", "Визуализация данных", "Сохранение данных", "Выход" }));
                switch (userChoice)
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
    }
}