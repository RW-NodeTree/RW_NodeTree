using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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

        private static bool PreStatWorker_GetValueUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref (Dictionary<string, object>, CompChildNodeProccesser) __state)
        {
            CompChildNodeProccesser proccesser = req.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.MethodHandle == GetMethodInfo_GetValueUnfinalized_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object>();
                __state.Item2 = proccesser;
                return proccesser.PreStatWorker_GetValueUnfinalized(__instance, req, applyPostProcess, __state.Item1);
            }
            return true;
        }
        private static bool PreStatWorker_FinalizeValue(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float val, ref (Dictionary<string, object>, CompChildNodeProccesser) __state)
        {
            CompChildNodeProccesser proccesser = req.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.MethodHandle == GetMethodInfo_FinalizeValue_OfType(__instance.GetType()).MethodHandle
            )
            {
                __state.Item1 = new Dictionary<string, object>();
                __state.Item2 = proccesser;
                return proccesser.PreStatWorker_FinalizeValue(__instance, req, applyPostProcess, ref val, __state.Item1);
            }
            return true;
        }
        private static void PostStatWorker_GetValueUnfinalized(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result, (Dictionary<string, object>, CompChildNodeProccesser) __state)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
                __result = proccesser.PostStatWorker_GetValueUnfinalized(__instance, req, applyPostProcess, __result, __state.Item1);
        }
        private static void PostStatWorker_FinalizeValue(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float val, (Dictionary<string, object>, CompChildNodeProccesser) __state)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
                val = proccesser.PostStatWorker_FinalizeValue(__instance, req, applyPostProcess, val, __state.Item1);
        }
        private static void FinalStatWorker_GetValueUnfinalized(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result, (Dictionary<string, object>, CompChildNodeProccesser) __state, Exception __exception)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
                __result = proccesser.FinalStatWorker_GetValueUnfinalized(__instance, req, applyPostProcess, __result, __state.Item1, __exception);
        }
        private static void FinalStatWorker_FinalizeValue(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float val, (Dictionary<string, object>, CompChildNodeProccesser) __state, Exception __exception)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccesser) = __state;
            if (stats != null &&
                proccesser != null
            )
                val = proccesser.FinalStatWorker_FinalizeValue(__instance, req, applyPostProcess, val, __state.Item1, __exception);
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
}

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {

        /// <summary>
        /// event proccesser before StatWorker.GetValueUnfinalized()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.GetValueUnfinalized)
        /// </summary>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetValueUnfinalized()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.GetValueUnfinalized()</param>
        internal bool PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser before StatWorker.FinalizeValue()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.FinalizeValue)
        /// </summary>
        /// <param name="value">result of StatWorker.FinalizeValue(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.FinalizeValue()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.FinalizeValue()</param>
        internal bool PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, ref float value, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreStatWorker_FinalizeValue(statWorker, req, applyPostProcess, ref value, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.GetValueUnfinalized()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.GetValueUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetValueUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetValueUnfinalized()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.GetValueUnfinalized()</param>
        internal float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, stats);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.FinalizeValue()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.FinalizeValue)
        /// </summary>
        /// <param name="result">result of StatWorker.FinalizeValue(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.FinalizeValue()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.FinalizeValue()</param>
        internal float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, stats);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.GetValueUnfinalized()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.GetValueUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetValueUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetValueUnfinalized()</param>
        /// <param name="applyFinalProcess">parm 'applyFinalProcess' of StatWorker.GetValueUnfinalized()</param>
        internal float FinalStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, stats, exception);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.FinalizeValue()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.FinalizeValue)
        /// </summary>
        /// <param name="result">result of StatWorker.FinalizeValue(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.FinalizeValue()</param>
        /// <param name="applyFinalProcess">parm 'applyFinalProcess' of StatWorker.FinalizeValue()</param>
        internal float FinalStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_FinalStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, stats, exception);
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
        protected virtual bool PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual bool PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, ref float value, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats)
        {
            return result;
        }
        protected virtual float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats)
        {
            return result;
        }
        protected virtual float FinalStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }
        protected virtual float FinalStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }
        internal bool internal_PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> stats)
            => PreStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, stats);
        internal bool internal_PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, ref float value, Dictionary<string, object> stats)
            => PreStatWorker_FinalizeValue(statWorker, req, applyPostProcess, ref value, stats);
        internal float internal_PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats)
            => PostStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, stats);
        internal float internal_PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats)
            => PostStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, stats);
        internal float internal_FinalStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats, Exception exception)
            => FinalStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, stats, exception);
        internal float internal_FinalStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> stats, Exception exception)
            => FinalStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, stats, exception);

    }
}