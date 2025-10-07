using Panik;

namespace CloverAPI.Classes;

public class VanillaLocalizedString : StringSource
{
    public VanillaLocalizedString(string key)
    {
        this.Key = key;
    }

    public string Key { get; }

    public override string GetString()
    {
        return Translation.Get(this.Key);
    }
}