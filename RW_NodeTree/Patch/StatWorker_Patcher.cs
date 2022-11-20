using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RW_NodeTree;
using RW_NodeTree.Tools;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(StatWorker))]
    internal static partial class StatWorker_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        public static void PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser comp = gear.RootNode();
            if (comp != null)
            {
                __state = new Dictionary<string, object>();
                comp.PreStatWorker_StatOffsetFromGear(gear, stat, __state);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        public static void PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser comp = gear.RootNode();
            if (comp != null)
            {
                __state = new Dictionary<string, object>();
                comp.PreStatWorker_InfoTextLineFromGear(gear, stat, __state);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        public static void PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref float __result, Dictionary<string, object> __state)
        {
            __result = gear.RootNode()?.PostStatWorker_StatOffsetFromGear(gear, stat, __result, __state) ?? __result;
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        public static void PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref string __result, Dictionary<string, object> __state)
        {
            __result = gear.RootNode()?.PostStatWorker_InfoTextLineFromGear(gear, stat, __result, __state) ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "RelevantGear"
            )]
        public static void PostStatWorker_RelevantGear(Pawn pawn, StatDef stat, ref IEnumerable<Thing> __result, Dictionary<string, object> __state)
        {
            __result = pawn.RootNode()?.PostStatWorker_RelevantGear(pawn, stat, __result, __state) ?? __result;

            if (pawn.apparel != null)
            {
                foreach (Apparel thing in pawn.apparel.WornApparel)
                {
                    __result = thing.RootNode()?.PostStatWorker_RelevantGear(thing, stat, __result, __state) ?? __result;
                }
            }

            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thing in pawn.equipment.AllEquipmentListForReading)
                {
                    __result = thing.RootNode()?.PostStatWorker_RelevantGear(thing, stat, __result, __state) ?? __result;
                }
            }
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
        public virtual void PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    comp.internal_PreStatWorker_StatOffsetFromGear(gear, stat, forPostRead);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }
        public virtual void PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    comp.internal_PreStatWorker_InfoTextLineFromGear(gear, stat, forPostRead);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }

        public virtual IEnumerable<Thing> PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_RelevantGear(gear, stat, result, forPostRead) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        public virtual float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_StatOffsetFromGear(gear, stat, result, forPostRead);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        public virtual string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_InfoTextLineFromGear(gear, stat, result, forPostRead) ?? result;
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
        protected virtual void PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual void PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual IEnumerable<Thing> PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        
        internal void internal_PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
            => PreStatWorker_StatOffsetFromGear(gear, stat, forPostRead);
        internal void internal_PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
            => PreStatWorker_InfoTextLineFromGear(gear, stat, forPostRead);
        internal float internal_PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
            => PostStatWorker_StatOffsetFromGear(gear, stat, result, forPostRead);
        internal string internal_PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> forPostRead)
            => PostStatWorker_InfoTextLineFromGear(gear, stat, result, forPostRead);
        internal IEnumerable<Thing> internal_PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result, Dictionary<string, object> forPostRead)
            => PostStatWorker_RelevantGear(gear, stat, result, forPostRead);
    }
}