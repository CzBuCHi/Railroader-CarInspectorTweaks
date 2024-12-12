using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using UI.Builder;
using UI.CarInspector;

namespace CarInspectorTweaks.Features;

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("BleedAll")]
public static class BleedAllOld
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) {
        var codeInspector = AccessTools.Field(original.DeclaringType!, "<>4__this");
        if (codeInspector == null) {
            throw new Exception("Field " + original.DeclaringType!.FullName + "::<>4__this not found");
        }

        var car = AccessTools.Field("UI.CarInspector.CarInspector:_car");
        if (car == null) {
            throw new Exception("Field UI.CarInspector.CarInspector:_car not found");
        }

        var tooltip = AccessTools.Method("UI.Builder.IConfigurableElement:Tooltip", [typeof(string), typeof(string)]);
        if (tooltip == null) {
            throw new Exception("Method UI.Builder.IConfigurableElement:Tooltip not found");
        }

        var bleedAllCars = AccessTools.Method(typeof(BleedAll), nameof(BleedAllCars));
        if (bleedAllCars == null) {
            throw new Exception("Method CarInspectorTweaks.Features.BleedAll:BleedAllCars not found");
        }

        var addButton = AccessTools.Method("UI.Builder.UIPanelBuilder:AddButton");
        if (addButton == null) {
            throw new Exception("Method UI.Builder.UIPanelBuilder:AddButton not found");
        }

        var codeMatcher = new CodeMatcher(instructions, generator);
        codeMatcher.MatchStartForward(
                       CodeMatch.Calls(() => default(UIPanelBuilder).Spacer())
                   )
                   .ThrowIfInvalid("Could not find any call to UIPanelBuilder.HStack")
                   .Advance(-1)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarga_S, 1))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, "Bleed All"))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, codeInspector))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, car))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, bleedAllCars))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Call, addButton))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, "Bleed All"))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, "Bleeds all cars in consist."))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, tooltip))
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Pop));

        return codeMatcher.Instructions();
    }

    private static Action BleedAllCars(Car car) {
        return () => {
            car.EnumerateCoupled().Do(c => {
                if (c.SupportsBleed()) {
                    c.SetBleed();
                }
            });
        };
    }

    public static MethodBase TargetMethod() {
        var type = typeof(CarInspector).Assembly.GetType("UI.CarInspector.CarInspector+<>c__DisplayClass23_0");
        if (type == null) {
            throw new Exception("Type UI.CarInspector.CarInspector+<>c__DisplayClass23_0 not found.");
        }

        var targetMethod = AccessTools.Method(type, "<PopulateCarPanel>b__3");
        if (targetMethod == null) {
            throw new Exception("Method UI.CarInspector.CarInspector+<>c__DisplayClass23_0::b__3 not found.");
        }

        return targetMethod;
    }
}

[PublicAPI]
[HarmonyPatch]
[HarmonyPatchCategory("BleedAll")]
public static class BleedAll
{
    [HarmonyPostfix]
    public static void Postfix(UIPanelBuilder field, object __instance) {
        var closure      = Traverse.Create(__instance)!.Field("<>4__this")!.GetValue()!;
        var car          = Traverse.Create(closure)!.Field<Car>("_car")!.Value!;
        var bleedAllCars = BleedAllCars(car);
        field.AddButtonCompact("All", bleedAllCars)!
             .Tooltip("Bleed All Valves", "Bleed the brakes to release pressure from the train's brake system.");
    }

    private static Action BleedAllCars(Car car) {
        return () => car.EnumerateCoupled()!.Do(c => {
            if (c.SupportsBleed()) {
                c.SetBleed();
            }
        });
    }

    public static MethodBase TargetMethod() {
        var type = typeof(CarInspector).Assembly.GetType("UI.CarInspector.CarInspector+<>c__DisplayClass23_0");
        if (type == null) {
            throw new Exception("Type UI.CarInspector.CarInspector+<>c__DisplayClass23_0 not found.");
        }

        var targetMethod = AccessTools.Method(type, "<PopulateCarPanel>b__4");
        if (targetMethod == null) {
            throw new Exception("Method UI.CarInspector.CarInspector+<>c__DisplayClass23_0::b__4 not found.");
        }

        return targetMethod;
    }
}
