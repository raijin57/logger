using Logs;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Lib
{
    public static class LogReader
    {
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
                                DateTime formattedDateTime = DateTime.ParseExact(splitted[0], "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
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
            return logs;
        }
    }
}
