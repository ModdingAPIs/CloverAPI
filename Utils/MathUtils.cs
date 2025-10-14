using System;

namespace CloverAPI.Utils;

public class MathUtils
{
    public static float RoundToMultipleOf(float value, float multiple)
    {
        if (multiple == 0)
        {
            throw new ArgumentException("Multiple cannot be zero.", nameof(multiple));
        }
        return (float)(Math.Round(value / multiple) * multiple);
    }
    
    public static float RoundToNearestSignificantFive(float value, int unroundedSignificantDigits = 1)
    {
        if (value == 0)
        {
            return 0;
        }
        float scale = (float)Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1 - unroundedSignificantDigits);
        return RoundToMultipleOf(value, 0.5f * scale);
    }

    public static float RoundToNearestSignificant(float value, int significantDigits = 1)
    {
        if (value == 0)
        {
            return 0;
        }
        float scale = (float)Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1 - significantDigits);
        return (float)(Math.Round(value / scale) * scale);
    }
}