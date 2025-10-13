using System;

namespace CloverAPI.Utils;

public class GameUtils
{
    public static bool GameReady { get; internal set; } = false;
    public static event Action OnGameReady = () => { };

    internal static void TriggerGameReady()
    {
        GameReady = true;
        OnGameReady.Invoke();
    }
}