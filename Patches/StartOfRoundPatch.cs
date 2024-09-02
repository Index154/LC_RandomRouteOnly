using GameNetcodeStuff;
using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(StartOfRound))]

public class StartOfRoundPatch {
	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	private static void AutoRouteRandomDayOne(ref StartOfRound __instance){
		// Fly to random when the game starts, but only on the first day or if the ship is orbiting the company
		// Fly to company when the game starts and it's the final day
		if(TimeOfDay.Instance.daysUntilDeadline == 0) Helper.FlyToLevel(ref __instance, false, false);
		else if(__instance.gameStats.daysSpent == 0 && __instance.currentLevel.levelID == 0 || __instance.currentLevel.levelID == 3) Helper.FlyToLevel(ref __instance, true, false);

	}

	[HarmonyPatch("SetShipReadyToLand")]
	[HarmonyPostfix]
	private static void AutoRouteRandomNewDay(ref StartOfRound __instance){
		
		bool isFinalDay = TimeOfDay.Instance.daysUntilDeadline == 0;

		if(!isFinalDay){
			// Fly to random after going into orbit, unless it's the final day
			Helper.FlyToLevel(ref __instance, true, false);
		}else{
			// Reset player rerolls on the final day
			foreach(PlayerControllerB player in __instance.allPlayerScripts){
                Rerolls cr = player.gameObject.GetComponent<Rerolls>();
                cr.rerolls = RandomRouteOnly.configManager.rerollsPerPlayer.Value;
            }
			// Fly to company on the final day
			Helper.FlyToLevel(ref __instance, false, true);
		}
	}

}