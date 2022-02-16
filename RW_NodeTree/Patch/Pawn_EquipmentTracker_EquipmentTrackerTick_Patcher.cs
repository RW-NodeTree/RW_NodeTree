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
    internal static partial class Pawn_EquipmentTracker_Patcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn_EquipmentTracker),
            "EquipmentTrackerTick"
        )]
        private static void PostPawn_EquipmentTracker_EquipmentTrackerTick(Pawn_EquipmentTracker __instance)
        {
            ThingOwner<ThingWithComps> list = Pawn_EquipmentTracker_equipment(__instance);
            list?.ThingOwnerTick();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Thing t = list[i];
                if (t.def.tickerType == TickerType.Never)
                {
                    if ((t is IVerbOwner) || (t as ThingWithComps)?.AllComps.Find(x => x is IVerbOwner) != null || (CompChildNodeProccesser)t != null)
                    {
                        t.Tick();
                        if (t.Destroyed)
                        {
                            list.Remove(t);
                        }
                    }
                }
            }
        }
        private static AccessTools.FieldRef<Pawn_EquipmentTracker, ThingOwner<ThingWithComps>> Pawn_EquipmentTracker_equipment = AccessTools.FieldRefAccess<ThingOwner<ThingWithComps>>(typeof(Pawn_EquipmentTracker), "equipment");
    }
}
