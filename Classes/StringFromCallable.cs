using System;

namespace CloverAPI.Classes;

public class StringFromCallable : StringSource
{
    private readonly StringProvider _provider;

    public StringFromCallable(Func<string> provider)
    {
        this._provider = provider.Invoke;
    }

    public override string GetString()
    {
        return this._provider();
    }

    private delegate string StringProvider();
}