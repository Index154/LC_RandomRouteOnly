using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RandomRouteOnly;

public class Helper(){
    static IEnumerator DelayStartGame(float delay){
        yield return new WaitForSeconds(delay);
        // Land the ship. This is the proper way of doing it to avoid issues
        StartMatchLever lever = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
        lever.StartGame();
    }
    public static void FlyToLevel(ref StartOfRound __instance, bool randomLevel, bool autoLand){
        // StartOfRound is run by clients too so stop those here
		if(!__instance.NetworkManager.IsHost) return;

		bool isFirstDay = __instance.gameStats.daysSpent == 0;
		SelectableLevel[] levels = __instance.levels;
		var possibleLevels = new List<SelectableLevel>();
        int newLevelId = 3;     // 3 is default for flying to company on the final day

        if(randomLevel) {
            foreach(SelectableLevel level in levels){
                // Exclude from random level pool:
                // Company, Liquidation and current level (unless it's day 1, otherwise day 1 will never be Experimentation)
                if(level.levelID != 3 && level.levelID != 11 && (level.levelID != __instance.currentLevel.levelID || isFirstDay)) possibleLevels.Add(level);
            }
            newLevelId = possibleLevels[Random.Range(0, possibleLevels.Count)].levelID;
        }
		
		if(__instance.CanChangeLevels() && newLevelId != __instance.currentLevel.levelID){
			__instance.ChangeLevelServerRpc(newLevelId, UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits);
            if(!randomLevel) {
                // Land automatically. Delay to prevent possible issues caused by landing too quickly
                if(autoLand) __instance.StartCoroutine(DelayStartGame(8f));
            }
		}
	}

}