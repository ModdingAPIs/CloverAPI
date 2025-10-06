using System.Collections.Generic;
using CloverAPI.Classes;
using CloverAPI.Content.Charms;
using Newtonsoft.Json;
using Panik;

namespace CloverAPI.Internal;

internal class CharmMappings : JsonPersistentData
{
	internal static CharmMappings Instance = new();

	[JsonProperty]
	internal Dictionary<string, PowerupScript.Identifier> Mappings = new();

	public override void BeforeSave()
	{
		Mappings = CharmManager.GUIDToId;
	}

	public override void AfterLoad()
	{
		Dictionary<PowerupScript.Identifier, PowerupScript.Identifier> remap = CharmManager.GenerateRemaps(Mappings);
		if (remap.Count == 0)
		{
			LogInfo("No Charm mappings to apply.");
			return;
		}
		LogInfo("Current Charm mappings:");
		foreach (KeyValuePair<PowerupScript.Identifier, PowerupScript.Identifier> pair in remap)
		{
			LogInfo($"  {pair.Key} -> {pair.Value}");
		}
		for (int i = 0; i < GameplayData.Instance.equippedPowerups.Length; i++)
		{
			string p1 = GameplayData.Instance.equippedPowerups[i];
			PowerupScript.Identifier id = PlatformDataMaster.EnumEntryFromString(p1, PowerupScript.Identifier.undefined);
			if (remap.TryGetValue(id, out var newId))
			{
				GameplayData.Instance.equippedPowerups[i] = newId.ToString();
			}
		}
		for (int i = 0; i < GameplayData.Instance.equippedPowerups_Skeleton.Length; i++)
		{
			string p2 = GameplayData.Instance.equippedPowerups_Skeleton[i];
			PowerupScript.Identifier id2 = PlatformDataMaster.EnumEntryFromString(p2, PowerupScript.Identifier.undefined);
			if (remap.TryGetValue(id2, out var newId2))
			{
				GameplayData.Instance.equippedPowerups_Skeleton[i] = newId2.ToString();
			}
		}
		for (int i = 0; i < GameplayData.Instance.drawerPowerups.Length; i++)
		{
			string p3 = GameplayData.Instance.drawerPowerups[i];
			PowerupScript.Identifier id3 = PlatformDataMaster.EnumEntryFromString(p3, PowerupScript.Identifier.undefined);
			if (remap.TryGetValue(id3, out var newId3))
			{
				GameplayData.Instance.drawerPowerups[i] = newId3.ToString();
			}
		}
	}
}
