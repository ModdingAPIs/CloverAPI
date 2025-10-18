using static Panik.Data;
using System;
using System.Reflection;

public static class RedButtonUtils
{

    private static readonly FieldInfo RedButtonMultiplier =
        typeof(SettingsData).GetField("_redButtonActivationsMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);

    public static float GetRedButtonMultiplier()
    {
        if (RedButtonMultiplier == null)
            throw new MissingFieldException("SettingsData._redButtonActivationsMultiplier field not found.");

        return (int)RedButtonMultiplier.GetValue(RedButtonMultiplier);
    }

    public static void SetRedButtonMultiplier(float value)
    {
        if (RedButtonMultiplier == null)
            throw new MissingFieldException("SettingsData._redButtonActivationsMultiplier fields not found.");

        RedButtonMultiplier.SetValue(RedButtonMultiplier, value);
    }

    public static void AddRedButtonMultiplier(float value)
    {
        AddRedButtonMultiplier(GetRedButtonMultiplier() + value);
    }

    public class FailedToGetInstanceException : Exception
    {
        public FailedToGetInstanceException(string message) : base(message) { }
    }
}
