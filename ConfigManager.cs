using System;
using BepInEx.Configuration;

namespace RandomRouteOnly;

public class ConfigManager {
    internal ConfigEntry<int> rerollsPerPlayer = null!;
    internal ConfigEntry<bool> allowCompany = null!;
    internal ConfigEntry<int> stayOnMoonDays = null!;
    // internal ConfigEntry<int> autoRerouteDelay = null!;      Unfinished idea
    internal ConfigEntry<bool> constellations = null!;

    internal void Setup(ConfigFile configFile) {
        rerollsPerPlayer = configFile.Bind("General", "Rerolls per player per quota", 1, new ConfigDescription("How many rerolls each player gets on every quota"));

        allowCompany = configFile.Bind("General", "Allow manually routing to Company", false, new ConfigDescription("If true, allows you to always route to the Company whenever you want"));

        stayOnMoonDays = configFile.Bind("General", "Max consecutive days per moon", 1, new ConfigDescription("The number of days you can spend on one moon before the ship automatically travels to a new random moon. If you route to the Company before reaching this limit then the ship will return to the previous moon after leaving the Company. Consecutive days per moon are not tracked across game restarts", new AcceptableValueRange<int>(1, 100), Array.Empty<object>()));

        // autoRerouteDelay = configFile.Bind("1. General", "Extra delay in seconds before auto routing to a new moon", 0, new ConfigDescription("Allows you to add some delay between returning to orbit and the ship automatically routing to a random moon. Might be useful because the reroute usually interrupts people who are talking"));

        constellations = configFile.Bind("Other", "Constellations compatibility", true, new ConfigDescription("Whether random routing should only select moons in the current constellations if LethalConstellations is enabled"));
    }
}