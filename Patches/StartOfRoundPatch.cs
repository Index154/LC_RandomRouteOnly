using System.Linq;
using UnityEngine;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(StartOfRound))]

public class StartOfRoundPatch {
	static IEnumerator DelayRerouteOnStart(StartOfRound __instance, float delay, bool randomLevel){
        yield return new WaitForSeconds(delay);
		Helper.FlyToLevel(ref __instance, randomLevel, false);
    }

	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	private static void AutoRouteRandomOnStart(ref StartOfRound __instance){
		// Runs after loading a save file
		Helper.Prepare(ref __instance);
		// Fly to company when the game starts and it's the final day (no auto landing so players can still join)
		if(TimeOfDay.Instance.daysUntilDeadline == 0) {
			__instance.StartCoroutine(DelayRerouteOnStart(__instance, 7f, false));
			RandomRouteOnly.Logger.LogInfo("Routing to company after loading save");
		}
		// Attempt to fly to random if the ship is orbiting the company
		else if(__instance.currentLevel.levelID == 3) {
			__instance.StartCoroutine(DelayRerouteOnStart(__instance, 7f, true));
			RandomRouteOnly.Logger.LogInfo("Leaving company after loading save");
		}
	}

	[HarmonyPatch("PlayFirstDayShipAnimation")]
	[HarmonyPostfix]
	private static void AutoRouteRandomDayOne(ref StartOfRound __instance){
		// Runs on day 1 of a save file (around when the speaker audio starts playing)
		Helper.Prepare(ref __instance);
		RandomRouteOnly.Logger.LogInfo("Day 1 reroute");
		if(__instance.currentLevel.levelID == 0) Helper.FlyToLevel(ref __instance, true, false);
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

		// We want only the host to run this
		if(__instance.NetworkManager.IsHost){
			
			if(levelID == 3){
				// Memorize the moon we are on before going to the company
				Helper.previousLevel = __instance.currentLevel.levelID;
			}else{
				// Update the list of recently routed to levels - Compare previousLevel since this code would otherwise be run twice
				if(RandomRouteOnly.configManager.noRepeatCount.Value != 0 && Helper.previousLevel != levelID){
					Helper.recentLevels.Add(levelID);

					if(RandomRouteOnly.configManager.noRepeatCount.Value < Helper.recentLevels.Count){

						if(RandomRouteOnly.configManager.noRepeatCount.Value == -1){
							// -1 = Never remove old entries. The list will be reset by the Helper once there's no new moon to route to anymore (or by routerandom-redexed)
						}else{
							// Remove oldest level to stay within the configured max number of moons to remember
							Helper.recentLevels.RemoveAt(0);
						}
					}

					RandomRouteOnly.Logger.LogInfo("Recently visited levels list:");
					foreach(int id in Helper.recentLevels){
						SelectableLevel lvl = __instance.levels.Where(i => i.levelID == id).FirstOrDefault();
						RandomRouteOnly.Logger.LogInfo(lvl.name);
					}
				}

				// Reset consecutive days counter when going somehwere new that isn't the company
				if(__instance.currentLevel.levelID != levelID && Helper.previousLevel != levelID){
					Helper.daysOnLevel = 0;
					Helper.previousLevel = levelID;
				}
			}
		}
	}
}