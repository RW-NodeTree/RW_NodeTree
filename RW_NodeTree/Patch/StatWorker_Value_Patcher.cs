using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
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
        private readonly static MethodInfo _PreStatWorker_GetValueUnfinalized = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetValueUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PreStatWorker_FinalizeValue = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_FinalizeValue", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostStatWorker_GetValueUnfinalized = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetValueUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostStatWorker_FinalizeValue = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_FinalizeValue", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static Type[] StatWorker_GetValueUnfinalized_ParmsType = new Type[] { typeof(StatRequest), typeof(bool) };
        private readonly static Type[] StatWorker_FinalizeValue_ParmsType = new Type[] { typeof(StatRequest), typeof(float).MakeByRefType(), typeof(bool) };

        private static void PreStatWorker_GetValueUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref Dictionary<string, object> __state)
        {
            if (__originalMethod.DeclaringType
                ==
                __instance.GetType().GetMethod(
                    "GetValueUnfinalized",
                    StatWorker_GetValueUnfinalized_ParmsType
                ).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                ((CompChildNodeProccesser)req.Thing)?.PreStatWorker_GetValueUnfinalized(__instance, req, applyPostProcess, __state);
            }
        }
        private static void PreStatWorker_FinalizeValue(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float val, ref Dictionary<string, object> __state)
        {
            if (
                __originalMethod.DeclaringType
                ==
                __instance.GetType().GetMethod(
                    "FinalizeValue",
                    StatWorker_FinalizeValue_ParmsType
                ).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                val = ((CompChildNodeProccesser)req.Thing)?.PreStatWorker_FinalizeValue(__instance, req, applyPostProcess, val, __state) ?? val;
            }
        }
        private static void PostStatWorker_GetValueUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float __result, ref Dictionary<string, object> __state)
        {
            if (__originalMethod.DeclaringType
                ==
                __instance.GetType().GetMethod(
                    "GetValueUnfinalized",
                    StatWorker_GetValueUnfinalized_ParmsType
                ).DeclaringType
            )
                __result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_GetValueUnfinalized(__instance, req, applyPostProcess, __result, __state) ?? __result;
        }
        private static void PostStatWorker_FinalizeValue(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float val, ref Dictionary<string, object> __state)
        {
            if (
                __originalMethod.DeclaringType
                ==
                __instance.GetType().GetMethod(
                    "FinalizeValue",
                    StatWorker_FinalizeValue_ParmsType
                ).DeclaringType
            )
                val = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_FinalizeValue(__instance, req, applyPostProcess, val, __state) ?? val;
        }

        public static void PatchValue(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetValueUnfinalized = type.GetMethod(
                    "GetValueUnfinalized",
                    StatWorker_GetValueUnfinalized_ParmsType
                );
                if (_GetValueUnfinalized?.DeclaringType == type && _GetValueUnfinalized.HasMethodBody())
                {
                    patcher.Patch(_GetValueUnfinalized, new HarmonyMethod(_PreStatWorker_GetValueUnfinalized), new HarmonyMethod(_PostStatWorker_GetValueUnfinalized));
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetValueUnfinalized + " PatchSuccess\n");
                }
                MethodInfo _FinalizeValue = type.GetMethod(
                    "FinalizeValue",
                    StatWorker_FinalizeValue_ParmsType
                );
                if (_FinalizeValue?.DeclaringType == type && _FinalizeValue.HasMethodBody())
                {
                    patcher.Patch(_FinalizeValue, new HarmonyMethod(_PreStatWorker_FinalizeValue), new HarmonyMethod(_PostStatWorker_FinalizeValue));
                    if (Prefs.DevMode) Log.Message(type + "::" + _FinalizeValue + " PatchSuccess\n");
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
        public void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, forPostRead);
            }
        }

        /// <summary>
        /// event proccesser before StatWorker.FinalizeValue()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.FinalizeValue)
        /// </summary>
        /// <param name="result">result of StatWorker.FinalizeValue(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.FinalizeValue()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.FinalizeValue()</param>
        public float PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PreStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead);
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
        public float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, forPostRead);
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
        public float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead);
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual float PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        internal void internal_PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
            => PreStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, forPostRead);
        internal float internal_PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
            => PreStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead);
        internal float internal_PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
            => PostStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, forPostRead);
        internal float internal_PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
            => PostStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead);

    }
}