using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RandomRouteOnly;

public class Helper(){

    public static List<SelectableLevel> levels = [];
    public static bool constellationsLoaded = false;
    public static int daysOnLevel = 0;
    public static int previousLevel = -1;

    static IEnumerator DelayStartGame(float delay){
        yield return new WaitForSeconds(delay);
        // Land the ship. This is the proper way of doing it to avoid issues
        StartMatchLever lever = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
        lever.StartGame();
    }
    public static int GetRandomLevel(ref StartOfRound __instance){
        bool isFirstDay = (__instance.gameStats.daysSpent == 0) && __instance.currentLevel.levelID == 0;
        var possibleLevels = new List<int>();

        foreach(SelectableLevel level in levels){
            bool addLevel = true;
            // Exclude current level (unless it's day 1, otherwise day 1 will never be Experimentation)
            if(level == __instance.currentLevel && !isFirstDay) addLevel = false;
            // Exclude levels not in the current constellation
            if(constellationsLoaded && RandomRouteOnly.configManager.constellations.Value){
                RandomRouteOnly.Logger.LogDebug("Constellation support: Removing moon from random level selection");
                if(!ConstellationsCompat.IsLevelInConstellation(level)) addLevel = false;
            }
            
            if(addLevel) possibleLevels.Add(level.levelID);
        }
        return possibleLevels[Random.Range(0, possibleLevels.Count)];
    }
    public static void FlyToLevel(ref StartOfRound __instance, bool randomLevel, bool autoLand){
        // StartOfRound is run by clients too so stop those here
		if(!__instance.NetworkManager.IsHost) return;

		bool isFirstDay = (__instance.gameStats.daysSpent == 0) && __instance.currentLevel.levelID == 0;
        bool maxDaysReached = false;
		var possibleLevels = new List<int>();
        // 3 is default for flying to company on the final day
        int newLevelId = 3;

        // Only fly to random if the maximum number of days per moon has been reached
        if(daysOnLevel >= RandomRouteOnly.configManager.stayOnMoonDays.Value) maxDaysReached = true;
        // Change newLevelId to something other than company to prevent the ship flying there when unintended
        if(randomLevel) newLevelId = 0;

        // Return to the previous moon after leaving the company unless we already spent the maximum amount of days there
        if(__instance.currentLevel.levelID == 3 && !maxDaysReached) newLevelId = previousLevel;

        // Pick random target if necessary
        if((randomLevel && maxDaysReached) || isFirstDay || newLevelId == -1) newLevelId = GetRandomLevel(ref __instance);

        // Prevent auto routing if the maximum number of days hasn't been reached yet (unless orbiting company or going to company or on day 0)
        if(!maxDaysReached && newLevelId != 3 && __instance.currentLevel.levelID != 3 && !isFirstDay){
            RandomRouteOnly.Logger.LogDebug("Maximum days per moon not reached - Aborting reroute");
            return;
        }

        // Fly to selected level if possible
		if(__instance.CanChangeLevels() && newLevelId != __instance.currentLevel.levelID){
			__instance.ChangeLevelServerRpc(newLevelId, UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits);
            // Land automatically after flying to the company. Delay to prevent possible issues caused by landing too quickly
            if(newLevelId == 3 && autoLand) __instance.StartCoroutine(DelayStartGame(8f));
		}
	}

}