using System.Collections.Generic;
using System.Linq;
using CarInspectorTweaks.HarmonyPatches;
using Game.Messages;
using Game.State;
using HarmonyLib;
using Model;
using Serilog;
using Serilog.Events;
using Track;

namespace CarInspectorTweaks;

public static class Utility
{
    public static void ConnectAir(List<Car> consist) {
        foreach (var car in consist) {
            ConnectAirCore(car, Car.LogicalEnd.A);
            ConnectAirCore(car, Car.LogicalEnd.B);
        }

        return;

        static void ConnectAirCore(Car car, Car.LogicalEnd end) {
            StateManager.ApplyLocal(new PropertyChange(car.id, CarPatches.KeyValueKeyFor(Car.EndGearStateKey.Anglecock, car.LogicalToEnd(end)), new FloatPropertyValue(car[end].IsCoupled ? 1f : 0f)));

            if (car.TryGetAdjacentCar(end, out var car2)) {
                StateManager.ApplyLocal(new SetGladhandsConnected(car.id, car2.id, true));
            }
        }
    }

    public static void ReleaseAllHandbrakes(List<Car> consist) {
        consist.Do(c => c.SetHandbrake(false));
    }

    public static void BleedAll(List<Car> consist) {
        consist.Do(c => {
            if (c.SupportsBleed()) {
                c.SetBleed();
            }
        });
    }

    public static void ToggleSwitch(BaseLocomotive locomotive, bool front) {
        var node = FindSwitch(locomotive, front);
        node.isThrown = !node.isThrown;
    }

    private static TrackNode FindSwitch(BaseLocomotive locomotive, bool front) {
        Log.Logger.Write(LogEventLevel.Information, "FindSwitch start");

        var coupledCars = locomotive.EnumerateCoupled(front ? Car.End.F : Car.End.R)!.ToList();
        var start       = StartLocation(locomotive, coupledCars, front);

        var segment    = start.segment;
        var segmentEnd = start.EndIsA ? TrackSegment.End.B : TrackSegment.End.A;

        var graph = Graph.Shared!;

        TrackNode node;
        do {
            node = segment.NodeForEnd(segmentEnd)!;
            Log.Logger.Write(LogEventLevel.Information, "got node " + node.id);

            graph.SegmentsReachableFrom(segment, segmentEnd, out var segmentExitNormal, out _);
            segment = segmentExitNormal;
            segmentEnd = segment.NodeForEnd(TrackSegment.End.A)!.id == node.id ? TrackSegment.End.B : TrackSegment.End.A;
        } while (!graph.IsSwitch(node));

        Log.Logger.Write(LogEventLevel.Information, "got switch " + node.id);
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
}
