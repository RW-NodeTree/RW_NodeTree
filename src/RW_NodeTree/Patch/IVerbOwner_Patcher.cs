using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch
{
    internal static partial class IVerbOwner_Patcher
    {
        private static readonly MethodInfo _PreIVerbOwner_GetVerbProperties = typeof(IVerbOwner_Patcher).GetMethod("PreIVerbOwner_GetVerbProperties", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PreIVerbOwner_GetTools = typeof(IVerbOwner_Patcher).GetMethod("PreIVerbOwner_GetTools", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostIVerbOwner_GetVerbProperties = typeof(IVerbOwner_Patcher).GetMethod("PostIVerbOwner_GetVerbProperties", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _PostIVerbOwner_GetTools = typeof(IVerbOwner_Patcher).GetMethod("PostIVerbOwner_GetTools", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalIVerbOwner_GetVerbProperties = typeof(IVerbOwner_Patcher).GetMethod("FinalIVerbOwner_GetVerbProperties", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _FinalIVerbOwner_GetTools = typeof(IVerbOwner_Patcher).GetMethod("FinalIVerbOwner_GetTools", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly Dictionary<Type, MethodInfo> MethodInfo_get_VerbProperties_OfType = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> MethodInfo_get_Tools_OfType = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetMethodInfo_get_VerbProperties_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_get_VerbProperties_OfType.TryGetValue(type, out result))
            {
                MethodInfo_get_VerbProperties_OfType.Add(
                    type,
                    result = type.GetMethod("get_VerbProperties", BindingFlags.Public | BindingFlags.Instance)
                );
            }
            return result;
        }
        private static MethodInfo GetMethodInfo_get_Tools_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_get_Tools_OfType.TryGetValue(type, out result))
            {
                MethodInfo_get_Tools_OfType.Add(
                    type,
                    result = type.GetMethod("get_Tools", BindingFlags.Public | BindingFlags.Instance)
                );
            }
            return result;
        }



        private static bool PreIVerbOwner_GetVerbProperties(IVerbOwner __instance, MethodInfo __originalMethod, ref (Dictionary<string, object?>, CompChildNodeProccesser, Type) __state)
        {
            Type type = __instance.GetType();
            CompChildNodeProccesser? proccesser = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
            if (proccesser != null &&
                CompChildNodeProccesser.GetSameTypeVerbOwner(type, proccesser.parent) == __instance &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_get_VerbProperties_OfType(type).DeclaringType
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = proccesser;
                __state.Item3 = type;
                return proccesser.PreIVerbOwner_GetVerbProperties(type, __state.Item1);
            }
            return true;
        }
        private static bool PreIVerbOwner_GetTools(IVerbOwner __instance, MethodInfo __originalMethod, ref (Dictionary<string, object?>, CompChildNodeProccesser, Type) __state)
        {
            Type type = __instance.GetType();
            CompChildNodeProccesser? proccesser = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
            if (proccesser != null &&
                CompChildNodeProccesser.GetSameTypeVerbOwner(type, proccesser.parent) == __instance &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_get_Tools_OfType(type).DeclaringType
            )
            {
                __state.Item1 = new Dictionary<string, object?>();
                __state.Item2 = proccesser;
                __state.Item3 = type;
                return proccesser.PreIVerbOwner_GetTools(type, __state.Item1);
            }
            return true;
        }
        private static void PostIVerbOwner_GetVerbProperties(ref List<VerbProperties?>? __result, (Dictionary<string, object?>, CompChildNodeProccesser, Type) __state)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccess, Type type) = __state;
            if (stats != null &&
                proccess != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<VerbProperties?>(__result) : new List<VerbProperties?>();
                __result = proccess.PostIVerbOwner_GetVerbProperties(type, __result, stats) ?? __result;
            }
        }
        private static void PostIVerbOwner_GetTools(ref List<Tool?>? __result, (Dictionary<string, object?>, CompChildNodeProccesser, Type) __state)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccesser, Type type) = __state;
            if (stats != null &&
                proccesser != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<Tool?>(__result) : new List<Tool?>();
                __result = proccesser.PostIVerbOwner_GetTools(type, __result, stats) ?? __result;
            }
        }
        private static void FinalIVerbOwner_GetVerbProperties(ref List<VerbProperties?>? __result, (Dictionary<string, object?>, CompChildNodeProccesser, Type) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccess, Type type) = __state;
            if (stats != null &&
                proccess != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<VerbProperties?>(__result) : new List<VerbProperties?>();
                __result = (proccess.FinalIVerbOwner_GetVerbProperties(type, __result, stats, __exception) as List<VerbProperties?>) ?? __result;
            }
        }
        private static void FinalIVerbOwner_GetTools(ref List<Tool?>? __result, (Dictionary<string, object?>, CompChildNodeProccesser, Type) __state, Exception __exception)
        {
            (Dictionary<string, object?> stats, CompChildNodeProccesser proccesser, Type type) = __state;
            if (stats != null &&
                proccesser != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<Tool?>(__result) : new List<Tool?>();
                __result = (proccesser.FinalIVerbOwner_GetTools(type, __result, stats, __exception) as List<Tool?>) ?? __result;
            }
        }
        public static void PatchIVerbOwner(Type type, Harmony patcher)
        {
            if (typeof(IVerbOwner).IsAssignableFrom(type))
            {
                MethodInfo _get_VerbProperties = GetMethodInfo_get_VerbProperties_OfType(type);
                if (_get_VerbProperties?.DeclaringType == type && _get_VerbProperties.HasMethodBody())
                {
                    patcher.Patch(
                        _get_VerbProperties,
                        new HarmonyMethod(_PreIVerbOwner_GetVerbProperties),
                        new HarmonyMethod(_PostIVerbOwner_GetVerbProperties),
                        null,
                        new HarmonyMethod(_FinalIVerbOwner_GetVerbProperties)
                        );
                    //if (Prefs.DevMode) Log.Message(type + "::" + _get_VerbProperties + " PatchSuccess\n");
                }

                MethodInfo _get_Tools = GetMethodInfo_get_Tools_OfType(type);
                if (_get_Tools?.DeclaringType == type && _get_Tools.HasMethodBody())
                {
                    patcher.Patch(
                        _get_Tools,
                        new HarmonyMethod(_PreIVerbOwner_GetTools),
                        new HarmonyMethod(_PostIVerbOwner_GetTools),
                        null,
                        new HarmonyMethod(_FinalIVerbOwner_GetTools)
                        );
                    //if (Prefs.DevMode) Log.Message(type + "::" + _get_Tools + " PatchSuccess\n");
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
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        internal bool PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object?> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreIVerbOwner_GetVerbProperties(ownerType, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        internal bool PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object?> stats)
        {
            UpdateNode();
            bool result = true;
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PreIVerbOwner_GetTools(ownerType, stats) && result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }


        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        internal List<VerbProperties?> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties?> verbProperties, Dictionary<string, object?> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    comp.internal_PostIVerbOwner_GetVerbProperties(ownerType, verbProperties, stats);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return verbProperties;
        }

        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        internal List<Tool?> PostIVerbOwner_GetTools(Type ownerType, List<Tool?> tools, Dictionary<string, object?> stats)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    comp.internal_PostIVerbOwner_GetTools(ownerType, tools, stats);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return tools;
        }
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        internal List<VerbProperties> FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties?> verbProperties, Dictionary<string, object?> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    verbProperties = comp.internal_FinalIVerbOwner_GetVerbProperties(ownerType, verbProperties, stats, exception) ?? verbProperties;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            lock (ChildNodes)
            {
                List<VerbPropertiesRegiestInfo> verbPropertiesRegiestInfo = internal_GetRegiestedNodeVerbPropertiesInfos(ownerType, verbProperties);
                List<VerbProperties> result = new List<VerbProperties>(verbPropertiesRegiestInfo.Count);
                foreach (VerbPropertiesRegiestInfo regiestInfo in verbPropertiesRegiestInfo)
                {
                    result.Add(regiestInfo.afterConvertProperties);
                }
                return result;
            }
        }

        /// <summary>
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        internal List<Tool> FinalIVerbOwner_GetTools(Type ownerType, List<Tool?> tools, Dictionary<string, object?> stats, Exception exception)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    tools = comp.internal_FinalIVerbOwner_GetTools(ownerType, tools, stats, exception) ?? tools;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            lock (ChildNodes)
            {
                List<VerbToolRegiestInfo> verbToolRegiestInfo = internal_GetRegiestedNodeVerbToolInfos(ownerType, tools);
                List<Tool> result = new List<Tool>(verbToolRegiestInfo.Count);
                foreach (VerbToolRegiestInfo regiestInfo in verbToolRegiestInfo)
                {
                    result.Add(regiestInfo.afterConvertTool);
                }
                return result;
            }
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual bool PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object?> stats)
        {
            return true;
        }
        protected virtual bool PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object?> stats)
        {
            return true;
        }
        protected virtual List<VerbProperties?> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties?> verbProperties, Dictionary<string, object?> stats)
        {
            return verbProperties;
        }
        protected virtual List<Tool?> PostIVerbOwner_GetTools(Type ownerType, List<Tool?> tools, Dictionary<string, object?> stats)
        {
            return tools;
        }
        protected virtual List<VerbProperties?> FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties?> result, Dictionary<string, object?> stats, Exception exception)
        {
            return result;
        }
        protected virtual List<Tool?> FinalIVerbOwner_GetTools(Type ownerType, List<Tool?> result, Dictionary<string, object?> stats, Exception exception)
        {
            return result;
        }
        internal bool internal_PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object?> stats)
            => PreIVerbOwner_GetVerbProperties(ownerType, stats);
        internal bool internal_PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object?> stats)
            => PreIVerbOwner_GetTools(ownerType, stats);
        internal List<VerbProperties?> internal_PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties?> verbProperties, Dictionary<string, object?> stats)
            => PostIVerbOwner_GetVerbProperties(ownerType, verbProperties, stats);
        internal List<Tool?> internal_PostIVerbOwner_GetTools(Type ownerType, List<Tool?> tools, Dictionary<string, object?> stats)
            => PostIVerbOwner_GetTools(ownerType, tools, stats);
        internal List<VerbProperties?> internal_FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties?> result, Dictionary<string, object?> stats, Exception exception)
            => FinalIVerbOwner_GetVerbProperties(ownerType, result, stats, exception);
        internal List<Tool?> internal_FinalIVerbOwner_GetTools(Type ownerType, List<Tool?> result, Dictionary<string, object?> stats, Exception exception)
            => FinalIVerbOwner_GetTools(ownerType, result, stats, exception);
    }
}