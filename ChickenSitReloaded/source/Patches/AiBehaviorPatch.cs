using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ChickenSitReloaded;

public class AiBehaviorPatch : ModSystem
{
    private Harmony _patcher;

    public static ICoreServerAPI _api;

    public static ChickenSitReloadedConfig Config { get; private set; }

    // animals in this dict are considered active and the patch is applied to them
    public static Dictionary<string, int> ActivePrefixes { get; private set; } = new();

    private const string ConfigFileName = "ChickenSitReloadedConfig.json";

    // map modid to animal prefixes
    private static readonly Dictionary<string, string> ModRequirements = new()
    {
        { "capercaillie",   "moreanimals" },
        { "goldenpheasant", "moreanimals" },
        { "pheasant",       "moreanimals" },
        { "wildturkey",     "moreanimals" },
        { "bovinae",        "bovinae"     },
    };

    public override void StartServerSide(ICoreServerAPI api)
    {
        _api = api;
        _patcher = new Harmony(Mod.Info.ModID);

        try
        {
            Config = api.LoadModConfig<ChickenSitReloadedConfig>(ConfigFileName);
        }
        catch
        {
            Config = null;
        }

        if (Config == null)
        {
            Config = new ChickenSitReloadedConfig();
            api.StoreModConfig(Config, ConfigFileName);
        }

        // add animals to the active dict based on which mods are installed
        ActivePrefixes = new Dictionary<string, int>();
        foreach (var kvp in Config.AnimalMinimumGeneration)
        {
            if (ModRequirements.TryGetValue(kvp.Key, out string requiredMod))
            {
                if (api.ModLoader.IsModEnabled(requiredMod))
                    ActivePrefixes[kvp.Key] = kvp.Value;
            }
            else
            {
                ActivePrefixes[kvp.Key] = kvp.Value;
            }
        }

        _patcher.PatchAll();
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        if (api.Side != EnumAppSide.Server)
        {
            return;
        }
    }

    public override void Dispose()
    {
        _patcher?.UnpatchAll(Mod.Info.ModID);
        base.Dispose();
    }

}


[HarmonyPatch(typeof(AiTaskFleeEntity))]
[HarmonyPatch("ShouldExecute")]
public static class DomesticationPatches
{
    [HarmonyPostfix]
    public static void ShouldExecutePostfix(AiTaskFleeEntity __instance, ref bool __result)
    {
        if (!__result || __instance.entity == null || __instance.targetEntity is not EntityPlayer) return;

        string path = __instance.entity.Code.Path;
        foreach (var kvp in AiBehaviorPatch.ActivePrefixes)
        {
            if (path.StartsWith(kvp.Key))
            {
                int generation = __instance.entity.WatchedAttributes.GetInt("generation", 0);
                if (generation >= kvp.Value) __result = false;
                return;
            }
        }
    }
}

