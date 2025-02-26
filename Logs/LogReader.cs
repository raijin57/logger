using System.Globalization;
using System.Runtime.CompilerServices;

namespace Logs
{
    public static class LogReader
    {
        /// <summary>
        /// Статическая переменная, в которой лежат данные из прочитанного файла.
        /// </summary>
        static List<Log> _logsRead;
        /// <summary>
        /// Асинхронный метод, читающий список логов по указанному файлу.
        /// </summary>
        /// <param name="path">Путь к файлу в строковом виде.</param>
        /// <returns></returns>
        public static async Task<List<Log>> Read(string path)
        {
            // Счётчик количества пропущенных строк ввиду ошибок.
            int skippedCounter = 0;
            // Список структур - логов.
            List<Log> logs = new List<Log>();
            try
            { // Читаем файл.
                using (StreamReader reader = new StreamReader(path))
                {
                    string? line;
                    // Пока файл не закончится.
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        // Удаляем лишние символы и выделяем интересующие фрагменты.
                        string[] splitted = line.Replace("[", "").Replace("] ", "_").Split("_");
                        // Проверяем, что три поля (ожидаем "дата", "важность", "сообщение").
                        if (splitted.Length == 3)
                        {
                            try
                            {
                                // Парсим, согласно формату.
                                DateTime formattedDateTime = DateTime.ParseExact(splitted[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                // Формируем объект и добавляем в итоговый список.
                                Log newLog = new Log(formattedDateTime, splitted[1], splitted[2]);
                                logs.Add(newLog);
                            }
                            catch (FormatException)
                            { // может считать количество пропущенных и причину?..
                                skippedCounter++;
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при чтении файла. {ex.ToString()}");
            }
            Console.WriteLine($"Некорректных строк (ошибка при форматировании), которые были пропущены: {skippedCounter}\n");
            _logsRead = logs;
            return logs;
        }

        /// <summary>
        /// Метод, для получения прочитанных данных.
        /// </summary>
        /// <returns>Список с отформатированными данными.</returns>
        public static async Task<List<Log>> GetLogs()
        {
            if (_logsRead == null)
            {
                Console.WriteLine("Сперва загрузите данные в программу");
                return null;
            }
            return _logsRead;
        }
    }
}
