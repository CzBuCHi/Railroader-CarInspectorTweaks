using System;
using System.Collections.Generic;
using System.Linq;
using CarInspectorTweaks.Extensions;
using Game.State;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using UI.Builder;
using UI.CarInspector;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("BleedAll")]
public static class BleedAll
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), "PopulateCarPanel")]
    public static void PopulateCarPanelPostfix(CarInspector __instance, UIPanelBuilder builder, Car ____car) {
        builder.ReplaceHStack(builder.GetFieldCount() - 1, hStack => {
            hStack.AddButton("Select", __instance.SelectConsist)!
                  .Tooltip("Select Car", "Selected locomotives display HUD controls. Shortcuts allow jumping to the selected car.");

            hStack.AddButton("Follow", () => CameraSelector.shared!.FollowCar(____car))!
                  .Tooltip("Follow Car", "Jump the overhead camera to this car and track it.");

            hStack.AddButton("Bleed all", () => BleedAllCars(____car.EnumerateCoupled()!.ToList()))!
                  .Tooltip("Bleed all Valves", "Bleed the brakes to release pressure from the consist brake system.");

            hStack.Spacer();
            if (!StateManager.IsSandbox) {
                return;
            }

            hStack.AddButton("Delete", __instance.DeleteConsist)!
                  .Tooltip("Delete Car", "Click to delete this car. Shift-Click deletes all coupled cars.");
        });
    }

    private static void BleedAllCars(List<Car> consist) {
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
