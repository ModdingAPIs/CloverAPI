using CloverAPI.Utils;

namespace CloverAPI.Assets;

internal class APIAssets
{
    private const string GENERIC_CHARM_OBJ_NAME = "Powerup Generic Editable";

    internal static AssetBundle _assetBundle;

    internal static GameObject _genericCharmPrefab;

    internal static GameObject InstantiateGenericCharm()
    {
        return Object.Instantiate(GetGenericCharmPrefab());
    }

    internal static GameObject GetGenericCharmPrefab()
    {
        if (_assetBundle == null)
        {
            LogError("AssetBundle not loaded!");
            return null;
        }

        if (_genericCharmPrefab == null)
        {
            _genericCharmPrefab = _assetBundle.LoadAsset<GameObject>(GENERIC_CHARM_OBJ_NAME);
            if (_genericCharmPrefab == null)
            {
                LogError($"Could not find {GENERIC_CHARM_OBJ_NAME} in AssetBundle!");
                AssetBundleUtils.PrintContentsOfAssetBundle(_assetBundle);
                return null;
            }
        }

        return _genericCharmPrefab;
    }
}