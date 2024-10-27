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
        var persistence = new AutoEngineerPersistence(____car.KeyValueObject!);
        var locomotive  = (BaseLocomotive)____car;
        var helper      = new AutoEngineerOrdersHelper(locomotive, persistence);
        var mode        = helper.Mode();

        if (mode == AutoEngineerMode.Road) {
            return;
        }

        builder.AddField("",
            builder.ButtonStrip(strip => {
                var cars = locomotive.EnumerateCoupled()!.ToList();

                cars.Do(car => {
                    strip.AddObserver(car.KeyValueObject!.Observe(PropertyChange.KeyForControl(PropertyChange.Control.Handbrake)!, _ => strip.Rebuild(), false)!);
                    strip.AddObserver(car.KeyValueObject.Observe(PropertyChange.KeyForControl(PropertyChange.Control.CylinderCock)!, _ => strip.Rebuild(), false)!);
                });

                if (cars.Any(c => c.air!.handbrakeApplied)) {
                    strip.AddButton($"Release {TextSprites.HandbrakeWheel}", () => {
                             ReleaseAllHandbrakes(cars);
                             strip.Rebuild();
                         })!
                         .Tooltip("Release handbrakes", $"Iterates over cars in this consist and releases {TextSprites.HandbrakeWheel}.");
                }

                strip.AddButton("Connect Air", () => {
                         ConnectAir(cars);
                         strip.Rebuild();
                     })!
                     .Tooltip("Connect Consist Air", "Iterates over each car in this consist and connects gladhands and opens anglecocks.");

                strip.AddButton("Oil all cars", () => {
                         OilAllCars(cars);
                         strip.Rebuild();
                     })!
                     .Tooltip("Oil all cars", "Iterates over each car in this consist and add little bit of oil to all boxes.");
            })!
        );
    }

    public static void ConnectAir(List<Car> consist) {
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

    public static void ReleaseAllHandbrakes(List<Car> consist) {
        consist.Do(c => c.SetHandbrake(false));
    }

    public static void OilAllCars(List<Car> consist) {
        consist.Do(c => c.OffsetOiled(0.01f));
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Car), "KeyValueKeyFor")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    private static string KeyValueKeyFor(Car.EndGearStateKey key, Car.End end) => throw new NotImplementedException("It's a stub: Car.KeyValueKeyFor");
}
