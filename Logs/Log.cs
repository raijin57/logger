namespace Logs
{
    /// <summary>
    /// Структура, представляюшая один лог.
    /// </summary>
    public struct Log
    {
        DateTime DateTime { get; set; }
        string level;
        string message;

        public Log(DateTime datetime, string importanceLevel, string message)
        {
            DateTime = datetime;
            level = importanceLevel;
            this.message = message;
        }
    }
}
