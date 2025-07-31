using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(StatWorker_MeleeAverageDPS))]
    internal static class StatWorker_MeleeAverageDPS_Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker_MeleeAverageDPS),
            "GetCurrentWeaponUser",
            typeof(Thing)
        )]
        private static void PreStatWorker_MeleeAverageDPS_GetCurrentWeaponUser(ref Thing weapon)
        {
            Thing? root = weapon.RootNode();
            weapon = root ?? weapon;
        }
    }
}
