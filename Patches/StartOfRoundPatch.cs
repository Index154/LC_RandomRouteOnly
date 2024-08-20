using GameNetcodeStuff;
using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(StartOfRound))]

public class StartOfRoundPatch {
	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	private static void AutoRouteRandomDayOne(ref StartOfRound __instance){
		if(__instance.gameStats.daysSpent == 0) Helper.FlyToLevel(ref __instance, true);
	}

	[HarmonyPatch("SetShipReadyToLand")]
	[HarmonyPostfix]
	private static void AutoRouteRandomNewDay(ref StartOfRound __instance){

		bool isFinalDay = TimeOfDay.Instance.daysUntilDeadline == 0;

		if(!isFinalDay){
			Helper.FlyToLevel(ref __instance, true);
		}else{
			// Reset player rerolls on the final day
			foreach(PlayerControllerB player in __instance.allPlayerScripts){
                CanReroll cr = player.gameObject.GetComponent<CanReroll>();
                cr.canReroll = true;
            }
			Helper.FlyToLevel(ref __instance, false);
		}
	}

}