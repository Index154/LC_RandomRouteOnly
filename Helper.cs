using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BepInEx.Bootstrap;
using System.Linq;
using BepInEx.Configuration;

namespace RandomRouteOnly;

public class Helper(){

    public static List<SelectableLevel> levels = [];
    public static Dictionary<string, int> levelWeights = [];
    public static bool constellationsLoaded = false;
    public static int daysOnLevel = 0;
    public static int previousLevel = -1;
    public static List<int> recentLevels = [];
    public static bool setupDone = false;

    static IEnumerator DelayStartGame(float delay){
        yield return new WaitForSeconds(delay);
        // Land the ship. This is the proper way of doing it to avoid issues
        StartMatchLever lever = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
        lever.StartGame();
    }
    public static void Prepare(ref StartOfRound __instance){
        // Only run this once
        if(setupDone) return;
        setupDone = true;

        // Check if LethalConstellations is active
		if(Chainloader.PluginInfos.ContainsKey("com.github.darmuh.LethalConstellations")) Helper.constellationsLoaded = true;

		// Build list of selectable level IDs that are registered in the terminal (LLL compat)
		// terminal.moonsCatalogueList does not contain hidden moons so we can not use it!
		levels = [];
		TerminalKeyword routeKeyword =  UnityObjectType.FindObjectOfType<Terminal>().terminalNodes.allKeywords[27];
		RandomRouteOnly.Logger.LogInfo("Registered moons:");
		foreach(CompatibleNoun n in routeKeyword.compatibleNouns){
			if(n.result.terminalOptions != null && n.result.terminalOptions.Length > 1){
				int id = n.result.terminalOptions[1].result.buyRerouteToMoon;
				if(id != 3 && n.noun.word != "LiquidationLevel"){
					RandomRouteOnly.Logger.LogInfo(n.noun.word + " | ID = " + id);
					// Need to get the SelectableLevel object here for LethalConstellations compat
					SelectableLevel lvl = __instance.levels.Where(i => i.levelID == id).FirstOrDefault();
					// Prevent duplicates because Dine has two route keywords for some reason and it would be added twice
					if(!levels.Contains(lvl)) levels.Add(lvl);
				}
			}
		}

        // Load weight configs
        ConfigManager.SetupLevelWeights(RandomRouteOnly.Conf, levels);
    }
    public static int GetRandomLevel(StartOfRound __instance){
        bool isFirstDay = (__instance.gameStats.daysSpent == 0) && __instance.currentLevel.levelID == 0;
        var availableLevels = new List<int>();
        var noRepeatLevels = new List<int>();

        RandomRouteOnly.Logger.LogInfo("Selectable levels:");
        foreach(SelectableLevel level in levels){
            bool addLevel = true;
            // Exclude current level (unless it's day 1, otherwise day 1 will never be Experimentation)
            if(level == __instance.currentLevel && !isFirstDay) addLevel = false;
            // Exclude levels not in the current constellation
            if(constellationsLoaded && RandomRouteOnly.configManager.constellations.Value){
                RandomRouteOnly.Logger.LogDebug("Constellation support: Removing moon from random level selection");
                if(!ConstellationsCompat.IsLevelInConstellation(level)) addLevel = false;
            }
            if(addLevel) availableLevels.Add(level.levelID);

            // Attempt to also exclude levels that are in the recently visited levels list
            if(!recentLevels.Contains(level.levelID) && addLevel){
                RandomRouteOnly.Logger.LogInfo(level.name);
                noRepeatLevels.Add(level.levelID);
            }
        }

        // If there are no possible levels to route to then reset the recent moons list and use all available moons instead
        List<int> chosenList;
        if(noRepeatLevels.Count < 1){
            recentLevels.Clear();
            chosenList = availableLevels;
        }else{
            chosenList = noRepeatLevels;
        }

        // Actual random selection process using level weights
        int weightSum = 0;
        foreach(KeyValuePair <int, ConfigEntry<int>> kvp in ConfigManager.levelWeights){
            if(chosenList.Contains(kvp.Key)){
                weightSum += kvp.Value.Value;
            }
        }
        int selectionRoll = Random.Range(1, weightSum);
        RandomRouteOnly.Logger.LogDebug("Level selection roll = " + selectionRoll);
        int chosenID = 0;
        foreach(int levelID in chosenList){
            if(selectionRoll <= ConfigManager.levelWeights[levelID].Value){
                chosenID = levelID;
                RandomRouteOnly.Logger.LogInfo("Randomly selected level " + levels.Where(i => i.levelID == levelID).FirstOrDefault().name);
                break;
            }else{
                selectionRoll -= ConfigManager.levelWeights[levelID].Value;
                RandomRouteOnly.Logger.LogDebug("Skipping level " + levelID);
            }
        }

        return chosenID;
    }
    public static void FlyToLevel(ref StartOfRound __instance, bool randomLevel, bool autoLand){
        // StartOfRound is run by clients too so stop those here
		if(!__instance.NetworkManager.IsHost) return;

		bool isFirstDay = (__instance.gameStats.daysSpent == 0) && __instance.currentLevel.levelID == 0;
        bool maxDaysReached = false;
        // 3 is default for flying to company on the final day
        int newLevelId = 3;

        // Only fly to random if the maximum number of days per moon has been reached
        if(daysOnLevel >= RandomRouteOnly.configManager.stayOnMoonDays.Value) maxDaysReached = true;
        // Change newLevelId to something other than company to prevent the ship flying there when unintended
        if(randomLevel) newLevelId = 0;

        // Return to the previous moon after leaving the company unless we already spent the maximum amount of days there
        if(__instance.currentLevel.levelID == 3 && !maxDaysReached) newLevelId = previousLevel;

        // Pick random target if necessary
        if((randomLevel && maxDaysReached) || isFirstDay || newLevelId == -1) newLevelId = GetRandomLevel(__instance);

        // Prevent auto routing if the maximum number of days hasn't been reached yet (unless orbiting company or going to company or on day 0)
        if(!maxDaysReached && newLevelId != 3 && __instance.currentLevel.levelID != 3 && !isFirstDay){
            RandomRouteOnly.Logger.LogInfo("Maximum days per moon not reached - Aborting reroute");
            return;
        }

        // Add Experimentation to list of recent moons if we start there on day 1
        if(isFirstDay && newLevelId == 0 && RandomRouteOnly.configManager.noRepeatCount.Value != 0){
				recentLevels.Add(newLevelId);
        }

        // Fly to selected level if possible
		if(__instance.CanChangeLevels() && newLevelId != __instance.currentLevel.levelID){
			__instance.ChangeLevelServerRpc(newLevelId, UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits);
            // Land automatically after flying to the company. Delay to prevent possible issues caused by landing too quickly
            if(newLevelId == 3 && autoLand) __instance.StartCoroutine(DelayStartGame(8f));
		}
	}

}