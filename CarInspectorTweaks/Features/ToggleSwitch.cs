using System;
using System.Collections.Generic;
using System.Linq;
using Game.Messages;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.AI;
using Track;
using UI.Builder;
using UI.CarInspector;
using UI.Common;
using UI.EngineControls;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("ToggleSwitch")]
public static class ToggleSwitch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateAIPanel))]
    public static void PopulateAIPanel(UIPanelBuilder builder, CarInspector __instance, Car ____car, Window ____window) {
        var persistence = new AutoEngineerPersistence(____car.KeyValueObject!);
        var locomotive  = (BaseLocomotive)____car;
        var helper      = new AutoEngineerOrdersHelper(locomotive, persistence);
        var mode        = helper.Mode();

        if (mode is AutoEngineerMode.Off or AutoEngineerMode.Yard) {
            AddToggleSwitchButtons(builder, locomotive);
        }
    }

    private static void AddToggleSwitchButtons(UIPanelBuilder builder, BaseLocomotive locomotive) {
        builder.AddField("Toggle switch",
            builder.ButtonStrip(strip => {
                strip.AddButton("in front", Execute(locomotive, true))!
                     .Tooltip("Toggle switch in front", "Toggles first switch in front of consist.");

                strip.AddButton("behind", Execute(locomotive, false))!
                     .Tooltip("Toggle switch behind", "Toggles first switch behind of consist.");
            })!
        );
    }

    private static Action Execute(BaseLocomotive locomotive, bool front) {
        return () => {
            var node = FindSwitch(locomotive, front);
            node.isThrown = !node.isThrown;
        };
    }

    private static TrackNode FindSwitch(BaseLocomotive locomotive, bool front) {
        var coupledCars = locomotive.EnumerateCoupled(front ? Car.End.F : Car.End.R)!.ToList();
        var start       = StartLocation(locomotive, coupledCars, front);

        var segment    = start.segment;
        var segmentEnd = start.EndIsA ? TrackSegment.End.B : TrackSegment.End.A;

        var graph = Graph.Shared!;

        TrackNode node;
        do {
            node = segment.NodeForEnd(segmentEnd)!;

            graph.SegmentsReachableFrom(segment, segmentEnd, out var segmentExitNormal, out _);
            segment = segmentExitNormal;
            segmentEnd = segment.NodeForEnd(TrackSegment.End.A)!.id == node.id ? TrackSegment.End.B : TrackSegment.End.A;
        } while (!graph.IsSwitch(node));

        return node;
    }

    private static Location StartLocation(BaseLocomotive locomotive, List<Car> coupledCarsCached, bool forward) {
        var logical = locomotive.EndToLogical(forward ? Car.End.F : Car.End.R);
        var car     = coupledCarsCached[0]!;
        if (logical == Car.LogicalEnd.A) {
            var locationA = car.LocationA;
            return !locationA.IsValid ? car.WheelBoundsA : locationA;
        }

        var locationB = car.LocationB;
        return (locationB.IsValid ? locationB : car.WheelBoundsB).Flipped();
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Graph), "SegmentsReachableFrom")]
    private static void SegmentsReachableFrom(this Graph __instance, TrackSegment segment, TrackSegment.End end, out TrackSegment normal, out TrackSegment reversed) =>
        throw new NotImplementedException("It's a stub: Graph.SegmentsReachableFrom");
}
