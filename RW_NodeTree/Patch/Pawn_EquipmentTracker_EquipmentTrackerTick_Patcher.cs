using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(Pawn_EquipmentTracker))]
    internal static class Pawn_EquipmentTracker_EquipmentTrackerTick_Patcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn_EquipmentTracker),
            "EquipmentTrackerTick"
        )]
        private static void PostPawn_EquipmentTracker_EquipmentTrackerTick(Pawn_EquipmentTracker __instance)
        {
            Pawn_EquipmentTracker_equipment(__instance)?.ThingOwnerTick();
        }
        private static AccessTools.FieldRef<Pawn_EquipmentTracker, ThingOwner<ThingWithComps>> Pawn_EquipmentTracker_equipment = AccessTools.FieldRefAccess<ThingOwner<ThingWithComps>>(typeof(Pawn_EquipmentTracker), "equipment");
    }
}
