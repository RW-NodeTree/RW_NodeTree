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
        private static readonly MethodInfo _PreStatWorker_GetValueUnfinalized = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetValueUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PreStatWorker_FinalizeValue = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_FinalizeValue", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetValueUnfinalized = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetValueUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_FinalizeValue = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_FinalizeValue", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_GetValueUnfinalized = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_GetValueUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_FinalizeValue = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_FinalizeValue", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_GetValueUnfinalized_ParmsType = new Type[] { typeof(StatRequest), typeof(bool) };
        private static readonly Type[] StatWorker_FinalizeValue_ParmsType = new Type[] { typeof(StatRequest), typeof(float).MakeByRefType(), typeof(bool) };

        private static readonly Dictionary<Type, MethodInfo> MethodInfo_GetValueUnfinalized_OfType = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> MethodInfo_FinalizeValue_OfType = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetMethodInfo_GetValueUnfinalized_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_GetValueUnfinalized_OfType.TryGetValue(type, out result))
            {
                MethodInfo_GetValueUnfinalized_OfType.Add(type,
                    result = type.GetMethod(
                        "GetValueUnfinalized",
                        StatWorker_GetValueUnfinalized_ParmsType
                    )
                );
            }
            return result;
        }
        private static MethodInfo GetMethodInfo_FinalizeValue_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_FinalizeValue_OfType.TryGetValue(type, out result))
            {
                MethodInfo_FinalizeValue_OfType.Add(type,
                    result = type.GetMethod(
                        "FinalizeValue",
                        StatWorker_FinalizeValue_ParmsType
                    )
                );
            }
            return result;
        }

        private static bool PreStatWorker_GetValueUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref (Dictionary<string, object?>, IStatValuePatcher) __state)
        {
            IStatValuePatcher? processer = req.Thing as IStatValuePatcher;
            if (processer != null &&
                __originalMethod.MethodHandle == GetMethodInfo_GetValueUnfinalized_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = processer;
                return processer.PreStatWorker_GetValueUnfinalized(__instance, StatWorker_stat(__instance), req, applyPostProcess, __state.Item1);
            }
            return true;
        }
        private static bool PreStatWorker_FinalizeValue(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float val, ref (Dictionary<string, object?>, IStatValuePatcher) __state)
        {
            IStatValuePatcher? processer = req.Thing as IStatValuePatcher;
            if (processer != null &&
                __originalMethod.MethodHandle == GetMethodInfo_FinalizeValue_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = processer;
                return processer.PreStatWorker_FinalizeValue(__instance, StatWorker_stat(__instance), req, applyPostProcess, ref val, __state.Item1);
            }
            return true;
        }
        private static void PostStatWorker_GetValueUnfinalized(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result, (Dictionary<string, object?>, IStatValuePatcher) __state)
        {
            (Dictionary<string, object?> stats, IStatValuePatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                __result = processer.PostStatWorker_GetValueUnfinalized(__instance, StatWorker_stat(__instance), req, applyPostProcess, __result, __state.Item1);
        }
        private static void PostStatWorker_FinalizeValue(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float val, (Dictionary<string, object?>, IStatValuePatcher) __state)
        {
            (Dictionary<string, object?> stats, IStatValuePatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                val = processer.PostStatWorker_FinalizeValue(__instance, StatWorker_stat(__instance), req, applyPostProcess, val, __state.Item1);
        }
        private static void FinalStatWorker_GetValueUnfinalized(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result, (Dictionary<string, object?>, IStatValuePatcher) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, IStatValuePatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                __result = processer.FinalStatWorker_GetValueUnfinalized(__instance, StatWorker_stat(__instance), req, applyPostProcess, __result, __state.Item1, __exception);
        }
        private static void FinalStatWorker_FinalizeValue(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float val, (Dictionary<string, object?>, IStatValuePatcher) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, IStatValuePatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                val = processer.FinalStatWorker_FinalizeValue(__instance, StatWorker_stat(__instance), req, applyPostProcess, val, __state.Item1, __exception);
        }

        public static void PatchValue(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetValueUnfinalized = GetMethodInfo_GetValueUnfinalized_OfType(type);
                if (_GetValueUnfinalized?.DeclaringType == type && _GetValueUnfinalized.HasMethodBody())
                {
                    patcher.Patch(
                        _GetValueUnfinalized,
                        new HarmonyMethod(_PreStatWorker_GetValueUnfinalized),
                        new HarmonyMethod(_PostStatWorker_GetValueUnfinalized),
                        null,
                        new HarmonyMethod(_FinalStatWorker_GetValueUnfinalized)
                        );
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetValueUnfinalized + " PatchSuccess\n");
                }
                MethodInfo _FinalizeValue = GetMethodInfo_FinalizeValue_OfType(type);
                if (_FinalizeValue?.DeclaringType == type && _FinalizeValue.HasMethodBody())
                {
                    patcher.Patch(
                        _FinalizeValue,
                        new HarmonyMethod(_PreStatWorker_FinalizeValue),
                        new HarmonyMethod(_PostStatWorker_FinalizeValue),
                        null,
                        new HarmonyMethod(_FinalStatWorker_FinalizeValue)
                        );
                    //if (Prefs.DevMode) Log.Message(type + "::" + _FinalizeValue + " PatchSuccess\n");
                }
            }
        }
    }
    
    public partial interface IStatValuePatcher
    {
        bool PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatDef stateDef, StatRequest req, bool applyPostProcess, Dictionary<string, object?> stats);

        bool PreStatWorker_FinalizeValue(StatWorker statWorker, StatDef stateDef, StatRequest req, bool applyPostProcess, ref float value, Dictionary<string, object?> stats);

        float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatDef stateDef, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object?> stats);

        float PostStatWorker_FinalizeValue(StatWorker statWorker, StatDef stateDef, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object?> stats);

        float FinalStatWorker_GetValueUnfinalized(StatWorker statWorker, StatDef stateDef, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object?> stats, Exception exception);

        float FinalStatWorker_FinalizeValue(StatWorker statWorker, StatDef stateDef, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object?> stats, Exception exception);

    }
}