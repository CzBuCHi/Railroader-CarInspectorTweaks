using HarmonyLib;
using JetBrains.Annotations;
using Model;
using UI.Builder;
using UI.CarInspector;
using UnityEngine;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("ShowCarOil")]
public static class ShowCarOil
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CarInspector), "PopulateCarPanel")]
    public static void PopulateCarPanelPrefix(UIPanelBuilder builder, Car ____car) {
        if (!____car.EnableOiling) {
            return;
        }

        builder.AddField("Oil", () => Mathf.RoundToInt(____car.Oiled * 100) + "%", UIPanelBuilder.Frequency.Periodic);
    }
}
