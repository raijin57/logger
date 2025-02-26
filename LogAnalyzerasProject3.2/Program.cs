using Library;
using Logs;

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
                string userChoice = Console.ReadLine();
                switch (userChoice)
                {
                    case "1":
                        Console.WriteLine("Введите путь к файлу:");
                        string? path = Console.ReadLine();
                        Console.Clear();
                        await PathChecker.isCorrectTxt(path);
                        break;

                    case "2":
                        LogFilters.SelectFilters();
                        break;

                    case "3":
                        LogFilters.ReorderFilters();
                        break;

                    /* Использование await LogReader.GetLogs() - вынужденная мера из-за проблем с видимостью переменных внутри switch.
                     * 
                     *  Почему-то..
                     * 
                     * case "1":
                     *     ...
                     *     List<Log> logs = await LogReader.GetLogs()
                     *     break;
                     * 
                     * case "2":
                     *     logs = ... <- жалуется, что logs не существует
                     *     break;
                     * 
                     * ..но
                     *
                     * case "1":
                     *     ...
                     *     List<Log> logs = await LogReader.GetLogs()
                     *     break;
                     * 
                     * case "2":
                     *     List<Log> logs = await LogReader.GetLogs() <- жалуется на переопределение уже существующей переменной logs.
                     *     ...
                     *     break;
                     * 
                     * Sorry!
                     */

                    case "4":
                        LogFilters.ApplyFilters(await LogReader.GetLogs());
                        break;

                    case "5":
                        LogFilters.PrintLogs(await LogReader.GetLogs());
                        break;

                    case "10":
                        Console.WriteLine("Программа завершила свою работу.");
                        return;
                }
            }
        }
    }
}