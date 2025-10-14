using CloverAPI.Classes;
using CloverAPI.Content.Charms;
using CloverAPI.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloverAPI.SaveData;

public class AllCharmData : JsonPersistentData
{
    public static AllCharmData Instance = new();

    [JsonProperty] internal Dictionary<string, CharmData> Entries = new();

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
        if (!this.Entries.TryGetValueNoCase(guid, out CharmData data))
        {
            data = new CharmData();
            this.Entries[guid] = data;
        }

        return data;
    }

    public CharmData GetCharmDataByID(PowerupScript.Identifier id)
    {
        if (CharmManager.IdToGUID.TryGetValue(id, out string guid))
        {
            return GetCharmDataByGUID(guid);
        }

        return null;
    }

    public bool HasCharmDataByGuid(string guid)
    {
        return this.Entries.ContainsKey(guid);
    }

    public bool HasCharmDataById(PowerupScript.Identifier id)
    {
        if (CharmManager.IdToGUID.TryGetValue(id, out string guid))
        {
            return HasCharmDataByGuid(guid);
        }

        return false;
    }

    public void RemoveCharmDataByGuid(string guid)
    {
        this.Entries.Remove(guid);
    }

    public void RemoveCharmDataById(PowerupScript.Identifier id)
    {
        if (CharmManager.IdToGUID.TryGetValue(id, out string guid))
        {
            RemoveCharmDataByGuid(guid);
        }
    }

    public void SetCharmDataByGuid(string guid, CharmData data)
    {
        this.Entries[guid] = data;
    }

    public void SetCharmDataById(PowerupScript.Identifier id, CharmData data)
    {
        if (CharmManager.IdToGUID.TryGetValue(id, out string guid))
        {
            SetCharmDataByGuid(guid, data);
        }
    }

    public override void OnReset()
    {
        this.Entries.Clear();
    }
}