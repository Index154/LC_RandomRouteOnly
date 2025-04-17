using HarmonyLib;

namespace RandomRouteOnly.Patches;

[HarmonyPatch(typeof(Terminal))]

public class TerminalPatch {

	private static readonly TerminalNode noReroll = new(){
		name = "noReroll",
		displayText = "\nYou have no rerolls left for this quota!\nLand the ship or let somebody else use the command.\n\nYour rerolls will be set to " + RandomRouteOnly.configManager.rerollsPerPlayer.Value + " at the start of the next quota\n\n\n",
		clearPreviousText = true
	};

	private static readonly TerminalNode noManualRoutesAllowed = new(){
		name = "noManualRoutesAllowed",
		displayText = "\nYou are not allowed to manually fly to this moon!\nLand the ship or use the command 'random'\n\n\n",
		clearPreviousText = true
	};

	[HarmonyPatch("ParsePlayerSentence")]
	[HarmonyPostfix]
	private static TerminalNode RestrictRouteUsage(TerminalNode __result){

		RandomRouteOnly.Logger.LogDebug("TerminalNode => " + __result.name);

		if(__result.name == "routeRandom" || __result.name == "routeRandomFilterWeather"){
			Rerolls cr = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<Rerolls>();
            if(cr.rerolls < 1){
                return noReroll;
            }
           	cr.rerolls -= 1;
		}else if(__result.name.ToLowerInvariant().Contains("route") && !__result.name.Contains("Confirm") && ((!__result.name.Contains("Company") && !__result.name.ToLowerInvariant().Contains("galetry")) || !RandomRouteOnly.configManager.allowCompany.Value)){
			// All manual moon routes should have route in the name... I hope
			// Todo: Make this work based on buyRerouteToMoon property instead?
			return noManualRoutesAllowed;
		}
		return __result;
	}

	[HarmonyPatch("TextPostProcess")]
	[HarmonyPostfix]
	private static string AddRemainingRerollsText(string modifiedDisplayText, TerminalNode node, ref string __result){
		if(__result.Contains("Routing autopilot to")){
			// Scuffed way of adding text to the command output when the command succeeds
			Rerolls cr = GameNetworkManager.Instance.localPlayerController.gameObject.GetComponent<Rerolls>();
			string sGrammar = "s";
			if(cr.rerolls == 1){ sGrammar = ""; }
			__result += "You have " + cr.rerolls + " reroll" + sGrammar + " left for this quota\n\n\n";
		}
		return __result;
	}
}