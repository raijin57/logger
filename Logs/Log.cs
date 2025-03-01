namespace Logs
{
    /// <summary>
    /// Структура, представляюшая один лог.
    /// </summary>
    public struct Log
    {
        public DateTime Timestamp { get; set; }
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
            return $"[{Timestamp}] [{ImportanceLevel}] [{Message}]";
        }
    }
}
