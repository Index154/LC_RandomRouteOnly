using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(StartMatchLever))]

public class StartMatchLeverPatch {
	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	private static void AutoRouteRandom(ref StartMatchLever __instance)
	{
		bool isFinalDay = TimeOfDay.Instance.daysUntilDeadline <= 0;
		
		// TODO - Route to random level
		int newLevelId = 0;
		
		if (__instance.playersManager.currentLevel.levelID != newLevelId && !isFinalDay && __instance.playersManager.CanChangeLevels())
		{
			__instance.playersManager.ChangeLevel(newLevelId);
		}

        // Reset player rerolls on the final day
        if(isFinalDay){
            foreach(PlayerControllerB player in __instance.playersManager.allPlayerScripts){
                CanReroll cr = player.gameObject.GetComponent<CanReroll>();
                cr.canReroll = true;
            }
        }
	}
}