using System;
using Lib;
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
                string userChoice = Console.ReadLine();
                List<Log> a;
                switch (userChoice)
                {
                    case "1":
                        
                        break;
                    case "2":
                        a = await LogReader.GetLogs();
                        Console.WriteLine("!!!!");
                        LogFilters.PrintLogs(a);
                        LogFilters.SelectFilters();
                        LogFilters.ApplyFilters(a);
                        LogFilters.PrintLogs(a);
                        break;
                    case "3":
                        break;
                    case "10":
                        break;
                }
            }
        }
    }
}