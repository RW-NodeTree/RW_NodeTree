using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(ThingOwner))]
    internal static partial class ThingOwner_Patcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ThingOwner),
            "ThingOwnerTick"
        )]
        private static void PostThingOwner_ThingOwnerTick(ThingOwner __instance, bool removeIfDestroyed)
        {
            for (int i = __instance.Count - 1; i >= 0; i--)
            {
                Thing t = __instance[i];
                if ((t is IVerbOwner) || (t as ThingWithComps)?.AllComps.Find(x => x is IVerbOwner) != null || (CompChildNodeProccesser)t != null)
                {
                    try
                    {
                        if (t.def.tickerType != TickerType.Normal)
                        {
                            t.Tick();
                            if (removeIfDestroyed && t.Destroyed)
                            {
                                __instance.Remove(t);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                    try
                    {
                        if (t.def.tickerType != TickerType.Rare && Find.TickManager.TicksGame % 250 == t.thingIDNumber % 250)
                        {
                            t.TickRare();
                            if (removeIfDestroyed && t.Destroyed)
                            {
                                __instance.Remove(t);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                    try
                    {
                        if (t.def.tickerType != TickerType.Long && Find.TickManager.TicksGame % 2000 == t.thingIDNumber % 2000)
                        {
                            t.TickLong();
                            if (removeIfDestroyed && t.Destroyed)
                            {
                                __instance.Remove(t);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
        }

    }
}
