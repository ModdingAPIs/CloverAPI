using System.Collections.Generic;
using CloverAPI.Classes;
using CloverAPI.Content.Charms;
using Newtonsoft.Json;

namespace CloverAPI.SaveData;

public class AllCharmData : JsonPersistentData
{
	[JsonProperty]
	internal Dictionary<string, CharmData> Entries = new();

	public static AllCharmData Instance = new();

	public CharmData this[string guid]
	{
		get => GetCharmDataByGUID(guid);
        set => SetCharmDataByGuid(guid, value);
    }

	public CharmData this[PowerupScript.Identifier id]
	{
		get => GetCharmDataByID(id);
        set => SetCharmDataById(id, value);
    }

	public CharmData GetCharmDataByGUID(string guid)
	{
		if (!Entries.TryGetValue(guid, out var data))
		{
			data = new CharmData();
			Entries[guid] = data;
		}
		return data;
	}

	public CharmData GetCharmDataByID(PowerupScript.Identifier id)
	{
		if (CharmManager.IdToGUID.TryGetValue(id, out var guid))
		{
			return GetCharmDataByGUID(guid);
		}
		return null;
	}

	public bool HasCharmDataByGuid(string guid)
	{
		return Entries.ContainsKey(guid);
	}

	public bool HasCharmDataById(PowerupScript.Identifier id)
	{
		if (CharmManager.IdToGUID.TryGetValue(id, out var guid))
		{
			return HasCharmDataByGuid(guid);
		}
		return false;
	}

	public void RemoveCharmDataByGuid(string guid)
	{
		Entries.Remove(guid);
	}

	public void RemoveCharmDataById(PowerupScript.Identifier id)
	{
		if (CharmManager.IdToGUID.TryGetValue(id, out var guid))
		{
			RemoveCharmDataByGuid(guid);
		}
	}

	public void SetCharmDataByGuid(string guid, CharmData data)
	{
		Entries[guid] = data;
	}

	public void SetCharmDataById(PowerupScript.Identifier id, CharmData data)
	{
		if (CharmManager.IdToGUID.TryGetValue(id, out var guid))
		{
			SetCharmDataByGuid(guid, data);
		}
	}

	public override void OnReset()
	{
		Entries.Clear();
	}
}
