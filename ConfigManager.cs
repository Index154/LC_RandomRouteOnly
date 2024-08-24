using BepInEx.Configuration;

namespace RandomRouteOnly;

public class ConfigManager {
    internal ConfigEntry<int> rerollsPerPlayer = null!;
    internal ConfigEntry<bool> allowCompany = null!;

    internal void Setup(ConfigFile configFile) {
        rerollsPerPlayer = configFile.Bind("General", "Rerolls per player per quota", 1, new ConfigDescription("How many rerolls each player gets on every quota"));

        allowCompany = configFile.Bind("General", "Allow manually routing to Company", false, new ConfigDescription("If true, allows you to always route to the Company. Although it's quite tedious, this basically lets you reroll your moon as often as you want which is why it's not on by default. Can be nice to have though if you're sure that nobody will abuse it"));
    }
}