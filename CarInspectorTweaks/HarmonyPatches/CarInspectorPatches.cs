using System;
using System.Linq;
using CarInspectorResizer.Behaviors;
using CarInspectorTweaks.Extensions;
using Game.Messages;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.AI;
using Model.Definition;
using Model.OpsNew;
using Model.Physics;
using UI.Builder;
using UI.CarInspector;
using UI.Common;
using UI.EngineControls;
using UnityEngine;

namespace CarInspectorTweaks.HarmonyPatches;

using Car = Car;

[PublicAPI]
[HarmonyPatch]
public static class CarInspectorPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(Populate))]
    public static void Populate(ref Window ____window) {
        var windowAutoHeight = ____window.gameObject!.GetComponent<CarInspectorAutoHeightBehavior>()!;
        windowAutoHeight.ExpandOrders(AutoEngineerMode.Off, 50);
        windowAutoHeight.ExpandTab("equipment", 30);
    }

    #region remember last selected tab when selecting new car

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

    #endregion

    #region add manual controls to ordders tab

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateAIPanel))]
    public static void PopulateAIPanel(UIPanelBuilder builder, CarInspector __instance, Car ____car, Window ____window) {
        var persistence = new AutoEngineerPersistence(____car.KeyValueObject!);
        var locomotive = (BaseLocomotive)____car;
        var helper = new AutoEngineerOrdersHelper(locomotive, persistence);
        var mode = helper.Mode();

        if (mode == AutoEngineerMode.Off) {
            AddManualControls(builder, locomotive, __instance, ____car);
        }

        builder.AddExpandingVerticalSpacer();
    }

    private static void AddManualControls(UIPanelBuilder builder, BaseLocomotive locomotive, CarInspector __instance, Car ____car) {
        var locomotiveControl = locomotive.locomotiveControl!;
        var air = locomotiveControl.air!;
        var isDiesel = locomotive.Archetype == CarArchetype.LocomotiveDiesel;

        var throttle = builder.AddSliderQuantized(() => locomotiveControl.AbstractThrottle,
            () => {
                var value = isDiesel ? Mathf.RoundToInt(locomotiveControl.AbstractThrottle * 8f) : (float)Mathf.RoundToInt(locomotiveControl.AbstractThrottle * 100f);
                return value.ToString("0");
            },
            o => locomotiveControl.AbstractThrottle = o, 0.01f, 0, 1,
            o => locomotiveControl.AbstractThrottle = o)!;

        var reverser = builder.AddSliderQuantized(() => locomotiveControl.AbstractReverser,
            () => locomotiveControl.AbstractReverser != 0 ? (locomotiveControl.AbstractReverser * 100).ToString("0") : "N",
            o => locomotiveControl.AbstractReverser = o, 0.05f, -1, 1,
            o => locomotiveControl.AbstractReverser = o)!;

        var locomotiveBrake = builder.AddSliderQuantized(() => locomotiveControl.LocomotiveBrakeSetting,
            () => Mathf.RoundToInt(air.BrakeCylinder!.Pressure).ToString("0"),
            o => locomotiveControl.LocomotiveBrakeSetting = o, 0.01f, 0, 1,
            o => locomotiveControl.LocomotiveBrakeSetting = o)!;

        var trainBrake = builder.AddSliderQuantized(() => locomotiveControl.TrainBrakeSetting,
            () => Mathf.RoundToInt(air.BrakeLine!.Pressure).ToString("0"),
            o => locomotiveControl.TrainBrakeSetting = o, 0.01f, 0, 1,
            o => locomotiveControl.TrainBrakeSetting = o)!;

        builder.AddField("Speed", () => {
            var velocityMphAbs = locomotive.VelocityMphAbs;
            velocityMphAbs = velocityMphAbs >= 1.0 ? Mathf.RoundToInt(velocityMphAbs) : velocityMphAbs > 0.10000000149011612 ? 1f : 0.0f;
            return velocityMphAbs + " MPH";
        }, UIPanelBuilder.Frequency.Periodic);
        builder.AddField("Throttle", throttle);
        builder.AddField("Reverser", reverser);
        builder.AddField("Independent", locomotiveBrake);
        builder.AddField("Train Brake", trainBrake);
        builder.AddField("",
            builder.ButtonStrip(strip => {
                var cars = locomotive.EnumerateCoupled()!.ToList()!;

                if (cars.Any(c => c.air!.handbrakeApplied)) {
                    strip.AddButton($"Release {TextSprites.HandbrakeWheel}", () => {
                             Utility.ReleaseAllHandbrakes(cars);
                             strip.Rebuild();
                         })!
                         .Tooltip("Release handbrakes", $"Iterates over cars in this consist and releases {TextSprites.HandbrakeWheel}.");
                }

                if (cars.Any(c => c.EndAirSystemIssue())) {
                    strip.AddButton("Connect Air", () => {
                             Utility.ConnectAir(cars);
                             strip.Rebuild();
                         })!
                         .Tooltip("Connect Consist Air", "Iterates over each car in this consist and connects gladhands and opens anglecocks.");
                }
            })!
        );
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CarInspector), nameof(SelectConsist))]
    public static void SelectConsist(CarInspector __instance) {
    }

    #endregion

    #region copy repair destination to rest of consist

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateEquipmentPanel))]
    public static void PopulateEquipmentPanel(UIPanelBuilder builder, Car ____car) {
        ____car.KeyValueObject!.Observe(OverrideDestination.Repair.Key()!, _ => builder.Rebuild(), false);
        if (____car.HasOverrideDestination(OverrideDestination.Repair)) {
            builder.ButtonStrip(strip => {
                strip.AddButton("<sprite name=Copy><sprite name=Coupled>", () => {
                    ____car.TryGetOverrideDestination(OverrideDestination.Repair, OpsController.Shared!, out var result);
                    ____car.EnumerateCoupled(Car.End.F)!
                           .Where(o => o != ____car)
                           .Do(o => { o.SetOverrideDestination(OverrideDestination.Repair, result); });

                    builder.Rebuild();
                })!.Tooltip("Copy repair destination", "Copy this car's repair destination to the other cars in consist.");
            });
        }
    }

    #endregion

    #region add car speed to to car tab

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateCarPanel))]
    public static void PopulateCarPanel(UIPanelBuilder builder, Car ____car) {
        if (____car.Archetype == CarArchetype.LocomotiveDiesel ||
            ____car.Archetype == CarArchetype.LocomotiveSteam) {
            return;
        }

        builder.AddField("Speed", () => {
            var velocityMphAbs = ____car.VelocityMphAbs;
            velocityMphAbs = velocityMphAbs >= 1.0 ? Mathf.RoundToInt(velocityMphAbs) : velocityMphAbs > 0.10000000149011612 ? 1f : 0.0f;
            return velocityMphAbs + " MPH";
        }, UIPanelBuilder.Frequency.Periodic);
    }

    #endregion

    #region strong man max speed changed to 5MPH

    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntegrationSet), nameof(AddVelocityToCar))]
    public static void AddVelocityToCar(Car car, float velocity, ref float maxVelocity) {
        if (Math.Abs(maxVelocity - 1.34111786f) < 0.00000001f) {
            maxVelocity = 1.34111786f * 2;
        }
    }

    #endregion
}
