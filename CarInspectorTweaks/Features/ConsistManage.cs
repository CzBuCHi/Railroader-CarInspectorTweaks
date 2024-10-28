using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Game.Messages;
using Game.State;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.AI;
using UI.Builder;
using UI.CarInspector;
using UI.Common;
using UI.EngineControls;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("ConsistManage")]
public static class ConsistManage
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateAIPanel))]
    public static void PopulateAIPanel(UIPanelBuilder builder, CarInspector __instance, Car ____car, Window ____window) {
        builder.ButtonStrip(strip => {
            var cars = ____car.EnumerateCoupled()!.ToList();

            cars.Do(car => {
                strip.AddObserver(car.KeyValueObject!.Observe(PropertyChange.KeyForControl(PropertyChange.Control.Handbrake)!, _ => strip.Rebuild(), false)!);
                strip.AddObserver(car.KeyValueObject.Observe(PropertyChange.KeyForControl(PropertyChange.Control.CylinderCock)!, _ => strip.Rebuild(), false)!);
                strip.AddObserver(car.KeyValueObject.Observe("oiled", _ => strip.Rebuild(), false)!);
            });

            if (cars.Any(c => c.air!.handbrakeApplied)) {
                strip.AddButton($"Release {TextSprites.HandbrakeWheel}", () => {
                         ReleaseAllHandbrakes(cars);
                         strip.Rebuild();
                     })!
                     .Tooltip("Release handbrakes", $"Iterates over cars in this consist and releases {TextSprites.HandbrakeWheel}.");
            }

            if (!IsAirConnected(cars)) {
                strip.AddButton("Connect Air", () => {
                         ConnectAir(cars);
                         strip.Rebuild();
                     })!
                     .Tooltip("Connect Consist Air", "Iterates over each car in this consist and connects gladhands and opens anglecocks.");

            }

            if (cars.Any(o => o.NeedsOiling)) {
                strip.AddButton("Oil all cars", () => {
                         OilAllCars(cars);
                         strip.Rebuild();
                     })!
                     .Tooltip("Oil all cars", "Iterates over each car in this consist and add oil to all boxes.");
            }
        });
    }

    private static bool IsAirConnected(List<Car> consist) {
        var result = true;
        foreach (var car in consist) {
            CheckAir(car, car.EndGearA!, Car.LogicalEnd.A);
            CheckAir(car, car.EndGearB!, Car.LogicalEnd.B);
        }

        return result;

        void CheckAir(Car car, Car.EndGear endGear, Car.LogicalEnd end) {
            if (car.CoupledTo(end)) {
                if (car.AirConnectedTo(end) == null || endGear.AnglecockSetting < 0.999f) {
                    result = false;
                }
            } else {
                if (endGear.AnglecockSetting > 0.001f) {
                    result = false;
                }
            }
        }
    }

    private static void ConnectAir(List<Car> consist) {
        foreach (var car in consist) {
            ConnectAirCore(car, Car.LogicalEnd.A);
            ConnectAirCore(car, Car.LogicalEnd.B);
        }

        return;

        static void ConnectAirCore(Car car, Car.LogicalEnd end) {
            StateManager.ApplyLocal(new PropertyChange(car.id!, KeyValueKeyFor(Car.EndGearStateKey.Anglecock, car.LogicalToEnd(end)), new FloatPropertyValue(car[end]!.IsCoupled ? 1f : 0f)));

            if (car.TryGetAdjacentCar(end, out var car2)) {
                StateManager.ApplyLocal(new SetGladhandsConnected(car.id!, car2!.id!, true));
            }
        }
    }

    private static void ReleaseAllHandbrakes(List<Car> consist) {
        consist.Do(c => c.SetHandbrake(false));
    }

    private static void OilAllCars(List<Car> consist) {
        consist.Do(c => c.OffsetOiled(1f));
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Car), "KeyValueKeyFor")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    private static string KeyValueKeyFor(Car.EndGearStateKey key, Car.End end) => throw new NotImplementedException("It's a stub: Car.KeyValueKeyFor");
}
