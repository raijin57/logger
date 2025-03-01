using Logs;
using ScottPlot;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    class TXTWriter
    {
        /// <summary>
        /// Метод экспорта данных в .txt.
        /// </summary>
        public static void Write()
        {
            if (LogFilters._logs == null || LogFilters._logs.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            string outputPath = AnsiConsole.Ask<string>("[dodgerblue2]Введите путь сохранения файла с логами (без имени файла и расширения) или \"0\" для отмены: [/]");
            if (!Checker.isCorrectPath(outputPath) || outputPath == "0")
            {
                return;
            }
            string fileName = AnsiConsole.Ask<string>("[dodgerblue2]Введите имя файла (без расширения): [/]");
            if (!Checker.ValidateFileName(fileName))
            {
                return;
            }
            try
            {
                /*
                * Если путь введён в конце с символом, используемым
                * для разделения элементов в пути, то удаляем его (чтобы
                * путь был корректен и не состоял из двух подряд разделителей)
                * и передаем "склеенный" корректный путь для создания файла.
                */

                // Аргумент false означает что будем перезаписывать уже существующий файл.
                using (StreamWriter writer = new StreamWriter($"{(outputPath.EndsWith(Path.DirectorySeparatorChar) ? outputPath.Remove(outputPath.Length - 1) : outputPath)}{Path.DirectorySeparatorChar}{fileName}.txt", false))
                {
                    foreach (Log line in LogFilters._logs)
                    {
                        writer.WriteLine(line); 
                    }
                }
                AnsiConsole.MarkupLine($"[dodgerblue2]Логи были сохранены в {(outputPath.EndsWith(Path.DirectorySeparatorChar) ? outputPath.Remove(outputPath.Length - 1) : outputPath)}{Path.DirectorySeparatorChar}{fileName}.txt[/]");
            }
            catch
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[red]Путь к файлу оказался неправильным.[/]");
                return;
            }
        }
    }
}
