using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(Terminal))]

public class TerminalPatch {
	[HarmonyPatch("ParsePlayerSentence")]
	[HarmonyPostfix]
	private static void RestrictRouteUsage(TerminalNode __result){
		if(__result.name == "routeRandom"){
			CanReroll cr = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CanReroll>();
            if(!cr.canReroll){
                // Make command fail
                // What happens when this is called by the code in StartMatchLeverPatch? I guess there is no ParsePlayerSentence happening in that case?
            }
           	cr.canReroll = false;
		}else{
			if(__result.name != "routeRandomFilterWeather"){
				// Make command fail
			}
		}
	}
}