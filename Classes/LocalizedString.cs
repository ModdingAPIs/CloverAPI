using System;
using System.Collections.Generic;
using Panik;

namespace CloverAPI.Classes;

public class LocalizedString : StringSource
{
	public string _key = "(Key Undefined)";
	private Dictionary<Translation.Language, string> _localizedValues;

	private string _defaultValue;

	private OnMissingLanguage _onMissingLanguage = OnMissingLanguage.UseDefault;

	public LocalizedString(Dictionary<Translation.Language, string> localizedValues, string defaultValue = null, OnMissingLanguage onMissingLanguage = OnMissingLanguage.ReturnErrorAsString)
	{
		if (localizedValues == null || localizedValues.Count == 0)
		{
			throw new ArgumentException("localizedValues must contain at least one entry.");
		}
		_localizedValues = localizedValues;
		_defaultValue = defaultValue;
		_onMissingLanguage = onMissingLanguage;
	}

	public override string GetString()
	{
		Translation.Language locale = Data.settings.language;
		if (_localizedValues.TryGetValue(locale, out var value))
		{
			return value;
		}
		switch (_onMissingLanguage)
		{
		case OnMissingLanguage.UseDefault:
			if (!string.IsNullOrEmpty(_defaultValue))
			{
				return _defaultValue;
			}
			goto case OnMissingLanguage.UseFirst;
		case OnMissingLanguage.UseFirst:
			return new List<string>(_localizedValues.Values)[0];
		case OnMissingLanguage.ReturnKey:
			return _key;
		case OnMissingLanguage.ReturnNull:
			return null;
		case OnMissingLanguage.ReturnEmpty:
			return string.Empty;
		case OnMissingLanguage.ReturnErrorAsString:
			return $"[Error: Key '{_key}' missing for language '{locale}']";
		case OnMissingLanguage.ThrowError:
			throw new Exception($"Missing localization for language: {locale}");
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

    public override void SetKey(string key)
    {
        _key = key;
    }
}
