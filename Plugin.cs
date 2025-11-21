using BepInEx.Configuration;
using CloverAPI.Assets;
using CloverAPI.Content.Builders;
using CloverAPI.Content.Charms;
using CloverAPI.Content.Data;
using CloverAPI.Content.Settings;
using CloverAPI.Content.Strings;
using CloverAPI.Internal;
using CloverAPI.SaveData;
using CloverAPI.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CloverAPI;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
[HarmonyPatch]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "ModdingAPIs.cloverpit.CloverAPI";
    public const string PluginName = "Clover API";
    public const string PluginVer = "0.2.0";
    internal const string MainContentFolder = "CloverAPI_Content";

    private const int FONT_SIZE = 16;

    internal static ManualLogSource Log;
    internal static readonly Harmony Harmony = new(PluginGuid);

    internal static string PluginPath;

    internal static ConfigEntry<bool> EnableDebugKeys;
    internal static ConfigEntry<bool> UseFullQualityTextures;
    internal static ConfigEntry<string> OverrideOrdering;

    public static string DataPath { get; private set; }
    public static string ImagePath { get; private set; }

    private void Awake()
    {
        Log = this.Logger;
        PluginPath = Path.GetDirectoryName(this.Info.Location);
        DataPath = Path.Combine(PluginPath, MainContentFolder, "Data");
        ImagePath = Path.Combine(PluginPath, MainContentFolder, "Images");
        MakeConfig();
        LoadAssets();
        GameUtils.OnGameReady += OnReady;
        PersistentDataManager.RegisterData("CharmMappings", CharmMappings.Instance);
        PersistentDataManager.RegisterData("CharmData", AllCharmData.Instance);
    }

    private void Update()
    {
        if (EnableDebugKeys.Value)
        {
            int mult = 1;
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
                List<CharmBuilder> moddedCharms = CharmManager.CustomCharms;
                PowerupScript[] randoms =
                    moddedCharms.Pick(4).Select(x => PowerupScript.GetPowerup_Quick(x.id)).ToArray();
                if (randoms.Length < 4)
                {
                    randoms = randoms.Concat(Enumerable.Repeat<PowerupScript>(null, 4 - randoms.Length)).ToArray();
                }

                StoreCapsuleScript.Restock(false, true, randoms, false, false);
            }
        }
    }

    private void OnReady()
    {
        LoadData();
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

    private void MakeConfig()
    {
        EnableDebugKeys = this.Config.Bind("General", "EnableDebugKeys", false,
            "If true, enables debug keybinds for testing purposes.");
        UseFullQualityTextures = this.Config.Bind("General", "UseFullQualityTextures", true,
            "If true, uses full quality textures for modded content. If false, resizes them to match the size of the original texture. Some textures require a restart to take effect.");
        ModSettingsManager.RegisterPageFromConfig(this, "Clover API Settings");
        OverrideOrdering = this.Config.Bind("General", "OverrideOrdering", "",
            "A comma-separated list of mod GUIDs that specifies the order in which overrides are applied. The leftmost mod in the list has the highest priority. Mods that have conflicting overrides will automatically be added to the end.");
        ResourceOrdering._ConfigRef(OverrideOrdering);
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
        LogError(
            $"Make sure you have the '{DataPath}' folder in the same directory as the plugin DLL with the 'templatemodel' AssetBundle inside it.");
    }

    private void LoadData()
    {
        LoadTranslations();
    }
    
    private void LoadTranslations()
    {
        string root = PluginPath;
        var trFiles = Directory.GetFiles(root, "*.loc",
            SearchOption.AllDirectories);
        foreach (var file in trFiles)
        {
            LanguageManager.LoadFromFile(file);
        }
        if (trFiles.Length > 0)
        {
            LogInfo($"Loaded {trFiles.Length} translation files.");
        }
    }
}