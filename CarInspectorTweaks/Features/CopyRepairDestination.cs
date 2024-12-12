using System;
using System.Linq;
using Game;
using Game.State;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.Ops;
using UI.Builder;
using UI.CarInspector;
using UI.CompanyWindow;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("CopyRepairDestination")]
public static class CopyRepairDestination
{
    // TODO
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateEquipmentPanel))]
    public static bool PopulateEquipmentPanel(CarInspector __instance, UIPanelBuilder builder, Car ____car) {
        builder.AddConditionField(____car);
        builder.AddMileageField(____car);
        if (____car.Condition < 0.99900001287460327) {
            var str = GameDateTimeInterval.DeltaStringMinutes((int)(CalculateRepairWorkOverall(____car) * 60.0 * 24.0))!;
            builder.AddField("Repair Estimate", str);
        }

        builder.AddRepairDestination(____car);
        builder.Spacer(2f);
        if (EquipmentPurchase.CarCanBeSold(____car)) {
            builder.AddSellDestination(____car);
            builder.Spacer(2f);
        }

        builder.AddExpandingVerticalSpacer();

        builder.ButtonStrip(strip => {
            var    canCustomize = __instance.CanCustomize(out var reason);
            if (canCustomize || !string.IsNullOrEmpty(reason)) {
                var customize= strip.AddButton("Customize", __instance.ShowCustomize)!;
                if (!canCustomize) {
                    customize.Disable(true)!
                             .Tooltip("Customize Not Available", reason);
                }
            }

            ____car.KeyValueObject!.Observe(OverrideDestination.Repair.Key()!, _ => builder.Rebuild(), false);
            if (____car.HasOverrideDestination(OverrideDestination.Repair)) {
                strip.AddButton("<sprite name=Copy><sprite name=Coupled>", () => {
                    ____car.TryGetOverrideDestination(OverrideDestination.Repair, OpsController.Shared!, out var result);
                    ____car.EnumerateCoupled(Car.End.F)!
                           .Where(o => o != ____car)
                           .Do(o => o.SetOverrideDestination(OverrideDestination.Repair, result));

                    builder.Rebuild();
                })!.Tooltip("Copy repair destination", "Copy this car's repair destination to the other cars in consist.");
            }
        });

        return false;
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CarInspector), nameof(CanCustomize))]
    private static bool CanCustomize(this CarInspector carInspector, out string reason) => throw new NotImplementedException("It's a stub: CarInspector.CanCustomize");

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CarInspector), nameof(ShowCustomize))]
    private static void ShowCustomize(this CarInspector carInspector) => throw new NotImplementedException("It's a stub: CarInspector.ShowCustomize");

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RepairTrack), nameof(CalculateRepairWorkOverall))]
    private static float CalculateRepairWorkOverall(Car car) => throw new NotImplementedException("It's a stub: RepairTrack.CalculateRepairWorkOverall");
}