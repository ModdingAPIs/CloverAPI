using BepInEx.Configuration;
using System.IO;
using CloverAPI.Assets;
using CloverAPI.Content.Builders;
using CloverAPI.Content.Charms;
using CloverAPI.Content.Data;
using CloverAPI.Internal;
using CloverAPI.SaveData;
using CloverAPI.Utils;
using Panik;
using System;
using System.Linq;

namespace CloverAPI;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "ModdingAPIs.cloverpit.CloverAPI";
	public const string PluginName = "Clover API";
	public const string PluginVer = "0.1.0";

	internal static ManualLogSource Log;
	internal static readonly Harmony Harmony = new(PluginGuid);

	internal static string PluginPath;
    internal const string MainContentFolder = "CloverAPI_Content";
    
    public static string DataPath { get; private set; }
    public static string ImagePath { get; private set; }

    internal ConfigEntry<bool> EnableDebugKeys;

	private const int FONT_SIZE = 24;

	private void Awake()
	{
		Log = this.Logger;
		PluginPath = Path.GetDirectoryName(Info.Location);
        DataPath = Path.Combine(PluginPath, MainContentFolder, "Data");
        ImagePath = Path.Combine(PluginPath, MainContentFolder, "Images");
		MakeConfig();
		LoadAssets();
		PersistentDataManager.Register("CharmMappings", CharmMappings.Instance);
		PersistentDataManager.Register("CharmData", AllCharmData.Instance);
	}

	private void MakeConfig()
	{
        EnableDebugKeys = Config.Bind("General", "EnableDebugKeys", false, "If true, enables debug keybinds for testing purposes.");
	}

	private void LoadAssets()
	{
		string text = Path.Combine(PluginPath, DataPath, "templatemodel");
		if (File.Exists(text))
		{
			APIAssets._assetBundle = AssetBundle.LoadFromFile(text);
			return;
		}
		LogError("AssetBundle not found at " + text + "!");
		LogError($"Make sure you have the '{DataPath}' folder in the same directory as the plugin DLL with the 'templatemodel' AssetBundle inside it.");
	}

	private void OnEnable()
	{
		Harmony.PatchAll();
		LogInfo($"Loaded {PluginName}!");
	}

	private void OnDisable()
	{
		Harmony.UnpatchSelf();
	    LogInfo($"Unloaded {PluginName}!");
	}

    private void Update()
    {
        if (EnableDebugKeys.Value)
        {
            var mult = 1;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                mult *= 10;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                mult *= 100;
            }
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                mult *= 1000;
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                GameplayData.CoinsAdd(mult, false);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                GameplayData.CloverTicketsAdd(mult, false);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                var moddedCharms = CharmManager.CustomCharms;
                var randoms = moddedCharms.Pick(4).Select(x => PowerupScript.GetPowerup_Quick(x.id)).ToArray();
                if (randoms.Length < 4)
                {
                    randoms = randoms.Concat(Enumerable.Repeat<PowerupScript>(null, 4 - randoms.Length)).ToArray();
                }

                StoreCapsuleScript.Restock(false, true, randoms, false, false);
            }
        }
    }

    private void OnGUI()
	{
		GUIStyle style = new(GUI.skin.label)
		{
			font = FontsMaster.instance.GetFontNormal(0).sourceFontFile,
			fontSize = FONT_SIZE,
			normal = 
			{
				textColor = new Color(0f, 0.8f, 0f)
			}
		};
		GUI.Label(new Rect(32f, 30f, 1000f, 40f), $"Modded, {PluginName} v{PluginVer}", style);
	}
}
