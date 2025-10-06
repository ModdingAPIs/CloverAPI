using System.Collections.Generic;

namespace CloverAPI.Utils;

public static class AssetBundleUtils
{
	private static readonly Dictionary<string, AssetBundle> LoadedAssetBundles = new();

	public static GameObject InstantiateCustomModelCharm(string assetBundleName, string prefabName = null)
	{
		return Object.Instantiate(LoadFromAssetBundle<GameObject>(assetBundleName, prefabName));
	}

	public static T LoadFromAssetBundle<T>(string assetBundleName, string assetName = null) where T : Object
	{
		string path = FileUtils.FindFile(assetBundleName);
		AssetBundle bundle = LoadAssetBundle(path);
		if (bundle == null)
		{
			LogError("Could not load AssetBundle from path: " + path);
			return null;
		}
		if (string.IsNullOrEmpty(assetName))
		{
			T[] itemsOfType = bundle.LoadAllAssets<T>();
			if (itemsOfType.Length != 0)
			{
				if (itemsOfType.Length > 1)
				{
					LogWarning($"Multiple assets of type {typeof(T)} found in AssetBundle '{assetBundleName}'. Using the first one found: '{itemsOfType[0].name}'.");
				}
				return itemsOfType[0];
			}
			LogError($"No assets of type {typeof(T)} found in AssetBundle '{assetBundleName}'.");
			PrintContentsOfAssetBundle(bundle);
			return null;
		}
		T asset = bundle.LoadAsset<T>(assetName);
		if (asset == null)
		{
			LogError($"Could not find asset '{assetName}' of type {typeof(T)} in AssetBundle '{assetBundleName}'.");
			PrintContentsOfAssetBundle(bundle);
			return null;
		}
		return asset;
	}

	public static AssetBundle LoadAssetBundle(string path)
	{
		if (LoadedAssetBundles.TryGetValue(path, out var assetBundle))
		{
			return assetBundle;
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		if (bundle == null)
		{
			LogError("Failed to load AssetBundle from path: " + path);
			return null;
		}
		LoadedAssetBundles[path] = bundle;
		return bundle;
	}

	public static void PrintContentsOfAssetBundle(AssetBundle assetBundle)
	{
		string[] assetNames = assetBundle.GetAllAssetNames();
		LogInfo("AssetBundle contains the following assets:");
		string[] array = assetNames;
		foreach (string name in array)
		{
			LogInfo("- " + name);
		}
	}
}
