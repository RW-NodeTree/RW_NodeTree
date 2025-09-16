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
        private static readonly AccessTools.FieldRef<StatWorker, StatDef> StatWorker_stat = AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "GearHasCompsThatAffectStat"
            )]
        private static bool PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref Dictionary<string, object?> __state)
        {
            IStatGearAffectPatcher? comp = gear as IStatGearAffectPatcher;
            if (comp != null)
            {
                __state = new Dictionary<string, object?>();
                return comp.PreStatWorker_GearHasCompsThatAffectStat(gear, stat, __state);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        private static bool PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref Dictionary<string, object?> __state)
        {
            IStatGearAffectPatcher? comp = gear as IStatGearAffectPatcher;
            if (comp != null)
            {
                __state = new Dictionary<string, object?>();
                return comp.PreStatWorker_StatOffsetFromGear(gear, stat, __state);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        private static bool PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref Dictionary<string, object?> __state)
        {
            IStatGearAffectPatcher? comp = gear as IStatGearAffectPatcher;
            if (comp != null)
            {
                __state = new Dictionary<string, object?>();
                return comp.PreStatWorker_InfoTextLineFromGear(gear, stat, __state);
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "GearHasCompsThatAffectStat"
            )]
        private static void PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref bool __result, Dictionary<string, object?> __state)
        {
            IStatGearAffectPatcher? proccesser = gear as IStatGearAffectPatcher;
            __result = proccesser?.PostStatWorker_GearHasCompsThatAffectStat(gear, stat, __result, __state) ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        private static void PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref float __result, Dictionary<string, object?> __state)
        {
            IStatGearAffectPatcher? proccesser = gear as IStatGearAffectPatcher;
            __result = proccesser?.PostStatWorker_StatOffsetFromGear(gear, stat, __result, __state) ?? __result;
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        private static void PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref string __result, Dictionary<string, object?> __state)
        {
            IStatGearAffectPatcher? proccesser = gear as IStatGearAffectPatcher;
            __result = proccesser?.PostStatWorker_InfoTextLineFromGear(gear, stat, __result, __state) ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(StatWorker),
            "RelevantGear"
            )]
        private static void PostStatWorker_RelevantGear(Pawn pawn, StatDef stat, ref IEnumerable<Thing> __result)
        {
            IStatGearAffectPatcher? proccesser = pawn as IStatGearAffectPatcher;
            __result = proccesser?.PostStatWorker_RelevantGear(pawn, stat, __result) ?? __result;

            if (pawn.apparel != null)
            {
                foreach (Apparel thing in pawn.apparel.WornApparel)
                {
                    proccesser = thing as IStatGearAffectPatcher;
                    __result = proccesser?.PostStatWorker_RelevantGear(thing, stat, __result) ?? __result;
                }
            }

            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thing in pawn.equipment.AllEquipmentListForReading)
                {
                    proccesser = thing as IStatGearAffectPatcher;
                    __result = proccesser?.PostStatWorker_RelevantGear(thing, stat, __result) ?? __result;
                }
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(StatWorker),
            "GearHasCompsThatAffectStat"
            )]
        private static void FinalStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, ref bool __result, Dictionary<string, object?> __state, Exception __exception)
        {
            IStatGearAffectPatcher? proccesser = gear as IStatGearAffectPatcher;
            __result = proccesser?.FinalStatWorker_GearHasCompsThatAffectStat(gear, stat, __result, __state, __exception) ?? __result;
        }

        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(StatWorker),
            "StatOffsetFromGear"
            )]
        private static void FinalStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, ref float __result, Dictionary<string, object?> __state, Exception __exception)
        {
            IStatGearAffectPatcher? proccesser = gear as IStatGearAffectPatcher;
            __result = proccesser?.FinalStatWorker_StatOffsetFromGear(gear, stat, __result, __state, __exception) ?? __result;
        }


        [HarmonyFinalizer]
        [HarmonyPatch(
            typeof(StatWorker),
            "InfoTextLineFromGear"
            )]
        private static void FinalStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, ref string __result, Dictionary<string, object?> __state, Exception __exception)
        {
            IStatGearAffectPatcher? proccesser = gear as IStatGearAffectPatcher;
            __result = proccesser?.FinalStatWorker_InfoTextLineFromGear(gear, stat, __result, __state, __exception) ?? __result;
        }
    }
}

namespace RW_NodeTree
{
    public partial interface IStatGearAffectPatcher
    {
        bool PreStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, Dictionary<string, object?> stats);
        bool PreStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, Dictionary<string, object?> stats);
        bool PreStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, Dictionary<string, object?> stats);
        bool PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object?> stats);
        float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object?> stats);
        string PostStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object?> stats);
        IEnumerable<Thing> PostStatWorker_RelevantGear(Thing gear, StatDef stat, IEnumerable<Thing> result);
        bool FinalStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object?> stats, Exception exception);
        float FinalStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object?> stats, Exception exception);
        string FinalStatWorker_InfoTextLineFromGear(Thing gear, StatDef stat, string result, Dictionary<string, object?> stats, Exception exception);
    }
}