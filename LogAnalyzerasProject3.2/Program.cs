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
            await MenuHandler.RunMenu();
        }
    }
}