using LethalConstellations.PluginCore;

namespace RandomRouteOnly;

internal class ConstellationsCompat
{
    internal static bool IsLevelInConstellation(SelectableLevel level)
    {
        return ClassMapper.IsLevelInConstellation(level);
    }
}