using System;
using System.Reflection;

using static Panik.Data;

public static class transitionUtils
{
    private static SettingsData Instance
    {
        get
        {
            var instProp = typeof(SettingsData).GetProperty("Inst", BindingFlags.Public | BindingFlags.Static);
            var instance = instProp?.GetValue(null);

            if (instance == null)
                throw new FailedToGetInstanceException("SettingsData instance could not be retrieved.");

            return (SettingsData)instance;
        }
    }

    private static FieldInfo TransitionSpeedField =>
        typeof(SettingsData).GetField("transitionSpeed", BindingFlags.Public | BindingFlags.Instance);

    public static int GetAnimationSpeed()
    {
        var field = TransitionSpeedField;
        if (field == null)
            throw new MissingFieldException("SettingsData.transitionSpeed field not found.");

        return (int)field.GetValue(Instance);
    }

    public static void SetAnimationSpeed(int value)
    {
        var field = TransitionSpeedField;
        if (field == null)
            throw new MissingFieldException("SettingsData.transitionSpeed field not found.");

        field.SetValue(Instance, value);
    }

    public static void AddAnimationSpeed(int value)
    {
        var field = TransitionSpeedField;
        if (field == null)
            throw new MissingFieldException("SettingsData.transitionSpeed field not found.");

        int current = (int)field.GetValue(Instance);
        field.SetValue(Instance, current + value);
    }

    public class FailedToGetInstanceException : Exception
    {
        public FailedToGetInstanceException(string message) : base(message) { }
    }
}
