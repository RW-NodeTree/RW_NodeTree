
using HarmonyLib;
using System;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(ThingOwner))]
    internal static partial class ThingOwner_Patcher
    {

        [HarmonyPostfix]
#if V13 || V14 || V15
        [HarmonyPatch(
            typeof(ThingOwner),
            "ThingOwnerTick"
        )]
        private static void PostThingOwner_ThingOwnerTick(ThingOwner __instance, bool removeIfDestroyed)
#else
        [HarmonyPatch(
            typeof(ThingOwner),
            "DoTick"
        )]
        private static void PostThingOwner_DoTick(ThingOwner __instance)
#endif // V16
        {
#if !V13 && !V14 && !V15
            bool removeIfDestroyed = __instance.removeContentsIfDestroyed;
#endif
            for (int i = __instance.Count - 1; i >= 0; i--)
            {
                Thing t = __instance[i];
                if ((t is IVerbOwner) || (t as ThingWithComps)?.AllComps.Find(x => x is IVerbOwner) != null || (CompChildNodeProccesser?)t != null)
                {
                    try
                    {
                        if (t.def.tickerType != TickerType.Normal)
                        {
                            TickerType tickerType = t.def.tickerType;
                            t.def.tickerType = TickerType.Normal;
#if V13 || V14 || V15
                            t.Tick();
#else
                            t.DoTick();
#endif
                            t.def.tickerType = tickerType;
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

                        if (
                        t.def.tickerType != TickerType.Rare
#if V13 || V14 || V15
                        && Find.TickManager.TicksGame % 250 == t.thingIDNumber % 250
#endif
                        )
                        {
                            TickerType tickerType = t.def.tickerType;
                            t.def.tickerType = TickerType.Rare;
#if V13 || V14 || V15
                            t.TickRare();
#else
                            t.DoTick();
#endif
                            t.def.tickerType = tickerType;
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

                        if (
                        t.def.tickerType != TickerType.Long
#if V13 || V14 || V15
                        && Find.TickManager.TicksGame % 2000 == t.thingIDNumber % 2000
#endif
                        )
                        {
                            TickerType tickerType = t.def.tickerType;
                            t.def.tickerType = TickerType.Long;
#if V13 || V14 || V15
                            t.TickLong();
#else
                            t.DoTick();
#endif
                            t.def.tickerType = tickerType;
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