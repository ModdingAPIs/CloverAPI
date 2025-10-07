using CloverAPI.Assets;
using CloverAPI.Content.Builders;
using CloverAPI.SaveData;
using CloverAPI.Utils;
using System;
using System.Collections.Generic;
using static PowerupScript;
using Object = UnityEngine.Object;

namespace CloverAPI.Content.Charms;

public static class CharmManager
{
    public static int CurrentId => CustomCharms.Count;
    public static int NextId => CurrentId + 1;

    public static int MaxId =>
        CustomCharms.Count == 0
            ? (int)Identifier.count - 1
            : (int)Identifier.count + CustomCharms.Count; // count is included because we add new charms after it

    public static int TotalCharms =>
        (int)Identifier.count - 1 + CustomCharms.Count; // count is included because we add new charms after it

    internal static List<CharmBuilder> CustomCharms { get; } = new();
    internal static Dictionary<string, Identifier> GUIDToId { get; } = new();
    internal static Dictionary<Identifier, string> IdToGUID { get; } = new();
    internal static Dictionary<Identifier, CharmBuilder> CustomCharmsById { get; } = new();


    public static CharmBuilder Builder(string nameSpace, string identifier)
    {
        return CharmBuilder.Create(nameSpace, identifier);
    }

    public static Identifier RegisterCharm(CharmBuilder charm)
    {
        if (!CustomCharms.Contains(charm))
        {
            charm.id = Identifier.count + NextId;
            GUIDToId[charm.guid] = charm.id;
            IdToGUID[charm.id] = charm.guid;
            CustomCharmsById[charm.id] = charm;
            CustomCharms.Add(charm);
            return charm.id;
        }

        if (charm.id != Identifier.undefined)
        {
            LogWarning("Charm " + charm.GetName() + " is already registered.");
            return charm.id;
        }

        throw new Exception("Charm " + charm.GetName() + " is already registered but has no identifier set.");
    }

    internal static PowerupScript SpawnCustomCharm(Identifier identifier, bool isNewGame)
    {
        int index = identifier - Identifier.count - 1;
        if (index < 0 || index >= CustomCharms.Count)
        {
            LogError($"Invalid custom charm identifier: {identifier}");
            return null;
        }

        CharmBuilder charm = CustomCharms[index];
        GameObject charmObj;
        if (charm.GetModelMode() == CharmBuilder.ModelMode.GenericModel)
        {
            charmObj = APIAssets.InstantiateGenericCharm();
        }
        else if (charm.GetModelMode() == CharmBuilder.ModelMode.ExistingModel)
        {
            charmObj = Object.Instantiate(GetPrefab(charm.GetSourceModel()));
        }
        else
        {
            if (charm.GetModelMode() != CharmBuilder.ModelMode.CustomModel)
            {
                throw new InvalidOperationException(
                    $"Unknown model mode for charm {charm.GetName()} (ID: {identifier}).");
            }

            charmObj = AssetBundleUtils.InstantiateCustomModelCharm(charm.GetCustomModelAssetBundlePath(),
                charm.GetCustomModelName());
        }

        if (charmObj == null)
        {
            LogError($"Could not instantiate charm {charm.GetName()} (ID: {identifier}).");
            return null;
        }

        PowerupScript ps = charmObj.GetComponent<PowerupScript>();
        Texture2D texture = charm.GetTexture();
        if (texture != null)
        {
            ps.materialDefault = new Material(ps.materialDefault);
            ps.materialDefault.mainTexture = texture;
        }

        charm._Initialize(ps, isNewGame);
        return ps;
    }

    internal static Dictionary<Identifier, Identifier> GenerateRemaps(Dictionary<string, Identifier> oldMap)
    {
        return GenerateRemaps(oldMap, GUIDToId);
    }

    internal static Dictionary<Identifier, Identifier> GenerateRemaps(Dictionary<string, Identifier> oldMap,
        Dictionary<string, Identifier> newMap)
    {
        Dictionary<Identifier, Identifier> remaps = new();
        foreach (KeyValuePair<string, Identifier> pair in oldMap)
        {
            if (newMap.TryGetValue(pair.Key, out Identifier newId))
            {
                if (newId != pair.Value)
                {
                    remaps[pair.Value] = newId;
                }
            }
            else
            {
                remaps[pair.Value] = Identifier.undefined;
            }
        }

        foreach (KeyValuePair<string, Identifier> pair in newMap)
        {
            if (!oldMap.ContainsKey(pair.Key))
            {
                remaps[pair.Value] = Identifier.undefined;
            }
        }

        return remaps;
    }

    public static CharmBuilder GetCharmByGUID(string guid)
    {
        if (GUIDToId.TryGetValue(guid, out Identifier id))
        {
            return GetCharmByID(id);
        }

        return null;
    }

    public static CharmBuilder GetCharmByID(Identifier id)
    {
        if (CustomCharmsById.TryGetValue(id, out CharmBuilder charm))
        {
            return charm;
        }

        return null;
    }

    public static CharmData GetCharmDataByGUID(string guid)
    {
        return AllCharmData.Instance.GetCharmDataByGUID(guid);
    }

    public static CharmData GetCharmDataByID(Identifier id)
    {
        return AllCharmData.Instance.GetCharmDataByID(id);
    }

    public static Identifier GetCharmIDByGUID(string guid)
    {
        if (GUIDToId.TryGetValue(guid, out Identifier id))
        {
            return id;
        }

        return Identifier.undefined;
    }

    public static string GetCharmGUIDByID(Identifier id)
    {
        if (IdToGUID.TryGetValue(id, out string guid))
        {
            return guid;
        }

        return null;
    }
}