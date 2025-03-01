using System;
using Spectre.Console;
using System.Net;
using System.Text;
using Logs;
using Newtonsoft.Json;

namespace ServiceLibrary
{
    /// <summary>
    /// Класс, содержащий всю логику http сервера (REST API).
    /// </summary>
    public static class SimpleHttpServer
    {
        // Создаём экземпляр HttpListener, который будет принимать входящие http запросы.
        private static HttpListener _listener;
        // Указываем url, по которому будем "слушать" запросы.
        // Измените хост, если 5214 у вас занят.
        private static readonly string _url = "http://localhost:5214/";

        /// <summary>
        /// Метод, запускающий REST API.
        /// </summary>
        public static void Start()
        {
            _listener = new HttpListener();
            // Добавляем префикс, который будем слушать.
            _listener.Prefixes.Add(_url);
            // Начинаем отлов запросов.
            _listener.Start();
            AnsiConsole.MarkupLine($"[dim]API сервер запущен на {_url}\n[/]");

            Task.Run(HandleRequests);
        }

        /// <summary>
        /// Метод, для остановки сервера.
        /// </summary>
        public static void Stop()
        {
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
                _listener = null;
                AnsiConsole.MarkupLine("[dim]Предыдущий сервер остановлен.[/]");
            }
        }

        /// <summary>
        /// Метод, ожидающий запросов по указанному адресу.
        /// </summary>
        /// <returns></returns>
        private static async Task HandleRequests()
        {
            while (_listener.IsListening)
            {
                // Ждём ввод запроса.
                var context = await _listener.GetContextAsync();
                // Начинаем выполнение.
                ProcessRequest(context);
            }
        }

        /// <summary>
        /// Метод, выполняющий полученный запрос.
        /// </summary>
        /// <param name="context">Параметры запрооса</param>
        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                // Объекты запроса и ответа из контекста.
                var request = context.Request;
                var response = context.Response;
                AnsiConsole.Clear();
                // Происходит какой-то баг с дублированием пунктов меню, кажется буфер не успевает, поэтому ещё раз пришлось очистить.
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"[dim]Получен запрос: {request.HttpMethod} {request.Url}\n[/]");

                // Обработка GET /logs.
                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/logs")
                {
                    var from = request.QueryString["from"];
                    var to = request.QueryString["to"];

                    // Проверяем, указаны ли параметры from и to.
                    if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out _) && !string.IsNullOrEmpty(to) && DateTime.TryParse(to, out _))
                    {
                        // Применяем фильтр по дате
                        var filteredLogs = FilterLogs(from, to);

                        // Формируем ответ в JSON.
                        var jsonResponse = JsonConvert.SerializeObject(filteredLogs);
                        response.ContentType = "application/json";
                        // Возвращаем статус что всё хорошо.
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.OutputStream.Write(Encoding.UTF8.GetBytes(jsonResponse));
                    }
                    else
                    {
                        // Если параметры from или to не указаны, возвращаем все логи.
                        var jsonResponse = JsonConvert.SerializeObject(LogFilters._logs);
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.OutputStream.Write(Encoding.UTF8.GetBytes(jsonResponse));
                    }
                }
                else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/logs")
                {
                    // Обработка POST /logs.
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        var json = reader.ReadToEnd();
                        // Десериализуем полученные данные в запросе.
                        var log = JsonConvert.DeserializeObject<Log>(json);
                        // Добавляем лог, полученный из тела POST запроса.
                        LogFilters._logs.Add(log);
                        response.StatusCode = (int)HttpStatusCode.Created;
                    }
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/logs/statistics")
                {
                    // Обработка GET /logs/statistics.
                    var from = request.QueryString["from"];
                    var to = request.QueryString["to"];

                    // Фильтруем логи по дате.
                    List<Log> filteredLogs = FilterLogs(from, to);
                    // Получаем статистику.
                    object statistics = Statistics.GETStatistics(from, to, filteredLogs);
                    // Формируем ответ в JSON.
                    var jsonResponse = JsonConvert.SerializeObject(statistics);
                    response.ContentType = "application/json";
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.OutputStream.Write(Encoding.UTF8.GetBytes(jsonResponse));
                }
                else
                {
                    // Если запрос некорректен, возвращаем код некорректного запроса.
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                // Отправляем сформировавшийся ответ.
                response.Close();
            }
            catch (Exception ex)
            {
                // Если произошла ошибка на стороне сервера.
                AnsiConsole.MarkupLine($"[dim red]Ошибка: {ex.Message}[/]");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
        }

        /// <summary>
        /// Метод, для фильтрации логов по дате из запроса.
        /// </summary>
        /// <param name="from">Начальная дата диапазона.</param>
        /// <param name="to">Конечная дата диапазона.</param>
        /// <returns>Список с отфильтрованными логами.</returns>
        private static List<Log> FilterLogs(string from, string to)
        {
            if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out var fromDate) && !string.IsNullOrEmpty(to) && DateTime.TryParse(to, out var toDate))
            {
                // Применяем фильтр по дате, если передали параметры.
                return LogFilters._logs.Where(LogFilters.FilterByDate(fromDate, toDate)).ToList();
            }
            // Если параметры не указаны, то возвращаем все логи.
            return LogFilters._logs.ToList();
        }
    }
}