using System.Collections.Generic;
using Vintagestory.API.Common;

namespace ChickenSitReloaded;

public sealed class ChickenSitReloadedModSystem : ModSystem
{

}

public class ChickenSitReloadedConfig
{
    public Dictionary<string, int> AnimalMinimumGeneration { get; set; } = new()
    {
        // Vanilla
        { "chicken",        1 },
        { "goat",           1 },
        { "rabbit",         1 },
        { "sheep",          1 },
        { "pig",            1 },
        // More Animals mod
        { "capercaillie",   1 },
        { "goldenpheasant", 1 },
        { "pheasant",       1 },
        { "wildturkey",     1 },
        // Fauna of the Stone Age: Bovinae mod
        { "bovinae",        1 },
    };
}
