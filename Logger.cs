// Wrapper function for consistent timestamp logging
public static class Logger
{
    public const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";

    public static void Log(string message, string prefix = "")
    {
        var timestamp = DateTime.Now.ToString(TIMESTAMP_FORMAT);
        Console.WriteLine($"[{timestamp}] {prefix}{message}");
    }
}

