using CloverAPI.Content.Builders;
using CloverAPI.Content.Charms;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CloverAPI.Patches;

[HarmonyPatch]
internal class CharmPatcher
{
    [HarmonyPatch(typeof(PowerupScript), nameof(PowerupScript.InitializeAll))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> PowerupScript_InitializeAll_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        const int maxId = (int)PowerupScript.Identifier.count - 1;
        // Yield original instructions until 'ldc.i4' with maxId is found
        IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();
        bool done = false;
        while (enumerator.MoveNext())
        {
            CodeInstruction instruction = enumerator.Current!;
            yield return instruction;
            if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int operand &&
                operand == maxId) // No need to check for 'ldc.i4.s' since maxId > 127
            {
                break;
            }
        }

        // Yield original instructions until 'callvirt' 'PowerupScript::Initialize' is found
        while (enumerator.MoveNext())
        {
            CodeInstruction instruction = enumerator.Current!;
            yield return instruction;

            // Check for 'callvirt' 'PowerupScript::Initialize'
            if (instruction.opcode == OpCodes.Callvirt && instruction.operand is MethodInfo methodInfo &&
                methodInfo == AccessTools.Method(typeof(PowerupScript), "Initialize"))
            {
                // Insert call to 'InitCustomCharms' after 'PowerupScript::Initialize'
                yield return new CodeInstruction(OpCodes.Ldarg_1); // Load 'isNewGame' argument
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(CharmPatcher), nameof(InitCustomCharms))); // Call 'InitCustomCharms'
                done = true;
                break;
            }
        }

        // Yield remaining original instructions
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        if (!done)
        {
            LogError("Failed to apply PowerupScript.InitializeAll transpiler patch!");
        }

        enumerator.Dispose();
    }

    [HarmonyPatch(typeof(GameplayData), nameof(GameplayData._EnsurePowerupDataArray))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> GameplayData__EnsurePowerupDataArray_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return ReplaceILCharmCounts(instructions, caller: "GameplayData._EnsurePowerupDataArray");
    }
    
    [HarmonyPatch(typeof(TerminalScript), nameof(TerminalScript.Initialize))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> TerminalScript_Initialize_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return ReplaceILCharmCounts(instructions, expectedAmountSeen: 2, caller: "TerminalScript.Initialize");
    }
    
#pragma warning disable Harmony003
    private static IEnumerable<CodeInstruction> ReplaceILCharmCounts(IEnumerable<CodeInstruction> instructions, string onNotFound = "error", string onMultipleFound = "warn", int replaceAmount = 1, int expectedAmountSeen = 1, string caller = "unknown")
    {
        if (replaceAmount < 1 || expectedAmountSeen < 1)
        {
            LogError($"Invalid arguments to ReplaceILCharmCounts in '{caller}'. Both replaceAmount and expectedAmountSeen must be at least 1.");
            replaceAmount = Mathf.Max(1, replaceAmount);
            expectedAmountSeen = Mathf.Max(1, expectedAmountSeen);
        }
        int seen = 0;
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int operand &&
                operand == (int)PowerupScript.Identifier.count) // No need to check for 'ldc.i4.s' since count > 127
            {
                if (seen < replaceAmount)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(CharmManager), nameof(CharmManager.NewCount)));
                }
                else
                {
                    yield return instruction;
                }
                seen++;
            }
            else
            {
                yield return instruction;
            }
        }

        if (seen < expectedAmountSeen)
        {
            if (expectedAmountSeen == 1)
            {
                if (onNotFound == "error")
                {
                    LogError(
                        $"Transpiler patch in '{caller}' failed to replace any instances of 'ldc.i4 {(int)PowerupScript.Identifier.count}'!");
                }
                else if (onNotFound == "warn")
                {
                    LogWarning(
                        $"Transpiler patch in '{caller}' failed to replace any instances of 'ldc.i4 {(int)PowerupScript.Identifier.count}'!");
                }
            }
            else
            {
                if (onNotFound == "error")
                {
                    LogError(
                        $"Transpiler patch in '{caller}' replaced only {seen} instances of 'ldc.i4 {(int)PowerupScript.Identifier.count}'. Expected to replace exactly {expectedAmountSeen} instances, but could be fine if an update changed this.");
                }
                else if (onNotFound == "warn")
                {
                    LogWarning(
                        $"Transpiler patch in '{caller}' replaced only {seen} instances of 'ldc.i4 {(int)PowerupScript.Identifier.count}'. Expected to replace exactly {expectedAmountSeen} instances, but could be fine if an update changed this.");
                }
            }
        }
        if (seen > expectedAmountSeen)
        {
            if (onMultipleFound == "error")
            {
                LogError(
                    $"Transpiler patch in '{caller}' replaced {seen} instances of 'ldc.i4 {(int)PowerupScript.Identifier.count}'. Expected to replace exactly {expectedAmountSeen} instance{(expectedAmountSeen == 1 ? "" : "s")}!");
            }
            else if (onMultipleFound == "warn")
            {
                LogWarning(
                    $"Transpiler patch in '{caller}' replaced {seen} instances of 'ldc.i4 {(int)PowerupScript.Identifier.count}'. Expected to replace exactly {expectedAmountSeen} instance{(expectedAmountSeen == 1 ? "" : "s")}!");
            }
        }
    }

    private static void InitCustomCharms(bool isNewGame)
    {
        foreach (CharmBuilder charm in CharmManager.CustomCharms)
        {
            CharmManager.SpawnCustomCharm(charm.id, isNewGame);
        }
    }
}
#pragma warning restore Harmony003