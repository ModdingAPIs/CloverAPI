using static Panik.Data;
using System;
using System.Reflection;

public static class TrippleSixUtils
{

    private static readonly FieldInfo TrippleSixField =
        typeof(SettingsData).GetField("_666Chance", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo TrippleSixMaxField =
        typeof(SettingsData).GetField("_666ChanceMaxAbsolute", BindingFlags.NonPublic | BindingFlags.Instance);


    public static float GetTrippleSixChance()
    {
        if (TrippleSixField == null)
            throw new MissingFieldException("SettingsData._666Chance field not found.");

        return (float)TrippleSixField.GetValue(TrippleSixField);
    }

    public static float GetTrippleSixMaxChance()
    {
        if (TrippleSixMaxField == null)
            throw new MissingFieldException("SettingsData._666ChanceMaxAbsolute field not found.");

        return (float)TrippleSixMaxField.GetValue(TrippleSixMaxField);
    }

    public static void SetTrippleSixChance(float value)
    {
        if (TrippleSixField == null || TrippleSixMaxField == null)
            throw new MissingFieldException("One or more _666Chance fields not found.");

        float max = GetTrippleSixMaxChance();
        value = Mathf.Clamp(value, 0f, max);

        TrippleSixField.SetValue(TrippleSixField, value);
    }

    public static void SetTrippleSixMaxChance(float value)
    {
        if (TrippleSixField == null)
            throw new MissingFieldException("_666ChanceMaxAbsolute field not found.");
        value = Mathf.Clamp(value, 0f, 1f);
        // 0.1 --> 10%, 1.0 --> 100%
        TrippleSixMaxField.SetValue(TrippleSixMaxField, value);
    }

    public static void AddTrippleSixChance(float value)
    {
        SetTrippleSixChance(GetTrippleSixChance() + value);
    }

    public class FailedToGetInstanceException : Exception
    {
        public FailedToGetInstanceException(string message) : base(message) { }
    }
}
