namespace Logs
{
    /// <summary>
    /// Структура, представляюшая один лог.
    /// </summary>
    public struct Log
    {
        public DateTime dateTime { get; set; }
        public string level;
        public string message;

        public Log(DateTime datetime, string importanceLevel, string message)
        {
            dateTime = datetime;
            level = importanceLevel;
            this.message = message;
        }
        public override string ToString()
        {
            return $"[{dateTime}] [{level}] [{message}]";
        }
    }
}
