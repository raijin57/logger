using Logs;
namespace Lib
{
    public static class PathChecker
    {
        public static async Task<List<Log>> isCorrectPath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Путь к файлу не может быть пустым или null.");
                }
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Файл {path} не найден.");
                }
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
                // Отправляем файл на чтение, если путь оказался корректен (отправляем сразу отсюда, ибо зачем нам ещё проверка на корректность пути, если не для этого метода?).
                return await LogReader.Read(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<Log>();
            }
        }
    }
}
