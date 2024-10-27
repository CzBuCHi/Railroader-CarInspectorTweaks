using HarmonyLib;
using JetBrains.Annotations;
using Model;
using UI.Builder;
using UI.CarInspector;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("RememberTab")]
public static class RememberTab
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CarInspector), nameof(Populate))]
    public static void Populate(Car car, ref Car? ____car, ref UIState<string?> ____selectedTabState) {
        if (____car != null) {
            CarInspectorTweaksPlugin.Settings.TabStates[____car.Archetype] = ____selectedTabState.Value;
            CarInspectorTweaksPlugin.SaveSettings();
        }

        CarInspectorTweaksPlugin.Settings.TabStates.TryGetValue(car.Archetype, out var state);
        ____selectedTabState.Value = state;
        ____car = car;
    }
}
