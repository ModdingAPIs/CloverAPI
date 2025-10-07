using System;

namespace CloverAPI.Classes;

public abstract class StringSource
{
    public abstract string GetString();

    public static implicit operator StringSource(string value)
    {
        return new RawString(value);
    }

    public static implicit operator string(StringSource src)
    {
        return src.GetString();
    }

    public static implicit operator StringSource(Func<string> func)
    {
        return new StringFromCallable(func);
    }

    public virtual void SetKey(string key) { }
}