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
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{
    internal static partial class StatWorker_Patcher
    {
        private static readonly MethodInfo _PreStatWorker_GetStatDrawEntryLabel = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_GetStatDrawEntryLabel", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_GetStatDrawEntryLabel = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetStatDrawEntryLabel", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_GetStatDrawEntryLabel_ParmsType = new Type[] { typeof(StatDef), typeof(float), typeof(ToStringNumberSense), typeof(StatRequest), typeof(bool)};

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

        private static void PreStatWorker_GetStatDrawEntryLabel(StatWorker __instance, MethodInfo __originalMethod, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser proccesser = optionalReq.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_GetStatDrawEntryLabel_OfType(__instance.GetType()).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                proccesser.PreStatWorker_GetStatDrawEntryLabel(__instance, stat, value, numberSense, optionalReq, finalized, __state);
            }
        }
        private static void PostStatWorker_GetStatDrawEntryLabel(StatWorker __instance, MethodInfo __originalMethod, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, ref string __result, ref Dictionary<string, object> __state)
        {
            CompChildNodeProccesser proccesser = optionalReq.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_GetStatDrawEntryLabel_OfType(__instance.GetType()).DeclaringType
            )
                __result = proccesser.PostStatWorker_GetStatDrawEntryLabel(__instance, stat, value, numberSense, optionalReq, finalized, __result, __state) ?? __result;
        }

        public static void PatchStatDrawEntry(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetStatDrawEntryLabel = GetMethodInfo_GetStatDrawEntryLabel_OfType(type);
                if (_GetStatDrawEntryLabel?.DeclaringType == type && _GetStatDrawEntryLabel.HasMethodBody())
                {
                    patcher.Patch(_GetStatDrawEntryLabel, new HarmonyMethod(_PreStatWorker_GetStatDrawEntryLabel), new HarmonyMethod(_PostStatWorker_GetStatDrawEntryLabel));
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetStatDrawEntryLabel + " PatchSuccess\n");
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
        /// event proccesser before StatWorker.GetStatDrawEntryLabel()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.GetStatDrawEntryLabel)
        /// </summary>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetStatDrawEntryLabel()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.GetStatDrawEntryLabel()</param>
        public void PreStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_GetStatDrawEntryLabel(statWorker, stat, value, numberSense, optionalReq, finalized, forPostRead);
            }
        }

        /// <summary>
        /// event proccesser after StatWorker.GetStatDrawEntryLabel()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.GetStatDrawEntryLabel)
        /// </summary>
        /// <param name="result">result of StatWorker.GetStatDrawEntryLabel(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetStatDrawEntryLabel()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.GetStatDrawEntryLabel()</param>
        public string PostStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_GetStatDrawEntryLabel(statWorker, stat, value, numberSense, optionalReq, finalized, result, forPostRead) ?? result;
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual void PreStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual string PostStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        internal void internal_PreStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, Dictionary<string, object> forPostRead)
            => PreStatWorker_GetStatDrawEntryLabel(statWorker, stat, value, numberSense, optionalReq, finalized, forPostRead);
        internal string internal_PostStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object> forPostRead)
            => PostStatWorker_GetStatDrawEntryLabel(statWorker, stat, value, numberSense, optionalReq, finalized, result, forPostRead);

    }
}