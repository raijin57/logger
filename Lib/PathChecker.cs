using Logs;
using Spectre.Console;
using System.IO;
namespace ServiceLibrary
{
    public static class PathChecker
    {
        /// <summary>
        /// Метод, проверяющий корректность .txt файла и отправляющий его на чтение.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException">Ошибка открытия файла (нет прав).</exception>
        /// <exception cref="FileNotFoundException">Файл по указанному пути не найден.</exception>
        public static async Task<List<Log>> isCorrectTxt(string path)
        {
            try
            {
                // Сперва проверяем корректность пути.
                if (isCorrectPath(path))
                {
                    try
                    {
                        // Пробуем открыть файл, чтобы проверить его доступность.
                        using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw new UnauthorizedAccessException("Недостаточно прав для доступа к файлу.");
                    }
                    // Отправляем файл на чтение, если путь оказался корректен (отправляем сразу отсюда, ибо зачем нам ещё проверка на корректность .txt, если не для этого метода?).
                    return await LogReader.Read(path);
                }
                else
                {
                    throw new FileNotFoundException("Путь к файлу некорректен.");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Clear();
                return new List<Log>();
            }
        }

        /// <summary>
        /// Метод, проверяющий корректность введённого пути.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">В пути есть некорректные символы.</exception>
        /// <exception cref="ArgumentException">Путь оказался пустым.</exception>
        public static bool isCorrectPath(string path)
        {
            try
            {
                if ((path == null) || (path.IndexOfAny(Path.GetInvalidPathChars()) != -1))
                {
                    throw new FormatException("В пути присутствуют некорректные символы.");
                }
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Путь к файлу не может быть пустым.");
                }
                try
                {
                    // Если по этому пути создаётся FileInfo <=> путь корректен.
                    var tempFileInfo = new FileInfo(path);
                    return true;
                }
                catch (NotSupportedException)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return false;
            }
        }

        /// <summary>
        /// Проверка имени файла на корректность.
        /// </summary>
        /// <param name="name">Проверяемое имя</param>        
        public static bool ValidateFileName(string name)
        {
            try
            {
                // Логика такая: если мы смогли создать такой файл, значит его имя корректно.
                FileStream file = File.Open(name, FileMode.Open);
                if (file != null) file.Close();
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                // Такого файла нет, но если поиск пошел, значит имя было корректно.
                return true;
            }
            catch (IOException)
            {
                // То же, но не удалось открыть => имя корректно.
                return true;
            }
            return true;
        }
    }
}
