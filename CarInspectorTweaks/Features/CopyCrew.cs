using System.Linq;
using Game.Messages;
using Game.State;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using UI.Builder;
using UI.CompanyWindow;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("CopyCrew")]
public static class CopyCrew
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuilderExtensions), nameof(AddTrainCrewDropdown), typeof(UIPanelBuilder), typeof(Car))]
    public static void AddTrainCrewDropdown(UIPanelBuilder builder, Car car, bool __result) {
        if (__result == false) {
            return;
        }

        builder.ButtonStrip(strip => {
            strip.AddButton("<sprite name=Copy><sprite name=Coupled>", () => {
                car.EnumerateCoupled(Car.End.F)!
                   .Where(o => o != car)
                   .Do(o => { StateManager.ApplyLocal(new SetCarTrainCrew(o.id, car.trainCrewId)); });
            })!.Tooltip("Copy crew", "Copy this car's crew to the other cars in consist.");
        });
    }
}
