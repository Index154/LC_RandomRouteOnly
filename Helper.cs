using UnityEngine;
using System.Collections.Generic;

namespace RandomRouteOnly;

public class Helper(){
    public static void FlyToLevel(ref StartOfRound __instance, bool randomLevel){

		if(!__instance.NetworkManager.IsHost) return;

		bool isFirstDay = __instance.gameStats.daysSpent == 0;
		SelectableLevel[] levels = __instance.levels;
		var possibleLevels = new List<SelectableLevel>();
        int newLevelId = 3;

        if(randomLevel){
            foreach(SelectableLevel level in levels){
                // Exclude Company, Liquidation and current level unless it's day 1 (otherwise day 1 will never be Experimentation)
                if(level.levelID != 3 && level.levelID != 11 && (level.levelID != __instance.currentLevel.levelID || isFirstDay)) possibleLevels.Add(level);
            }
            newLevelId = possibleLevels[Random.Range(0, possibleLevels.Count)].levelID;
        }
		
		if (__instance.CanChangeLevels() && newLevelId != __instance.currentLevel.levelID)
		{
			__instance.ChangeLevelServerRpc(newLevelId, UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits);
            if(!randomLevel) {
                new WaitForSeconds(4f);
                __instance.StartGame();
            }
		}
	}
}