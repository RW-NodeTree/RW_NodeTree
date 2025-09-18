using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch
{
    internal static partial class StatWorker_Patcher
    {
        private static readonly MethodInfo _PreStatWorker_GetStatDrawEntryLabel = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetStatDrawEntryLabel", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetStatDrawEntryLabel = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetStatDrawEntryLabel", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_GetStatDrawEntryLabel = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_GetStatDrawEntryLabel", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_GetStatDrawEntryLabel_ParmsType = new Type[] { typeof(StatDef), typeof(float), typeof(ToStringNumberSense), typeof(StatRequest), typeof(bool) };

        private static readonly Dictionary<Type, MethodInfo> MethodInfo_GetStatDrawEntryLabel_OfType = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetMethodInfo_GetStatDrawEntryLabel_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_GetStatDrawEntryLabel_OfType.TryGetValue(type, out result))
            {
                MethodInfo_GetStatDrawEntryLabel_OfType.Add(type,
                    result = type.GetMethod(
                        "GetStatDrawEntryLabel",
                        StatWorker_GetStatDrawEntryLabel_ParmsType
                    )
                );
            }
            return result;
        }

        private static bool PreStatWorker_GetStatDrawEntryLabel(StatWorker __instance, MethodInfo __originalMethod, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, ref (Dictionary<string, object?>, IStatLabelPatcher) __state)
        {
            IStatLabelPatcher? proccesser = optionalReq.Thing as IStatLabelPatcher;
            if (proccesser != null &&
                __originalMethod.MethodHandle == GetMethodInfo_GetStatDrawEntryLabel_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = proccesser;
                return proccesser.PreStatWorker_GetStatDrawEntryLabel(__instance, stat, value, numberSense, optionalReq, finalized, __state.Item1);
            }
            return true;
        }
        private static void PostStatWorker_GetStatDrawEntryLabel(StatWorker __instance, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, ref string __result, (Dictionary<string, object?>, IStatLabelPatcher) __state)
        {
            (Dictionary<string, object?> stats, IStatLabelPatcher proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
                __result = proccesser.PostStatWorker_GetStatDrawEntryLabel(__instance, stat, value, numberSense, optionalReq, finalized, __result, stats) ?? __result;
        }
        private static void FinalStatWorker_GetStatDrawEntryLabel(StatWorker __instance, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, ref string __result, (Dictionary<string, object?>, IStatLabelPatcher) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, IStatLabelPatcher proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
                __result = proccesser.FinalStatWorker_GetStatDrawEntryLabel(__instance, stat, value, numberSense, optionalReq, finalized, __result, stats, __exception) ?? __result;
        }

        public static void PatchStatDrawEntry(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetStatDrawEntryLabel = GetMethodInfo_GetStatDrawEntryLabel_OfType(type);
                if (_GetStatDrawEntryLabel?.DeclaringType == type && _GetStatDrawEntryLabel.HasMethodBody())
                {
                    patcher.Patch(
                        _GetStatDrawEntryLabel,
                        new HarmonyMethod(_PreStatWorker_GetStatDrawEntryLabel),
                        new HarmonyMethod(_PostStatWorker_GetStatDrawEntryLabel),
                        null,
                        new HarmonyMethod(_FinalStatWorker_GetStatDrawEntryLabel)
                        );
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetStatDrawEntryLabel + " PatchSuccess\n");
                }
            }
        }
    }
    
    public partial interface IStatLabelPatcher : IThingHolder
    {
        bool PreStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, Dictionary<string, object?> stats);
        string PostStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object?> stats);
        string FinalStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object?> stats, Exception exception);

    }
}