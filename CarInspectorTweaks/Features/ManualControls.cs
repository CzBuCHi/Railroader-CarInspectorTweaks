using Game.Messages;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.AI;
using Model.Definition;
using UI.Builder;
using UI.CarInspector;
using UI.Common;
using UI.EngineControls;
using UnityEngine;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("ManualControls")]
public static class ManualControls
{
    private static Vector2? _UpdatedWindowSize;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(Show))]
    public static void Show(Car car) {
        var instance = Traverse.Create<CarInspector>()!.Field("_instance")!.GetValue<CarInspector>()!;
        var window   = Traverse.Create(instance)!.Field("_window")!.GetValue<Window>()!;

        if (_UpdatedWindowSize == null) {
            _UpdatedWindowSize = window.GetContentSize() + new Vector2(-2, 90);
        }

        window.SetContentSize(_UpdatedWindowSize!.Value);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CarInspector), nameof(Rebuild))]
    public static void Rebuild(Window ____window) {
        ____window.SetContentSize(_UpdatedWindowSize!.Value);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CarInspector), nameof(PopulateAIPanel))]
    public static void PopulateAIPanel(UIPanelBuilder builder, CarInspector __instance, Car ____car, Window ____window) {
        var persistence = new AutoEngineerPersistence(____car.KeyValueObject!);
        var locomotive  = (BaseLocomotive)____car;
        var helper      = new AutoEngineerOrdersHelper(locomotive, persistence);
        var mode        = helper.Mode;

        if (mode == AutoEngineerMode.Off) {
            AddManualControls(builder, locomotive);
        }
    }

    private static void AddManualControls(UIPanelBuilder builder, BaseLocomotive locomotive) {
        var locomotiveControl = locomotive.locomotiveControl!;
        var air               = locomotiveControl.air!;
        var isDiesel          = locomotive.Archetype == CarArchetype.LocomotiveDiesel;

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
    }
}