using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Track;
using UI.Builder;
using UI.CarInspector;
using UI.Common;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("ToggleSwitch")]
public static class ToggleSwitch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateAIPanel))]
    public static void PopulateAIPanel(UIPanelBuilder builder, CarInspector __instance, Car ____car, Window ____window) {
        builder.AddField("Toggle switch",
            builder.ButtonStrip(strip => {
                strip.AddButton("in front", Execute(____car, true))!
                     .Tooltip("Toggle switch in front", "Toggles first switch in front of consist.");

                strip.AddButton("behind", Execute(____car, false))!
                     .Tooltip("Toggle switch behind", "Toggles first switch behind of consist.");
            })!
        );
    }

    private static Action Execute(Car car, bool front) {
        return () => {
            var node = FindSwitch(car, front);
            node.isThrown = !node.isThrown;
        };
    }

    private static TrackNode FindSwitch(Car car, bool front) {
        var coupledCars = car.EnumerateCoupled(front ? Car.End.F : Car.End.R)!.ToList();
        var start       = StartLocation(car, coupledCars, front);

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

    private static Location StartLocation(Car car, List<Car> coupledCarsCached, bool forward) {
        var logical = car.EndToLogical(forward ? Car.End.F : Car.End.R);
        var firstCar     = coupledCarsCached[0]!;
        if (logical == Car.LogicalEnd.A) {
            var locationA = firstCar.LocationA;
            return !locationA.IsValid ? firstCar.WheelBoundsA : locationA;
        }

        var locationB = firstCar.LocationB;
        return (locationB.IsValid ? locationB : firstCar.WheelBoundsB).Flipped();
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Graph), "SegmentsReachableFrom")]
    private static void SegmentsReachableFrom(this Graph __instance, TrackSegment segment, TrackSegment.End end, out TrackSegment normal, out TrackSegment reversed) =>
        throw new NotImplementedException("It's a stub: Graph.SegmentsReachableFrom");
}
