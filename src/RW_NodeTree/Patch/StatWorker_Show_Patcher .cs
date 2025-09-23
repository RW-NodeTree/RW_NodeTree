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
        private static readonly MethodInfo _PreStatWorker_ShouldShowFor = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_ShouldShowFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PreStatWorker_IsDisabledFor = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_IsDisabledFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_ShouldShowFor = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_ShouldShowFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_IsDisabledFor = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_IsDisabledFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_ShouldShowFor = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_ShouldShowFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_IsDisabledFor = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_IsDisabledFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_ShouldShowFor_ParmsType = new Type[] { typeof(StatRequest) };
        private static readonly Type[] StatWorker_IsDisabledFor_ParmsType = new Type[] { typeof(Thing) };

        private static readonly Dictionary<Type, MethodInfo> MethodInfo_ShouldShowFor_OfType = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> MethodInfo_IsDisabledFor_OfType = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetMethodInfo_ShouldShowFor_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_ShouldShowFor_OfType.TryGetValue(type, out result))
            {
                MethodInfo_ShouldShowFor_OfType.Add(type,
                    result = type.GetMethod(
                        "ShouldShowFor",
                        StatWorker_ShouldShowFor_ParmsType
                    )
                );
            }
            return result;
        }
        private static MethodInfo GetMethodInfo_IsDisabledFor_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_IsDisabledFor_OfType.TryGetValue(type, out result))
            {
                MethodInfo_IsDisabledFor_OfType.Add(type,
                    result = type.GetMethod(
                        "IsDisabledFor",
                        StatWorker_IsDisabledFor_ParmsType
                    )
                );
            }
            return result;
        }

        private static bool PreStatWorker_ShouldShowFor(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ref (Dictionary<string, object?>, IStatShowPatcher) __state)
        {
            IStatShowPatcher? processer = req.Thing as IStatShowPatcher;
            if (processer != null &&
                __originalMethod.MethodHandle == GetMethodInfo_ShouldShowFor_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = processer;
                return processer.PreStatWorker_ShouldShowFor(__instance, StatWorker_stat(__instance), __state.Item1);
            }
            return true;
        }
        private static bool PreStatWorker_IsDisabledFor(StatWorker __instance, MethodInfo __originalMethod, Thing thing, ref (Dictionary<string, object?>, IStatShowPatcher) __state)
        {
            IStatShowPatcher? processer = thing as IStatShowPatcher;
            if (processer != null &&
                __originalMethod.MethodHandle == GetMethodInfo_IsDisabledFor_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = processer;
                return processer.PreStatWorker_IsDisabledFor(__instance, StatWorker_stat(__instance), __state.Item1);
            }
            return true;
        }
        private static void PostStatWorker_ShouldShowFor(StatWorker __instance, ref bool __result, (Dictionary<string, object?>, IStatShowPatcher) __state)
        {
            (Dictionary<string, object?> stats, IStatShowPatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                __result = processer.PostStatWorker_ShouldShowFor(__instance, StatWorker_stat(__instance), __result, stats);
        }
        private static void PostStatWorker_IsDisabledFor(StatWorker __instance, ref bool __result, (Dictionary<string, object?>, IStatShowPatcher) __state)
        {
            (Dictionary<string, object?> stats, IStatShowPatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                __result = processer.PostStatWorker_IsDisabledFor(__instance, StatWorker_stat(__instance), __result, stats);
        }
        private static void FinalStatWorker_ShouldShowFor(StatWorker __instance, ref bool __result, (Dictionary<string, object?>, IStatShowPatcher) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, IStatShowPatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                __result = processer.FinalStatWorker_ShouldShowFor(__instance, StatWorker_stat(__instance), __result, stats, __exception);
        }
        private static void FinalStatWorker_IsDisabledFor(StatWorker __instance, ref bool __result, (Dictionary<string, object?>, IStatShowPatcher) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, IStatShowPatcher processer) = __state;
            if (stats != null &&
                processer != null
            )
                __result = processer.FinalStatWorker_IsDisabledFor(__instance, StatWorker_stat(__instance), __result, stats, __exception);
        }

        public static void PatchShouldShowForAndIsDisabledFor(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _ShouldShowFor = GetMethodInfo_ShouldShowFor_OfType(type);
                if (_ShouldShowFor?.DeclaringType == type && _ShouldShowFor.HasMethodBody())
                {
                    patcher.Patch(
                        _ShouldShowFor,
                        new HarmonyMethod(_PreStatWorker_ShouldShowFor),
                        new HarmonyMethod(_PostStatWorker_ShouldShowFor),
                        null,
                        new HarmonyMethod(_FinalStatWorker_ShouldShowFor)
                        );
                    //if(Prefs.DevMode) Log.Message(type + "::" + _ShouldShowFor + " PatchSuccess\n");
                }
                MethodInfo _IsDisabledFor = GetMethodInfo_IsDisabledFor_OfType(type);
                if (_IsDisabledFor?.DeclaringType == type && _IsDisabledFor.HasMethodBody())
                {
                    patcher.Patch(
                        _IsDisabledFor,
                        new HarmonyMethod(_PreStatWorker_IsDisabledFor),
                        new HarmonyMethod(_PostStatWorker_IsDisabledFor),
                        null,
                        new HarmonyMethod(_FinalStatWorker_IsDisabledFor)
                        );
                    //if (Prefs.DevMode) Log.Message(type + "::" + _IsDisabledFor + " PatchSuccess\n");
                }
            }
        }
    }
    
    public partial interface IStatShowPatcher
    {
        bool PreStatWorker_ShouldShowFor(StatWorker statWorker, StatDef stateDef, Dictionary<string, object?> stats);
        bool PreStatWorker_IsDisabledFor(StatWorker statWorker, StatDef stateDef, Dictionary<string, object?> stats);
        bool PostStatWorker_ShouldShowFor(StatWorker statWorker, StatDef stateDef, bool result, Dictionary<string, object?> stats);
        bool PostStatWorker_IsDisabledFor(StatWorker statWorker, StatDef stateDef, bool result, Dictionary<string, object?> stats);
        bool FinalStatWorker_ShouldShowFor(StatWorker statWorker, StatDef stateDef, bool result, Dictionary<string, object?> stats, Exception exception);
        bool FinalStatWorker_IsDisabledFor(StatWorker statWorker, StatDef stateDef, bool result, Dictionary<string, object?> stats, Exception exception);

    }
}