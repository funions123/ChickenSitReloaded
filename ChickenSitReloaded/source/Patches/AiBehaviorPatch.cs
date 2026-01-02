using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ChickenSitReloaded;
public class AiBehaviorPatch : ModSystem
{
    private Harmony _patcher;

    public static ICoreServerAPI _api;

    public static ChickenSitReloadedConfig Config { get; private set; }

    public static HashSet<string> AllowedEntityPaths = new HashSet<string>
    {
        "chicken",
        "goat"
    };

    public override void StartServerSide(ICoreServerAPI api)
    {
        _api = api;
        _patcher = new Harmony(Mod.Info.ModID);

        try
        {
            Config = api.LoadModConfig<ChickenSitReloadedConfig>("ChickenSitReloadedConfig.json");
        }
        catch
        {
            Config = null;
        }

        if (Config == null)
        {
            // rewrite default config if old one not found or corrupted
            Config = new ChickenSitReloadedConfig();
            api.StoreModConfig(Config, "ChickenSitReloadedConfig.json");
        }

        if (api.ModLoader.IsModEnabled("moreanimals"))
        {
            api.Logger.Notification("ChickenSitReloaded: 'More Animals' detected. Enabling compatibility.");
            AllowedEntityPaths.Add("capercaillie");
            AllowedEntityPaths.Add("goldenpheasant");
            AllowedEntityPaths.Add("pheasant");
            AllowedEntityPaths.Add("wildturkey");
        }

        if (api.ModLoader.IsModEnabled("bovinae"))
        {
            api.Logger.Notification("ChickenSitReloaded: 'Fauna of the Stone Age: Bovinae' detected. Enabling compatibility.");
            AllowedEntityPaths.Add("bovinae");
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
        if (!__result || __instance.entity == null) return;

        Entity entity = __instance.entity;
        if (entity == null) return;

        string path = entity.Code.Path;
        bool isTargetAnimal = false;
        foreach (var allowed in AiBehaviorPatch.AllowedEntityPaths)
        {
            if (path.StartsWith(allowed))
            {
                isTargetAnimal = true;
                break;
            }
        }

        if (!isTargetAnimal) return;

        int generation = entity.WatchedAttributes.GetInt("generation", 0);
        if (generation < AiBehaviorPatch.Config.MinimumGeneration) return;

        Entity target = __instance.targetEntity;

        if (target is EntityPlayer)
        {
            __result = false;
        }
    }
}
