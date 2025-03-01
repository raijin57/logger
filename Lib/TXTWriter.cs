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
            if (LogFilters._logs == null)
            {
                AnsiConsole.MarkupLine("[red]Сперва введите данные в программу.[/]");
                return;
            }
            string outputPath = AnsiConsole.Ask<string>("Введите путь сохранения файла с логами (без имени файла и расширения): ");
            if (!PathChecker.isCorrectPath(outputPath)) return;
            string fileName = AnsiConsole.Ask<string>("Введите имя файла (без расширения): ");
            if (!PathChecker.ValidateFileName(fileName)) return;
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
                AnsiConsole.MarkupLine($"[green]Логи были сохранены в {(outputPath.EndsWith(Path.DirectorySeparatorChar) ? outputPath.Remove(outputPath.Length - 1) : outputPath)}{Path.DirectorySeparatorChar}{fileName}.txt[/]");
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]Путь к файлу оказался неправильным.[/]");
                return;
            }
        }
    }
}
