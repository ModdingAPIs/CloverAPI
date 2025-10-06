using CloverAPI.Utils;
using UnityEngine;

namespace CloverAPI.Assets;

public class APIAssets
{
	private const string GENERIC_CHARM_OBJ_NAME = "Powerup Generic Editable";

	internal static AssetBundle _assetBundle;

	internal static GameObject _genericCharmPrefab;

	public static GameObject InstantiateGenericCharm()
	{
		return Object.Instantiate(GetGenericCharmPrefab());
	}

	public static GameObject GetGenericCharmPrefab()
	{
		if ((Object)(object)_assetBundle == null)
		{
			Logging.LogError("AssetBundle not loaded!");
			return null;
		}
		if (_genericCharmPrefab == null)
		{
			_genericCharmPrefab = _assetBundle.LoadAsset<GameObject>("Powerup Generic Editable");
			if (_genericCharmPrefab == null)
			{
				Logging.LogError("Could not find Powerup Generic Editable in AssetBundle!");
				AssetBundleUtils.PrintContentsOfAssetBundle(_assetBundle);
				return null;
			}
		}
		return _genericCharmPrefab;
	}
}
