using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(Pawn_EquipmentTracker))]
    internal static partial class Pawn_EquipmentTracker_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Pawn_EquipmentTracker),
            "EquipmentTrackerTick"
        )]
        private static bool PrePawn_EquipmentTracker_EquipmentTrackerTick(Pawn_EquipmentTracker __instance)
        {
#if V13 || V14 || V15
            __instance.GetDirectlyHeldThings()?.ThingOwnerTick();
#else
            __instance.GetDirectlyHeldThings()?.DoTick();
#endif
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
        private static void PostPawn_EquipmentTracker_GetGizmosPostfix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
        {
            IEnumerable<Gizmo> forEach(IEnumerable<Gizmo> result)
            {
                foreach (Gizmo gizmo in result)
                {
                    yield return gizmo;
                }
                ThingOwner list = __instance.GetDirectlyHeldThings();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    CompChildNodeProccesser? proccesser = list[i];
                    if (proccesser != null)
                    {
                        foreach (CompBasicNodeComp comp in proccesser.AllNodeComp)
                        {
                            List<Gizmo> gizmos = new List<Gizmo>();
                            try
                            {
                                gizmos.AddRange(comp.CompGetGizmosExtra());
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                            foreach (Gizmo gizmo in gizmos) if (gizmo != null) yield return gizmo;
                        }
                    }
                }
            }
            __result = forEach(__result);
        }
    }
}
