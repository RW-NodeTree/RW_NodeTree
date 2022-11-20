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
            "GearHasCompsThatAffectStat"
            )]
        public static void PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser comp = gear.RootNode();
            if (comp != null)
            {
                __state = new Dictionary<string, object>();
                comp.PreStatWorker_GearHasCompsThatAffectStat(gear, stat, __state);
            }
        }

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
            "GearHasCompsThatAffectStat"
            )]
        public static void PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref bool __result, Dictionary<string, object> __state)
        {
            __result = gear.RootNode()?.PostStatWorker_GearHasCompsThatAffectStat(gear, stat, __result, __state) ?? __result;
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
    }
}

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {
        public virtual void PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_GearHasCompsThatAffectStat(gear, stat, forPostRead);
            }
        }
        public virtual void PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_StatOffsetFromGear(gear, stat, forPostRead);
            }
        }
        public virtual void PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_InfoTextLineFromGear(gear, stat, forPostRead);
            }
        }

        public virtual bool PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_GearHasCompsThatAffectStat(gear, stat, result, forPostRead);
            }
            return result;
        }

        public virtual float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_StatOffsetFromGear(gear, stat, result, forPostRead);
            }
            return result;
        }

        public virtual string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_InfoTextLineFromGear(gear, stat, result, forPostRead) ?? result;
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual void PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual void PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual void PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual bool PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> forPostRead)
        {
            return result;
        }

        internal void internal_PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
            => PreStatWorker_GearHasCompsThatAffectStat(gear, stat, forPostRead);
        internal void internal_PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
            => PreStatWorker_StatOffsetFromGear(gear, stat, forPostRead);
        internal void internal_PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object> forPostRead)
            => PreStatWorker_InfoTextLineFromGear(gear, stat, forPostRead);
        internal bool internal_PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> forPostRead)
            => PostStatWorker_GearHasCompsThatAffectStat(gear, stat, result, forPostRead);
        internal float internal_PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
            => PostStatWorker_StatOffsetFromGear(gear, stat, result, forPostRead);
        internal string internal_PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object> forPostRead)
            => PostStatWorker_InfoTextLineFromGear(gear, stat, result, forPostRead);
    }
}