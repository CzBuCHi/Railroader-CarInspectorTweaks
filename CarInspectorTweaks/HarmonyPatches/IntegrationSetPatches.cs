using System;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using Model.Physics;

namespace CarInspectorTweaks.HarmonyPatches;

[PublicAPI]
[HarmonyPatch]
public static class IntegrationSetPatches
{
    #region strong man max speed changed to 6MPH

    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntegrationSet), nameof(AddVelocityToCar))]
    public static void AddVelocityToCar(Car car, float velocity, ref float maxVelocity) {
        if (Math.Abs(maxVelocity - 1.34111786f) < 0.00000001f) {
            maxVelocity = 1.34111786f * 2;
        }
    }

    #endregion
}
