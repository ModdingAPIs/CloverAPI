using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace CloverAPI.SaveData;

public class CharmData
{
	[JsonProperty]
	internal Dictionary<string, object> Data = new();

	public T Get<T>(string key, T defaultValue = default)
	{
		if (Data.TryGetValue(key, out var value))
		{
			if (value is T tValue)
			{
				return tValue;
			}
			T obj = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
			if (obj != null)
			{
				return obj;
			}
			return defaultValue;
		}
		return defaultValue;
	}

	public string GetString(string key, string defaultValue = null)
	{
		return Get(key, defaultValue);
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		return Get(key, defaultValue);
	}

	public float GetFloat(string key, float defaultValue = 0f)
	{
		return Get(key, defaultValue);
	}

	public bool GetBool(string key, bool defaultValue = false)
	{
		return Get(key, defaultValue);
	}

	public BigInteger GetBigInteger(string key, BigInteger defaultValue = default)
	{
		return Get(key, defaultValue);
	}

	public List<T> GetList<T>(string key, List<T> defaultValue = null)
	{
		return Get(key, defaultValue ?? []);
	}

	public T[] GetArray<T>(string key, T[] defaultValue = null)
	{
		return Get(key, defaultValue ?? []);
	}

	public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string key, Dictionary<TKey, TValue> defaultValue = null)
	{
		return Get(key, defaultValue ?? new Dictionary<TKey, TValue>());
	}

	public void Set<T>(string key, T value)
	{
		Data[key] = value;
	}

	public void Remove(string key)
	{
		Data.Remove(key);
	}

	public bool ContainsKey(string key)
	{
		return Data.ContainsKey(key);
	}
}
