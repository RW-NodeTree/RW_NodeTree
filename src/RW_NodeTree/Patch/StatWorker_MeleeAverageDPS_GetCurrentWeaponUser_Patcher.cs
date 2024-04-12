using HarmonyLib;
using RimWorld;
using Verse;
using RW_NodeTree.Tools;

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
            weapon = weapon.RootNode() ?? weapon;
        }
    }
}
