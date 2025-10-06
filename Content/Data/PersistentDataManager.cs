using System.Collections.Generic;
using CloverAPI.Classes;
using Cysharp.Threading.Tasks;
using Panik;

namespace CloverAPI.Content.Data;

public class PersistentDataManager
{
	internal static Dictionary<string, PersistentData> PersistentDataItems = new();

	public static void Register(string name, PersistentData data)
	{
		PersistentDataItems[name] = data;
	}

	public static async UniTask<bool> SaveData()
	{
		bool anyFailed = false;
		foreach (string key in PersistentDataItems.Keys)
		{
			PersistentDataItems[key].BeforeSave();
			bool flag = anyFailed;
			anyFailed = flag | !(await _Save(key, PersistentDataItems[key]));
			PersistentDataItems[key].AfterSave();
		}
		return !anyFailed;
	}

	public static async UniTask<bool> LoadData()
	{
		bool anyFailed = false;
		foreach (string key in PersistentDataItems.Keys)
		{
			PersistentDataItems[key].BeforeLoad();
			bool flag = anyFailed;
			anyFailed = flag | !(await _Load(key, PersistentDataItems[key]));
			PersistentDataItems[key].AfterLoad();
		}
		return !anyFailed;
	}

	private static async UniTask<bool> _Save(string name, PersistentData data)
	{
		return await PlatformDataMaster.Save(name, PlatformDataMaster.GameFolderPath + "ModData_" + name + ".json", data.ToString());
	}

	private static async UniTask<bool> _Load(string name, PersistentData data)
	{
		string json = await PlatformDataMaster.Load(name, PlatformDataMaster.GameFolderPath + "ModData_" + name + ".json");
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		data.FromString(json);
		return true;
	}

	public static void ResetAll()
	{
		foreach (string key in PersistentDataItems.Keys)
		{
			PersistentDataItems[key].OnReset();
		}
	}
}
