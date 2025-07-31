using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(StatWorker))]
    internal static partial class StatWorker_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "GearHasCompsThatAffectStat"
            )]
        private static bool PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser? comp = gear.RootNode();
            if (comp != null)
            {
                __state = new Dictionary<string, object>();
                return comp.PreStatWorker_GearHasCompsThatAffectStat(gear, stat, __state);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        private static bool PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser? comp = gear.RootNode();
            if (comp != null)
            {
                __state = new Dictionary<string, object>();
                return comp.PreStatWorker_StatOffsetFromGear(gear, stat, __state);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        private static bool PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser? comp = gear.RootNode();
            if (comp != null)
            {
                __state = new Dictionary<string, object>();
                return comp.PreStatWorker_InfoTextLineFromGear(gear, stat, __state);
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "GearHasCompsThatAffectStat"
            )]
        private static void PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref bool __result, Dictionary<string, object> __state)
        {
            __result = gear.RootNode()?.PostStatWorker_GearHasCompsThatAffectStat(gear, stat, __result, __state) ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        private static void PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref float __result, Dictionary<string, object> __state)
        {
            __result = gear.RootNode()?.PostStatWorker_StatOffsetFromGear(gear, stat, __result, __state) ?? __result;
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        private static void PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref string __result, Dictionary<string, object> __state)
        {
            __result = gear.RootNode()?.PostStatWorker_InfoTextLineFromGear(gear, stat, __result, __state) ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "RelevantGear"
            )]
        private static void PostStatWorker_RelevantGear(Pawn pawn, StatDef stat, ref IEnumerable<Thing> __result)
        {
            __result = pawn.RootNode()?.PostStatWorker_RelevantGear(pawn, stat, __result) ?? __result;

            if (pawn.apparel != null)
            {
                foreach (Apparel thing in pawn.apparel.WornApparel)
                {
                    __result = thing.RootNode()?.PostStatWorker_RelevantGear(thing, stat, __result) ?? __result;
                }
            }

            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thing in pawn.equipment.AllEquipmentListForReading)
                {
                    __result = thing.RootNode()?.PostStatWorker_RelevantGear(thing, stat, __result) ?? __result;
                }
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(StatWorker),
            "GearHasCompsThatAffectStat"
            )]
        private static void FinalStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref bool __result, Dictionary<string, object> __state, Exception __exception)
        {
            __result = gear.RootNode()?.FinalStatWorker_GearHasCompsThatAffectStat(gear, stat, __result, __state, __exception) ?? __result;
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        private static void FinalStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref float __result, Dictionary<string, object> __state, Exception __exception)
        {
            __result = gear.RootNode()?.FinalStatWorker_StatOffsetFromGear(gear, stat, __result, __state, __exception) ?? __result;
        }


        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        private static void FinalStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref string __result, Dictionary<string, object> __state, Exception __exception)
        {
            __result = gear.RootNode()?.FinalStatWorker_InfoTextLineFromGear(gear, stat, __result, __state, __exception) ?? __result;
        }
    }
}

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {
        internal bool PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_GearHasCompsThatAffectStat(gear, stat, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal bool PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_StatOffsetFromGear(gear, stat, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal bool PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_InfoTextLineFromGear(gear, stat, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal bool PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_GearHasCompsThatAffectStat(gear, stat, result, stats);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_StatOffsetFromGear(gear, stat, result, stats);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_InfoTextLineFromGear(gear, stat, result, stats) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal IEnumerable<Thing> PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result)
        {
            UpdateNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_RelevantGear(gear, stat, result) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal bool FinalStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_GearHasCompsThatAffectStat(gear, stat, result, stats, exception);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal float FinalStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_StatOffsetFromGear(gear, stat, result, stats, exception);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        internal string FinalStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_InfoTextLineFromGear(gear, stat, result, stats, exception) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual bool PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual bool PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual bool PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual bool PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> stats)
        {
            return result;
        }
        protected virtual float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> stats)
        {
            return result;
        }
        protected virtual string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> stats)
        {
            return result;
        }
        protected virtual IEnumerable<Thing> PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result)
        {
            return result;
        }
        protected virtual bool FinalStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }
        protected virtual float FinalStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }
        protected virtual string FinalStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }

        internal bool internal_PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object> stats)
            => PreStatWorker_GearHasCompsThatAffectStat(gear, stat, stats);
        internal bool internal_PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> stats)
            => PreStatWorker_StatOffsetFromGear(gear, stat, stats);
        internal bool internal_PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> stats)
            => PreStatWorker_InfoTextLineFromGear(gear, stat, stats);
        internal bool internal_PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> stats)
            => PostStatWorker_GearHasCompsThatAffectStat(gear, stat, result, stats);
        internal float internal_PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> stats)
            => PostStatWorker_StatOffsetFromGear(gear, stat, result, stats);
        internal string internal_PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> stats)
            => PostStatWorker_InfoTextLineFromGear(gear, stat, result, stats);
        internal IEnumerable<Thing> internal_PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result)
            => PostStatWorker_RelevantGear(gear, stat, result);
        internal bool internal_FinalStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> stats, Exception exception)
            => FinalStatWorker_GearHasCompsThatAffectStat(gear, stat, result, stats, exception);
        internal float internal_FinalStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> stats, Exception exception)
            => FinalStatWorker_StatOffsetFromGear(gear, stat, result, stats, exception);
        internal string internal_FinalStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> stats, Exception exception)
            => FinalStatWorker_InfoTextLineFromGear(gear, stat, result, stats, exception);
    }
}