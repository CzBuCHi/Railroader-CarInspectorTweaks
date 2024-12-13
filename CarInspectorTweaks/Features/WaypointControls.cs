using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Game.Messages;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using UI;
using UI.Builder;
using UI.CarInspector;
using UI.EngineControls;

namespace CarInspectorTweaks.Features;

internal static class WaypointControlsUtils
{
    public static readonly Action<object, AutoEngineerMode?, bool?, int?, float?> SetOrdersValue;

    static WaypointControlsUtils() {
        var type = typeof(CarInspector).Assembly.GetType("UI.CarInspector.CarInspector+<>c__DisplayClass49_0");
        if (type == null) {
            throw new Exception("Type UI.CarInspector.CarInspector+<>c__DisplayClass49_0 not found.");
        }

        var targetMethod = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
                               .OfType<MethodInfo>()
                               .FirstOrDefault(o => o.Name == "<PopulateAIPanel>b__15");
        if (targetMethod == null) {
            throw new Exception("Method UI.CarInspector.CarInspector+<>c__DisplayClass49_0::b__15 not found.");
        }

        var setOrdersValue = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic).OfType<MethodInfo>().FirstOrDefault(o => o.Name == "<PopulateAIPanel>g__SetOrdersValue|0");
        if (setOrdersValue == null) {
            throw new Exception("Method UI.CarInspector.CarInspector+<>c__DisplayClass49_0::<PopulateAIPanel>g__SetOrdersValue|0 not found.");
        }

        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var modeParam     = Expression.Parameter(typeof(AutoEngineerMode?), "mode");
        var forwardParam  = Expression.Parameter(typeof(bool?), "forward");
        var maxSpeedParam = Expression.Parameter(typeof(int?), "maxSpeedMph");
        var distanceParam = Expression.Parameter(typeof(float?), "distance");

        var instanceCast = Expression.Convert(instanceParam, setOrdersValue.DeclaringType!);

        var methodCall = Expression.Call(instanceCast, setOrdersValue, modeParam, forwardParam, maxSpeedParam, distanceParam);

        var lambda = Expression.Lambda<Action<object, AutoEngineerMode?, bool?, int?, float?>>(
            methodCall, instanceParam, modeParam, forwardParam, maxSpeedParam, distanceParam);

        SetOrdersValue = lambda.Compile();
    }
}

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("WaypointControls")]
public static class WaypointControlsWp
{
    [HarmonyPostfix]
    public static void Postfix(UIPanelBuilder builder, object __instance) {
        var mode = Traverse.Create(__instance)!.Field<AutoEngineerMode>("mode")!.Value;

        builder.AddButtonSelectable("WP", mode == AutoEngineerMode.Waypoint, () => WaypointControlsUtils.SetOrdersValue(__instance, AutoEngineerMode.Waypoint, null, null, null));
    }

    public static MethodBase TargetMethod() {
        var type = typeof(CarInspector).Assembly.GetType("UI.CarInspector.CarInspector+<>c__DisplayClass49_0");
        if (type == null) {
            throw new Exception("Type UI.CarInspector.CarInspector+<>c__DisplayClass49_0 not found.");
        }

        var targetMethod = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
                               .OfType<MethodInfo>()
                               .FirstOrDefault(o => o.Name == "<PopulateAIPanel>b__2");
        if (targetMethod == null) {
            throw new Exception("Method UI.CarInspector.CarInspector+<>c__DisplayClass49_0::b__2 not found.");
        }

        return targetMethod;
    }
}

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("WaypointControls")]
public static class WaypointControlsInf
{
    [HarmonyPostfix]
    public static void Postfix(UIPanelBuilder builder, object __instance) {
        builder.AddButton("INF", () => WaypointControlsUtils.SetOrdersValue(__instance, null, null, null, 1000000f));
    }

    public static MethodBase TargetMethod() {
        var type = typeof(CarInspector).Assembly.GetType("UI.CarInspector.CarInspector+<>c__DisplayClass49_0");
        if (type == null) {
            throw new Exception("Type UI.CarInspector.CarInspector+<>c__DisplayClass49_0 not found.");
        }

        var targetMethod = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
                               .OfType<MethodInfo>()
                               .FirstOrDefault(o => o.Name == "<PopulateAIPanel>b__15");

        if (targetMethod == null) {
            throw new Exception("Method UI.CarInspector.CarInspector+<>c__DisplayClass49_0::b__15 not found.");
        }

        return targetMethod;
    }
}

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("WaypointControls")]
public static class WaypointControlsWpControls
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CarInspector), "PopulateAIPanel")]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) {
        var car          = AccessTools.Field(typeof(CarInspector), "_car");
        var type         = typeof(CarInspector).Assembly.GetType("UI.CarInspector.CarInspector+<>c__DisplayClass49_0")!;
        var helperField  = AccessTools.Field(type, "helper");
        var builderField = AccessTools.Field(type, "builder");
        var modeField    = AccessTools.Field(type, "mode");

        var buildWaypointControls      = AccessTools.Method(typeof(WaypointControlsWpControls), nameof(BuildWaypointControls));
        var addExpandingVerticalSpacer = AccessTools.Method(typeof(UIPanelBuilder), nameof(UIPanelBuilder.AddExpandingVerticalSpacer));

        var codeMatcher = new CodeMatcher(instructions, generator);

        // remove 'Direction' from 'Waypoint' mode
        codeMatcher.MatchStartForward(
                       new CodeMatch(OpCodes.Ldstr, "Direction")
                   )
                   ?.ThrowIfInvalid("Could not find 'Direction'")!
                   .Advance(-1);

        var labelSkip = generator.DefineLabel();

        codeMatcher.InsertAndAdvance(
                       new CodeInstruction(OpCodes.Ldfld, modeField),
                       new CodeInstruction(OpCodes.Ldc_I4_4),
                       new CodeInstruction(OpCodes.Bne_Un_S, labelSkip),
                       new CodeInstruction(OpCodes.Ldloc_0)
                   )
                   .Advance(11)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { labelSkip } });

        // add controls to 'Waypoint' mode
        codeMatcher.End()
                   .MatchStartBackwards(
                       new CodeMatch(OpCodes.Ldloc_0),
                       new CodeMatch(OpCodes.Ldflda, builderField),
                       new CodeMatch(OpCodes.Call, addExpandingVerticalSpacer)
                   )
                   ?.ThrowIfInvalid("Could not find any call to UIPanelBuilder.AddExpandingVerticalSpacer");

        var label2Instruction = codeMatcher.InstructionAt(0);
        if (label2Instruction.labels.Count == 0) {
            throw new Exception("Could not find correct call to UIPanelBuilder.AddExpandingVerticalSpacer");
        }

        var label2 = label2Instruction.labels[0];
        label2Instruction.labels.Clear();

        codeMatcher
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Nop) { labels = [label2] },
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldfld, builderField),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, car),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldfld, helperField),
                new CodeInstruction(OpCodes.Call, buildWaypointControls)
            );
        return codeMatcher.Instructions();
    }

    public static void BuildWaypointControls(UIPanelBuilder builder, Car car, AutoEngineerOrdersHelper helper) {
        builder.AddField("Waypoints",
            builder.ButtonStrip(strip => {
                strip.AddButton("Set Waypoint", () => AutoEngineerDestinationPicker.Shared!.StartPickingLocation((BaseLocomotive)car, helper))!
                     .Tooltip("Choose Waypoint", "Click to Choose Destination Waypoint");
                strip.AddButton("Clear", helper.ClearWaypoint)!
                     .Tooltip("Clear Waypoint", "Click to remove Destination Waypoint\"");
            })!
        );
    }
}
