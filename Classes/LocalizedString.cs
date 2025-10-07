using Panik;
using System;
using System.Collections.Generic;

namespace CloverAPI.Classes;

public enum OnMissingLanguage
{
    UseDefault,
    UseFirst,
    ReturnKey,
    ReturnNull,
    ReturnEmpty,
    ReturnErrorAsString,
    ThrowError
}

public class LocalizedString : StringSource
{
    private readonly string _defaultValue;
    private readonly Dictionary<Translation.Language, string> _localizedValues;

    private readonly OnMissingLanguage _onMissingLanguage;
    public string _key = "(Key Undefined)";

    public LocalizedString(Dictionary<Translation.Language, string> localizedValues, string defaultValue = null,
        OnMissingLanguage onMissingLanguage = OnMissingLanguage.ReturnErrorAsString)
    {
        if (localizedValues == null || localizedValues.Count == 0)
        {
            throw new ArgumentException("localizedValues must contain at least one entry.");
        }

        this._localizedValues = localizedValues;
        this._defaultValue = defaultValue;
        this._onMissingLanguage = onMissingLanguage;
    }

    public override string GetString()
    {
        Translation.Language locale = Data.settings.language;
        if (this._localizedValues.TryGetValue(locale, out string value))
        {
            return value;
        }

        switch (this._onMissingLanguage)
        {
            case OnMissingLanguage.UseDefault:
                if (!string.IsNullOrEmpty(this._defaultValue))
                {
                    return this._defaultValue;
                }

                goto case OnMissingLanguage.UseFirst;
            case OnMissingLanguage.UseFirst:
                return new List<string>(this._localizedValues.Values)[0];
            case OnMissingLanguage.ReturnKey:
                return this._key;
            case OnMissingLanguage.ReturnNull:
                return null;
            case OnMissingLanguage.ReturnEmpty:
                return string.Empty;
            case OnMissingLanguage.ReturnErrorAsString:
                return $"[Error: Key '{this._key}' missing for language '{locale}']";
            case OnMissingLanguage.ThrowError:
                throw new Exception($"Missing localization for language: {locale}");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void SetKey(string key)
    {
        this._key = key;
    }
}