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
                //Пример, как получить данные.
                List<Log> a = await PathChecker.isCorrectPath(@"C:\Users\Arsen\source\repos\LogAnalyzerasProject3.2\logs.txt");
                foreach (Log log in a)
                {
                    Console.WriteLine(log);
                }
                break;
            }
        }
    }
}