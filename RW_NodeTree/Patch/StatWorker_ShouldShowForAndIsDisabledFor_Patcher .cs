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
        private static readonly MethodInfo _PreStatWorker_ShouldShowFor = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_ShouldShowFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PreStatWorker_IsDisabledFor = typeof(StatWorker_Patcher).GetMethod("PreStatWorker_IsDisabledFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_ShouldShowFor = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_ShouldShowFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostStatWorker_IsDisabledFor = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_IsDisabledFor", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_ShouldShowFor_ParmsType = new Type[] { typeof(StatRequest)};
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

        private static void PreStatWorker_ShouldShowFor(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ref Dictionary<string, object> __state)
        {
            if (__originalMethod.DeclaringType
                ==
                GetMethodInfo_ShouldShowFor_OfType(__instance.GetType()).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                req.Thing.RootNode()?.PreStatWorker_ShouldShowFor(__instance, req, __state);
            }
        }
        private static void PreStatWorker_IsDisabledFor(StatWorker __instance, MethodInfo __originalMethod, Thing thing, ref Dictionary<string, object> __state)
        {
            if (
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_IsDisabledFor_OfType(__instance.GetType()).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                thing.RootNode()?.PreStatWorker_IsDisabledFor(__instance, thing, __state);
            }
        }
        private static void PostStatWorker_ShouldShowFor(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ref bool __result, ref Dictionary<string, object> __state)
        {
            if (__originalMethod.DeclaringType
                ==
                GetMethodInfo_ShouldShowFor_OfType(__instance.GetType()).DeclaringType
            )
                __result = req.Thing.RootNode()?.PostStatWorker_ShouldShowFor(__instance, req, __result, __state) ?? __result;
        }
        private static void PostStatWorker_IsDisabledFor(StatWorker __instance, MethodInfo __originalMethod, Thing thing, ref bool __result, ref Dictionary<string, object> __state)
        {
            if (
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_IsDisabledFor_OfType(__instance.GetType()).DeclaringType
            )
                __result = thing.RootNode()?.PostStatWorker_IsDisabledFor(__instance, thing, __result, __state) ?? __result;
        }

        public static void PatchShouldShowForAndIsDisabledFor(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _ShouldShowFor = GetMethodInfo_ShouldShowFor_OfType(type);
                if (_ShouldShowFor?.DeclaringType == type && _ShouldShowFor.HasMethodBody())
                {
                    patcher.Patch(_ShouldShowFor, new HarmonyMethod(_PreStatWorker_ShouldShowFor), new HarmonyMethod(_PostStatWorker_ShouldShowFor));
                    if(Prefs.DevMode) Log.Message(type + "::" + _ShouldShowFor + " PatchSuccess\n");
                }
                MethodInfo _IsDisabledFor = GetMethodInfo_IsDisabledFor_OfType(type);
                if (_IsDisabledFor?.DeclaringType == type && _IsDisabledFor.HasMethodBody())
                {
                    patcher.Patch(_IsDisabledFor, new HarmonyMethod(_PreStatWorker_IsDisabledFor), new HarmonyMethod(_PostStatWorker_IsDisabledFor));
                    if (Prefs.DevMode) Log.Message(type + "::" + _IsDisabledFor + " PatchSuccess\n");
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
        /// event proccesser before StatWorker.ShouldShowFor()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.ShouldShowFor)
        /// </summary>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.ShouldShowFor()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.ShouldShowFor()</param>
        public void PreStatWorker_ShouldShowFor(StatWorker statWorker, StatRequest req, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_ShouldShowFor(statWorker, req, forPostRead);
            }
        }

        /// <summary>
        /// event proccesser before StatWorker.IsDisabledFor()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.IsDisabledFor)
        /// </summary>
        /// <param name="result">result of StatWorker.IsDisabledFor(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.IsDisabledFor()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.IsDisabledFor()</param>
        public void PreStatWorker_IsDisabledFor(StatWorker statWorker, Thing thing, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_PreStatWorker_IsDisabledFor(statWorker, thing, forPostRead);
            }
        }

        /// <summary>
        /// event proccesser after StatWorker.ShouldShowFor()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.ShouldShowFor)
        /// </summary>
        /// <param name="result">result of StatWorker.ShouldShowFor(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.ShouldShowFor()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.ShouldShowFor()</param>
        public bool PostStatWorker_ShouldShowFor(StatWorker statWorker, StatRequest req, bool result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_ShouldShowFor(statWorker, req, result, forPostRead);
            }
            return result;
        }

        /// <summary>
        /// event proccesser after StatWorker.IsDisabledFor()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.IsDisabledFor)
        /// </summary>
        /// <param name="result">result of StatWorker.IsDisabledFor(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.IsDisabledFor()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.IsDisabledFor()</param>
        public bool PostStatWorker_IsDisabledFor(StatWorker statWorker, Thing thing, bool result, Dictionary<string, object> forPostRead)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.internal_PostStatWorker_IsDisabledFor(statWorker, thing, result, forPostRead);
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual void PreStatWorker_ShouldShowFor(StatWorker statWorker, StatRequest req, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual void PreStatWorker_IsDisabledFor(StatWorker statWorker, Thing thing, Dictionary<string, object> forPostRead)
        {
            return;
        }
        protected virtual bool PostStatWorker_ShouldShowFor(StatWorker statWorker, StatRequest req, bool result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        protected virtual bool PostStatWorker_IsDisabledFor(StatWorker statWorker, Thing thing, bool result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        internal void internal_PreStatWorker_ShouldShowFor(StatWorker statWorker, StatRequest req, Dictionary<string, object> forPostRead)
            => PreStatWorker_ShouldShowFor(statWorker, req, forPostRead);
        internal void internal_PreStatWorker_IsDisabledFor(StatWorker statWorker, Thing thing, Dictionary<string, object> forPostRead)
            => PreStatWorker_IsDisabledFor(statWorker, thing, forPostRead);
        internal bool internal_PostStatWorker_ShouldShowFor(StatWorker statWorker, StatRequest req, bool result, Dictionary<string, object> forPostRead)
            => PostStatWorker_ShouldShowFor(statWorker, req, result, forPostRead);
        internal bool internal_PostStatWorker_IsDisabledFor(StatWorker statWorker, Thing thing, bool result, Dictionary<string, object> forPostRead)
            => PostStatWorker_IsDisabledFor(statWorker, thing, result, forPostRead);

    }
}