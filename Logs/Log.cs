namespace Logs
{
    /// <summary>
    /// Структура, представляющая один лог.
    /// </summary>
    public struct Log
    {
        public DateTime Timestamp;
        public string ImportanceLevel;
        public string Message;

        public Log(DateTime datetime, string importanceLevel, string message)
        {
            Timestamp = datetime;
            ImportanceLevel = importanceLevel;
            Message = message;
        }
        public override string ToString()
        {
            return $"[{Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}] [{ImportanceLevel}] {Message}";
        }
    }
}
