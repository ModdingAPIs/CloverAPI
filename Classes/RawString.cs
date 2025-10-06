namespace CloverAPI.Classes;

public class RawString : StringSource
{
	private string _value;

	public RawString(string value)
	{
		_value = value;
	}

	public override string GetString()
	{
		return _value;
	}

	public static implicit operator RawString(string value) => new(value);
	public static implicit operator string(RawString src) => src._value;
}
