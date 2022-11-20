using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(Pawn_ApparelTracker))]
    internal static partial class Pawn_ApparelTracker_Patcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn_ApparelTracker),
            "ApparelTrackerTick"
        )]
        private static void PostPawn_ApparelTracker_ApparelTrackerTick(Pawn_ApparelTracker __instance)
        {
            ThingOwner<Apparel> list = Pawn_ApparelTracker_wornApparel(__instance);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Thing t = list[i];
                if (t.def.tickerType == TickerType.Never)
                {
                    if ((t is IVerbOwner) || (t as ThingWithComps)?.AllComps.Find(x => x is IVerbOwner) != null || (CompChildNodeProccesser)t != null)
                    {
                        try
                        {
                            t.Tick();
                            if (t.Destroyed)
                            {
                                list.Remove(t);
                            }
                        }
                        catch(Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                }
            }
        }

        private static AccessTools.FieldRef<Pawn_ApparelTracker, ThingOwner<Apparel>> Pawn_ApparelTracker_wornApparel = AccessTools.FieldRefAccess<ThingOwner<Apparel>>(typeof(Pawn_ApparelTracker), "wornApparel");
    }
}
