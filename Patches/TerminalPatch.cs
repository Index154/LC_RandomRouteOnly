using System.Linq;
using HarmonyLib;
using UnityEngine.Assertions.Must;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(Terminal))]

public class TerminalPatch {

	private static readonly TerminalNode noReroll = new(){
		name = "noReroll",
		displayText = "\nYou have no rerolls left for this quota!\nLand the ship or let somebody else use the command.\n\n\n",
		clearPreviousText = true
	};

	private static readonly TerminalNode noManualRoutesAllowed = new(){
		name = "noManualRoutesAllowed",
		displayText = "\nYou are not allowed to choose a moon manually!\nLand the ship or use the random command.\n\n\n",
		clearPreviousText = true
	};

	[HarmonyPatch("ParsePlayerSentence")]
	[HarmonyPostfix]
	private static TerminalNode RestrictRouteUsage(TerminalNode __result){
		if(__result.name == "routeRandom" || __result.name == "routeRandomFilterWeather"){
			CanReroll cr = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<CanReroll>();
            if(!cr.canReroll){
                return noReroll;
            }
           	cr.canReroll = false;
		}else if(__result.name.Contains("route") && __result.name != "routeRandomFilterWeather"){
			return noManualRoutesAllowed;
		}
		return __result;
	}
}