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

    public override void StartServerSide(ICoreServerAPI api)
    {
        _api = api;
        _patcher = new Harmony(Mod.Info.ModID);

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
        if (!__result) return;

        Entity entity = __instance.entity;
        if (entity == null) return;

        string path = entity.Code.Path;
        bool isTargetAnimal = path.StartsWith("chicken") || path.StartsWith("goat");

        if (!isTargetAnimal) return;

        int generation = entity.WatchedAttributes.GetInt("generation", 0);
        if (generation < 1) return;

        Entity target = __instance.targetEntity;

        if (target is EntityPlayer)
        {
            __result = false;
        }
    }
}
