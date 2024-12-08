using System;
using CarInspectorTweaks.Extensions;
using Game.Messages;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.AI;
using UI;
using UI.Builder;
using UI.CarInspector;
using UI.EngineControls;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("WaypointControls")]
public static class WaypointControls
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CarInspector), "PopulateAIPanel")]
    public static void PopulateAIPanelPrefix(UIPanelBuilder builder, out int __state) {
        __state = builder.GetFieldCount();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), "PopulateAIPanel")]
    public static void PopulateAIPanelPostfix(UIPanelBuilder builder, Car ____car, int __state) {
        var persistence = new AutoEngineerPersistence(____car.KeyValueObject!);
        var helper      = new AutoEngineerOrdersHelper(____car, persistence);
        var mode        = helper.Mode;

        builder.ReplaceField(__state,
            builder.ButtonStrip(strip => {
                strip.AddButtonSelectable("Manual", mode == AutoEngineerMode.Off, () => helper.SetOrdersValue(AutoEngineerMode.Off));
                strip.AddButtonSelectable("Road", mode == AutoEngineerMode.Road, () => helper.SetOrdersValue(AutoEngineerMode.Road));
                strip.AddButtonSelectable("Yard", mode == AutoEngineerMode.Yard, () => helper.SetOrdersValue(AutoEngineerMode.Yard));
                strip.AddButtonSelectable("WP", mode == AutoEngineerMode.Waypoint, () => helper.SetOrdersValue(AutoEngineerMode.Waypoint));
            })!
        );
        
        if (!persistence.Orders.Enabled) {
            return;
        }

        switch (mode) {
            case AutoEngineerMode.Off:
                break;

            case AutoEngineerMode.Road:
                break;

            case AutoEngineerMode.Yard:
                builder.ReplaceField(__state + 2,
                    builder.ButtonStrip(strip => {
                        strip.AddButton("Stop", () => helper.SetOrdersValue(distance: 0.0f));
                        strip.AddButton("\u00BD", () => helper.SetOrdersValue(distance: 6.1f));
                        strip.AddButton("1", () => helper.SetOrdersValue(distance: 12.2f));
                        strip.AddButton("2", () => helper.SetOrdersValue(distance: 24.4f));
                        strip.AddButton("5", () => helper.SetOrdersValue(distance: 61f));
                        strip.AddButton("10", () => helper.SetOrdersValue(distance: 122f));
                        strip.AddButton("20", () => helper.SetOrdersValue(distance: 244f));
                        strip.AddButton("inf", () => helper.SetOrdersValue(distance: 1000000f));
                    }, 4)!
                );
                break;

            case AutoEngineerMode.Waypoint:
                builder.ReplaceField(__state + 1,
                    builder.ButtonStrip(strip => {
                        strip.AddButton("Waypoint", () => AutoEngineerDestinationPicker.Shared!.StartPickingLocation((BaseLocomotive)____car, helper))!
                             .Tooltip("Choose Waypoint", "Click to Choose Destination Waypoint");
                        strip.AddButton("Clear", () => helper.ClearWaypoint())!
                             .Tooltip("Clear Waypoint", "Click to remove Destination Waypoint\"");
                    })!
                );
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

    }

}
