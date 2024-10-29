using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Game.State;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Serilog;
using UI.Builder;
using UI.CarInspector;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("BleedAll")]
public static class BleedAll
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateCarPanel))]
    public static IEnumerable<CodeInstruction> PopulateCarPanel(IEnumerable<CodeInstruction> instructions) {
        var codeInstructions = instructions.ToList();

        // PopulateCarPanel end with this Op sequence, patch will remove instructions marked with x
        var valid = true;
        OpCode[] sequence = [
            OpCodes.Call,
            OpCodes.Ldloc_0, // x
            OpCodes.Ldflda,  // x
            OpCodes.Ldloc_0, // x
            OpCodes.Ldftn,   // x
            OpCodes.Newobj,  // x
            OpCodes.Ldc_R4,  // x
            OpCodes.Call,    // x
            OpCodes.Pop,
            OpCodes.Ret
        ];

        var start = codeInstructions.Count - sequence.Length;
        for (int i = 0, j = start; i < sequence.Length; i++, j++) {
            if (codeInstructions[j]!.opcode != sequence[i]) {
                valid = false;
            }
        }

        if (codeInstructions[start]!.operand is not MethodInfo { Name: "AddExpandingVerticalSpacer" }) {
            valid = false;
        }

        if (!valid) {
            Log.Error("CarInspector.PopulateCarPanel has changed - 'Follow_BleedAll' disabled");
        } else {
            codeInstructions.RemoveRange(start + 1, 8);
        }

        return codeInstructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateCarPanel))]
    public static void PopulateCarPanel(CarInspector __instance, UIPanelBuilder builder, Car ____car) {
        builder.HStack(hStack => {
            hStack.AddButton("Select", __instance.SelectConsist)!
                  .Tooltip("Select Car", "Selected locomotives display HUD controls. Shortcuts allow jumping to the selected car.");

            hStack.AddButton("Follow", () => CameraSelector.shared!.FollowCar(____car))!
                  .Tooltip("Follow Car", "Jump the overhead camera to this car and track it.");
            
            hStack.AddButton("Bleed all", () => Execute(____car.EnumerateCoupled()!.ToList()))!
                    .Tooltip("Bleed all Valves", "Bleed the brakes to release pressure from the consist brake system.");
            
            hStack.Spacer();
            if (!StateManager.IsSandbox) {
                return;
            }

            hStack.AddButton("Delete", __instance.DeleteConsist)!
                  .Tooltip("Delete Car", "Click to delete this car. Shift-Click deletes all coupled cars.");
        });
    }

    private static void Execute(List<Car> consist) {
        consist.Do(c => {
            if (c.SupportsBleed()) {
                c.SetBleed();
            }
        });
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CarInspector), nameof(SelectConsist))]
    private static void SelectConsist(this CarInspector carInspector) => throw new NotImplementedException("It's a stub: CarInspector.SelectConsist");

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CarInspector), nameof(DeleteConsist))]
    private static void DeleteConsist(this CarInspector carInspector) => throw new NotImplementedException("It's a stub: CarInspector.DeleteConsist");
}
