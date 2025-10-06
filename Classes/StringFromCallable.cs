using System;

namespace CloverAPI.Classes;

public class StringFromCallable : StringSource
{
	private delegate string StringProvider();

	private StringProvider _provider;

	public StringFromCallable(Func<string> provider)
	{
		_provider = provider.Invoke;
	}

	public override string GetString()
	{
		return _provider();
	}
}
