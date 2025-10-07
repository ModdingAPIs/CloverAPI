namespace CloverAPI;

public static class Logging
{
    public static ManualLogSource Logger => Plugin.Log;

    public static void Log(LogLevel level, object message)
    {
        Logger.Log(level, message);
    }

    public static void LogInfo(object message)
    {
        Logger.LogInfo(message);
    }

    public static void LogDebug(object message)
    {
        Logger.LogDebug(message);
    }

    public static void LogWarning(object message)
    {
        Logger.LogWarning(message);
    }

    public static void LogError(object message)
    {
        Logger.LogError(message);
    }

    public static void LogFatal(object message)
    {
        Logger.LogFatal(message);
    }
}