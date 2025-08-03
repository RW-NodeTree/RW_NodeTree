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

        private static readonly MethodInfo _PreStatWorker_GetExplanationUnfinalized = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetExplanationUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PreStatWorker_GetExplanationFinalizePart = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetExplanationFinalizePart", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetExplanationUnfinalized = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetExplanationUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetExplanationFinalizePart = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetExplanationFinalizePart", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_GetExplanationUnfinalized = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_GetExplanationUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalStatWorker_GetExplanationFinalizePart = typeof(StatWorker_Patcher).GetMethod("FinalStatWorker_GetExplanationFinalizePart", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_GetExplanationUnfinalized_ParmsType = new Type[] { typeof(StatRequest), typeof(ToStringNumberSense) };
        private static readonly Type[] StatWorker_GetExplanationFinalizePart_ParmsType = new Type[] { typeof(StatRequest), typeof(ToStringNumberSense), typeof(float) };

        private static readonly Dictionary<Type, MethodInfo> MethodInfo_GetExplanationUnfinalized_OfType = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> MethodInfo_GetExplanationFinalizePart_OfType = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetMethodInfo_GetExplanationUnfinalized_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_GetExplanationUnfinalized_OfType.TryGetValue(type, out result))
            {
                MethodInfo_GetExplanationUnfinalized_OfType.Add(type,
                    result = type.GetMethod(
                        "GetExplanationUnfinalized",
                        StatWorker_GetExplanationUnfinalized_ParmsType
                    )
                );
            }
            return result;
        }
        private static MethodInfo GetMethodInfo_GetExplanationFinalizePart_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_GetExplanationFinalizePart_OfType.TryGetValue(type, out result))
            {
                MethodInfo_GetExplanationFinalizePart_OfType.Add(type,
                    result = type.GetMethod(
                        "GetExplanationFinalizePart",
                        StatWorker_GetExplanationFinalizePart_ParmsType
                    )
                );
            }
            return result;
        }

        private static bool PreStatWorker_GetExplanationUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ToStringNumberSense numberSense, ref (Dictionary<string, object?>, CompChildNodeProccesser) __state)
        {
            CompChildNodeProccesser? proccesser = req.Thing.RootNode();
            if (
                proccesser != null &&
                __originalMethod.MethodHandle == GetMethodInfo_GetExplanationUnfinalized_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = proccesser;
                return proccesser.PreStatWorker_GetExplanationUnfinalized(__instance, req, numberSense, __state.Item1);
            }
            return true;
        }
        private static bool PreStatWorker_GetExplanationFinalizePart(StatWorker __instance, MethodBase __originalMethod, StatRequest req, ToStringNumberSense numberSense, float finalVal, ref (Dictionary<string, object?>, CompChildNodeProccesser) __state)
        {
            CompChildNodeProccesser? proccesser = req.Thing.RootNode();
            if (
                proccesser != null &&
                __originalMethod.MethodHandle
                ==
                GetMethodInfo_GetExplanationFinalizePart_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = proccesser;
                return proccesser.PreStatWorker_GetExplanationFinalizePart(__instance, req, numberSense, finalVal, __state.Item1);
            }
            return true;
        }
        private static void PostStatWorker_GetExplanationUnfinalized(StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, ref string __result, (Dictionary<string, object?>, CompChildNodeProccesser) __state)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
            {
                __result = proccesser.PostStatWorker_GetExplanationUnfinalized(__instance, req, numberSense, __result, stats) ?? __result;
            }
        }
        private static void PostStatWorker_GetExplanationFinalizePart(StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, float finalVal, ref string __result, (Dictionary<string, object?>, CompChildNodeProccesser) __state)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
            {
                __result = proccesser.PostStatWorker_GetExplanationFinalizePart(__instance, req, numberSense, finalVal, __result, stats) ?? __result;
            }
        }
        private static void FinalStatWorker_GetExplanationUnfinalized(StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, ref string __result, (Dictionary<string, object?>, CompChildNodeProccesser) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
            {
                __result = proccesser.FinalStatWorker_GetExplanationUnfinalized(__instance, req, numberSense, __result, stats, __exception) ?? __result;
            }
        }
        private static void FinalStatWorker_GetExplanationFinalizePart(StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, float finalVal, ref string __result, (Dictionary<string, object?>, CompChildNodeProccesser) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
            {
                __result = proccesser.FinalStatWorker_GetExplanationFinalizePart(__instance, req, numberSense, finalVal, __result, stats, __exception) ?? __result;
            }
        }

        public static void PatchExplanation(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetExplanationUnfinalized = GetMethodInfo_GetExplanationUnfinalized_OfType(type);
                if (_GetExplanationUnfinalized?.DeclaringType == type && _GetExplanationUnfinalized.HasMethodBody())
                {
                    patcher.Patch(
                        _GetExplanationUnfinalized,
                        new HarmonyMethod(_PreStatWorker_GetExplanationUnfinalized),
                        new HarmonyMethod(_PostStatWorker_GetExplanationUnfinalized),
                        null,
                        new HarmonyMethod(_FinalStatWorker_GetExplanationUnfinalized)
                        );
                    //if (Prefs.DevMode) Log.Message(type + "::" + _GetExplanationUnfinalized + " PatchSuccess\n");
                }
                MethodInfo _GetExplanationFinalizePart = GetMethodInfo_GetExplanationFinalizePart_OfType(type);
                if (_GetExplanationFinalizePart?.DeclaringType == type && _GetExplanationFinalizePart.HasMethodBody())
                {
                    patcher.Patch(
                        _GetExplanationFinalizePart,
                        new HarmonyMethod(_PreStatWorker_GetExplanationFinalizePart),
                        new HarmonyMethod(_PostStatWorker_GetExplanationFinalizePart),
                        null,
                        new HarmonyMethod(_FinalStatWorker_GetExplanationFinalizePart)
                        );
                    //if (Prefs.DevMode) Log.Message(type + "::" + _GetExplanationFinalizePart + " PatchSuccess\n");
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

        /// <summary>
        /// event proccesser before StatWorker.GetExplanationUnfinalized()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationUnfinalized)
        /// </summary>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationUnfinalized()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationUnfinalized()</param>
        internal bool PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object?> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser before StatWorker.GetExplanationFinalizePart()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationFinalizePart)
        /// </summary>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="finalVal">parm 'finalVal' of StatWorker.GetExplanationFinalizePart()</param>
        internal bool PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object?> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }
        /// <summary>
        /// event proccesser after StatWorker.GetExplanationUnfinalized()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationUnfinalized()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationUnfinalized()</param>
        internal string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, stats) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.GetExplanationFinalizePart()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationFinalizePart)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationFinalizePart(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="finalVal">parm 'finalVal' of StatWorker.GetExplanationFinalizePart()</param>
        internal string PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, result, stats) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }
        /// <summary>
        /// event proccesser after StatWorker.GetExplanationUnfinalized()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationUnfinalized()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationUnfinalized()</param>
        internal string FinalStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, stats, exception) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.GetExplanationFinalizePart()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationFinalizePart)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationFinalizePart(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="finalVal">parm 'finalVal' of StatWorker.GetExplanationFinalizePart()</param>
        internal string FinalStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, result, stats, exception) ?? result;
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
        protected virtual bool PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object?> stats)
        {
            return true;
        }
        protected virtual bool PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object?> stats)
        {
            return true;
        }
        protected virtual string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats)
        {
            return result;
        }
        protected virtual string PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats)
        {
            return result;
        }
        protected virtual string FinalStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats, Exception exception)
        {
            return result;
        }
        protected virtual string FinalStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats, Exception exception)
        {
            return result;
        }
        internal bool internal_PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object?> stats)
            => PreStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, stats);
        internal bool internal_PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object?> stats)
            => PreStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, stats);
        internal string internal_PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats)
            => PostStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, stats);
        internal string internal_PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats)
            => PostStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, result, stats);
        internal string internal_FinalStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats, Exception exception)
            => FinalStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, stats, exception);
        internal string internal_FinalStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats, Exception exception)
            => FinalStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, result, stats, exception);
    }
}