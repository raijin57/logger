namespace Lib
{
    public static class PathChecker
    {
        public static void isCorrectPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Путь к файлу не может быть пустым или null.", nameof(path));
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Файл не найден.", path);
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
            LogReader.Read(path);
        }
    }
}
