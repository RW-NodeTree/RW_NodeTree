using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
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

        private static readonly MethodInfo _PreStatWorker_GetExplanationUnfinalized = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetExplanationUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PreStatWorker_GetExplanationFinalizePart = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetExplanationFinalizePart", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetExplanationUnfinalized = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetExplanationUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetExplanationFinalizePart = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetExplanationFinalizePart", BindingFlags.NonPublic | BindingFlags.Static);
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

        private static void PreStatWorker_GetExplanationUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ToStringNumberSense numberSense, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser proccesser = req.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_GetExplanationUnfinalized_OfType(__instance.GetType()).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                proccesser.PreStatWorker_GetExplanationUnfinalized(__instance, req, numberSense, __state);
            }
        }
        private static void PreStatWorker_GetExplanationFinalizePart(StatWorker __instance, MethodBase __originalMethod, StatRequest req, ToStringNumberSense numberSense, float finalVal, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser proccesser = req.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_GetExplanationFinalizePart_OfType(__instance.GetType()).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                proccesser.PreStatWorker_GetExplanationFinalizePart(__instance, req, numberSense, finalVal, __state);
            }
        }
        private static void PostStatWorker_GetExplanationUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ToStringNumberSense numberSense, ref string __result, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser proccesser = req.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_GetExplanationUnfinalized_OfType(__instance.GetType()).DeclaringType
            )
            {
                __result = proccesser.PostStatWorker_GetExplanationUnfinalized(__instance, req, numberSense, __result, __state) ?? __result;
            }
        }
        private static void PostStatWorker_GetExplanationFinalizePart(StatWorker __instance, MethodBase __originalMethod, StatRequest req, ToStringNumberSense numberSense, float finalVal, ref string __result, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser proccesser = req.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_GetExplanationFinalizePart_OfType(__instance.GetType()).DeclaringType
            )
            {
                __result = proccesser.PostStatWorker_GetExplanationFinalizePart(__instance, req, numberSense, finalVal, __result, __state) ?? __result;
            }
        }

        public static void PatchExplanation(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetExplanationUnfinalized = GetMethodInfo_GetExplanationUnfinalized_OfType(type);
                if (_GetExplanationUnfinalized?.DeclaringType == type && _GetExplanationUnfinalized.HasMethodBody())
                {
                    patcher.Patch(_GetExplanationUnfinalized, new HarmonyMethod(_PreStatWorker_GetExplanationUnfinalized), new HarmonyMethod(_PostStatWorker_GetExplanationUnfinalized));
                    //if (Prefs.DevMode) Log.Message(type + "::" + _GetExplanationUnfinalized + " PatchSuccess\n");
                }
                MethodInfo _GetExplanationFinalizePart = GetMethodInfo_GetExplanationFinalizePart_OfType(type);
                if (_GetExplanationFinalizePart?.DeclaringType == type && _GetExplanationFinalizePart.HasMethodBody())
                {
                    patcher.Patch(_GetExplanationFinalizePart, new HarmonyMethod(_PreStatWorker_GetExplanationFinalizePart), new HarmonyMethod(_PostStatWorker_GetExplanationFinalizePart));
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
        public void PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, forPostRead);
            }
        }

        /// <summary>
        /// event proccesser before StatWorker.GetExplanationFinalizePart()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationFinalizePart)
        /// </summary>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="finalVal">parm 'finalVal' of StatWorker.GetExplanationFinalizePart()</param>
        public void PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, forPostRead);
            }
        }
        /// <summary>
        /// event proccesser after StatWorker.GetExplanationUnfinalized()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationUnfinalized()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationUnfinalized()</param>
        public string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, forPostRead) ?? result;
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
        public string PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, result, forPostRead) ?? result;
            }
            return result;
        }

    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual void PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual void PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual string PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        internal void internal_PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object> forPostRead)
            => PreStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, forPostRead);
        internal void internal_PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object> forPostRead)
            => PreStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, forPostRead);
        internal string internal_PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
            => PostStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, forPostRead);
        internal string internal_PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object> forPostRead)
            => PostStatWorker_GetExplanationFinalizePart(statWorker, req, numberSense, finalVal, result, forPostRead);
    }
}