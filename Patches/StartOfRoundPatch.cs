using GameNetcodeStuff;
using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(StartOfRound))]

public class StartOfRoundPatch {
	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	private static void AutoRouteRandomDayOne(ref StartOfRound __instance){
		// Runs after loading a save file

		// Build list of selectable level IDs that are registered in the terminal (LLL compat)
		TerminalKeyword routeKeyword =  UnityObjectType.FindObjectOfType<Terminal>().terminalNodes.allKeywords[27];
		Helper.levels = [];
		RandomRouteOnly.Logger.LogDebug("Registered moons:");
		foreach(CompatibleNoun n in routeKeyword.compatibleNouns){
			if(n.result.terminalOptions != null && n.result.terminalOptions.Length > 1){
				int id = n.result.terminalOptions[1].result.buyRerouteToMoon;
				if(id != 3){
					RandomRouteOnly.Logger.LogDebug(n.noun.word + " | ID = " + id);
					Helper.levels.Add(id);
				}
			}
		}
		
		// Fly to company when the game starts and it's the final day (no auto landing so players can still join)
		if(TimeOfDay.Instance.daysUntilDeadline == 0) Helper.FlyToLevel(ref __instance, false, false);
		// Attempt to fly to random when the game starts, but only on the first day (and orbiting Experimentation) or if the ship is orbiting the company
		else if(__instance.gameStats.daysSpent == 0 && __instance.currentLevel.levelID == 0 || __instance.currentLevel.levelID == 3) Helper.FlyToLevel(ref __instance, true, false);

	}

	[HarmonyPatch("SetShipReadyToLand")]
	[HarmonyPostfix]
	private static void AutoRouteRandomNewDay(ref StartOfRound __instance){
		// Runs after going into orbit
		
		// Keep track of how many days we've been on the current moon for
		if(__instance.currentLevel.levelID != 3){
			if(Helper.previousLevel == __instance.currentLevel.levelID){
				Helper.daysOnLevel++;
			}else{
				Helper.previousLevel = __instance.currentLevel.levelID;
				Helper.daysOnLevel = 1;
			}
		}

		// Attempt to fly to a random moon unless it's the final day
		if(!(TimeOfDay.Instance.daysUntilDeadline == 0)){
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

	[HarmonyPatch("ChangeLevelServerRpc")]
	[HarmonyPrefix]
	private static void TrackLevelChanges(int levelID, int newGroupCreditsAmount, ref StartOfRound __instance){
		
		if(levelID == 3){
			// Memorize the moon we are on before going to the company
			Helper.previousLevel = __instance.currentLevel.levelID;
		}else{
			// Reset consecutive days counter when going somehwere new that isn't the company
			if(__instance.currentLevel.levelID != levelID && Helper.previousLevel != levelID){
				Helper.daysOnLevel = 0;
				Helper.previousLevel = levelID;
			}
		}
		
	}

}