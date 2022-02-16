using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RW_NodeTree;

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
            weapon = ((CompChildNodeProccesser)weapon)?.RootNode ?? weapon;
        }
    }
}
