using CloverAPI.Content.Data;
using Cysharp.Threading.Tasks;
using Panik;
using System.IO;

namespace CloverAPI.Patches;

[HarmonyPatch]
internal class SavePatcher
{
    [HarmonyPatch(typeof(Data), nameof(Data.SaveGame))]
    [HarmonyPrefix]
    internal static void Data_SaveGame_Postfix()
    {
        PersistentDataManager.SaveData().Forget();
    }

    [HarmonyPatch(typeof(Data), nameof(Data.LoadGame))]
    [HarmonyPostfix]
    internal static void Data_LoadGame_Postfix()
    {
        PersistentDataManager.LoadData().Forget();
    }

    [HarmonyPatch(typeof(Data.GameData), nameof(Data.GameData.GameplayDataReset))]
    [HarmonyPostfix]
    internal static void Data_GameData_GameplayDataReset_Postfix(Data.GameData __instance)
    {
        PersistentDataManager.ResetAll();
    }

    [HarmonyPatch(typeof(PlatformDataMaster), nameof(PlatformDataMaster.PathGet_GameDataFile))]
    [HarmonyPostfix]
    internal static void PlatformDataMaster_PathGet_GameDataFile_Postfix(ref string __result, string extraAppendix)
    {
        string moddedFile = PlatformDataMaster.GameFolderPath + "GameDataModded" + extraAppendix + ".json";
        if (!File.Exists(moddedFile) && File.Exists(__result))
        {
            File.Copy(__result, moddedFile);
        }

        __result = moddedFile;
    }
}