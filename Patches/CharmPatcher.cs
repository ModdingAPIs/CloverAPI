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
        int repl = 0;
        const int count = (int)PowerupScript.Identifier.count;
        // Replace all instances of 'ldc.i4' with 'count' as operand with 'ldc.i4' with 'TotalCharms' as operand
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int operand &&
                operand == count) // No need to check for 'ldc.i4.s' since count > 127
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4, CharmManager.TotalCharms);
                repl++;
            }
            else
            {
                yield return instruction;
            }
        }

        if (repl == 0)
        {
            LogError("Failed to apply GameplayData._EnsurePowerupDataArray transpiler patch!");
        }

        if (repl > 1)
        {
            LogWarning(
                $"GameplayData._EnsurePowerupDataArray transpiler patch replaced {repl} instances of 'ldc.i4 {count}'. Expected to replace exactly 1 instance, but could be fine if an update added more.");
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