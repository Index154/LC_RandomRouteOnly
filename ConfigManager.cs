using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace RandomRouteOnly;

public class ConfigManager {
    internal ConfigEntry<int> rerollsPerPlayer = null!;
    internal ConfigEntry<bool> allowCompany = null!;
    internal ConfigEntry<int> noRepeatCount = null!;
    internal ConfigEntry<int> stayOnMoonDays = null!;
    // internal ConfigEntry<int> autoRerouteDelay = null!;      Unfinished idea
    internal ConfigEntry<bool> constellations = null!;
    public static Dictionary<int, ConfigEntry<int>> levelWeights = [];

    internal void Setup(ConfigFile configFile) {
        rerollsPerPlayer = configFile.Bind("General", "Rerolls per player per quota", 1, new ConfigDescription("How many uses of the terminal command 'random' each player gets per quota"));

        allowCompany = configFile.Bind("General", "Allow manually routing to Company", false, new ConfigDescription("If true, allows you to always route to the Company whenever you want. Also supports the modded moon Galetry"));

        noRepeatCount = configFile.Bind("General", "Number of previous moons to avoid", 0, new ConfigDescription("How many of the most recently routed to moons should be removed from the random routing pool. Useful for ensuring a certain level of variety. Set it to -1 if you want to avoid any and all moon repeats for as long as possible. In this case, once there are no more unvisited moons left to route to, the list of visited moons will be emptied again. Not tracked across game restarts", new AcceptableValueRange<int>(-1, 50), Array.Empty<object>()));

        stayOnMoonDays = configFile.Bind("General", "Max consecutive days per moon", 1, new ConfigDescription("The number of days you can spend on a moon before the ship automatically travels to a new random moon. If you route to the Company before reaching this limit then the ship will return to the previous moon after leaving the Company. Not tracked across game restarts", new AcceptableValueRange<int>(1, 100), Array.Empty<object>()));

        // autoRerouteDelay = configFile.Bind("1. General", "Extra delay in seconds before auto routing to a new moon", 0, new ConfigDescription("Allows you to add some delay between returning to orbit and the ship automatically routing to a random moon. Might be useful because the reroute usually interrupts people who are talking"));

        constellations = configFile.Bind("Other", "Constellations compatibility", true, new ConfigDescription("Whether random routing should only select moons in the current constellations if LethalConstellations is enabled"));
    }

    internal static void SetupLevelWeights(ConfigFile configFile, List<SelectableLevel> levels) {
        RandomRouteOnly.Logger.LogInfo("Random routing weights:");
        foreach(SelectableLevel lvl in levels){
            string name = lvl.name.Replace("Level", "");
            ConfigEntry<int> lvlEntry = configFile.Bind("Weights", name + " weight", 100);
            if(!levelWeights.ContainsKey(lvl.levelID)) {
                levelWeights.Add(lvl.levelID, lvlEntry);
                RandomRouteOnly.Logger.LogInfo(lvl.name + " weight = " + levelWeights[lvl.levelID].Value);
            }
        }
    }
}