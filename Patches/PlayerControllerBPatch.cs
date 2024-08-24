using GameNetcodeStuff;
using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]

public class PlayerControllerBPatch {

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    private static void AddRerollValue(PlayerControllerB __instance) {
        Rerolls cr = __instance.gameObject.AddComponent<Rerolls>();
    }
}