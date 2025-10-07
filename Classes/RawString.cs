namespace CloverAPI.Classes;

public class RawString : StringSource
{
    private readonly string _value;

    public RawString(string value)
    {
        this._value = value;
    }

    public override string GetString()
    {
        return this._value;
    }

    public static implicit operator RawString(string value)
    {
        return new RawString(value);
    }

    public static implicit operator string(RawString src)
    {
        return src._value;
    }
}