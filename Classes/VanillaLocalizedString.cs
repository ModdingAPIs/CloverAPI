using CloverAPI.Content.Strings;
using Panik;

namespace CloverAPI.Classes;

public class VanillaLocalizedString : StringSource
{
    public string Key { get; }

    public VanillaLocalizedString(string key)
    {
        Key = key;
    }

    public override string GetString()
    {
        return Translation.Get(Key);
    }
}
