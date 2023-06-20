using HarmonyLib;
using RW_NodeTree.Tools;
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



        private static bool PreIVerbOwner_GetVerbProperties(IVerbOwner __instance, MethodInfo __originalMethod, ref (Dictionary<string ,object>, CompChildNodeProccesser, Type) __state)
        {
            Type type = __instance.GetType();
            CompChildNodeProccesser proccesser = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
            if (proccesser!= null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_get_VerbProperties_OfType(type).DeclaringType
            )
            {
                __state.Item1 = new Dictionary<string ,object>();
                __state.Item2 = proccesser;
                __state.Item3 = type;
                return proccesser.PreIVerbOwner_GetVerbProperties(type, __state.Item1);
            }
            return true;
        }
        private static bool PreIVerbOwner_GetTools(IVerbOwner __instance, MethodInfo __originalMethod, ref (Dictionary<string, object>, CompChildNodeProccesser, Type) __state)
        {
            Type type = __instance.GetType();
            CompChildNodeProccesser proccesser = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
            if (proccesser != null &&
                __originalMethod.DeclaringType
                ==
                GetMethodInfo_get_Tools_OfType(type).DeclaringType
            )
            {
                __state.Item1 = new Dictionary<string, object>();
                __state.Item2 = proccesser;
                __state.Item3 = type;
                return proccesser.PreIVerbOwner_GetTools(type, __state.Item1);
            }
            return true;
        }
        private static void PostIVerbOwner_GetVerbProperties(ref List<VerbProperties> __result, (Dictionary<string, object>, CompChildNodeProccesser, Type) __state)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccess, Type type) = __state;
            if (stats != null &&
                proccess != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<VerbProperties>(__result) : new List<VerbProperties>();
                __result = proccess.PostIVerbOwner_GetVerbProperties(type, __result, stats) ?? __result;
            }
        }
        private static void PostIVerbOwner_GetTools(ref List<Tool> __result, (Dictionary<string, object>, CompChildNodeProccesser, Type) __state)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccesser, Type type) = __state;
            if (stats != null &&
                proccesser != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<Tool>(__result) : new List<Tool>();
                __result = proccesser.PostIVerbOwner_GetTools(type, __result, stats) ?? __result;
            }
        }
        private static void FinalIVerbOwner_GetVerbProperties(ref List<VerbProperties> __result, (Dictionary<string, object>, CompChildNodeProccesser, Type) __state, Exception __exception)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccess, Type type) = __state;
            if (stats != null &&
                proccess != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<VerbProperties>(__result) : new List<VerbProperties>();
                __result = proccess.FinalIVerbOwner_GetVerbProperties(type, __result, stats, __exception) ?? __result;
            }
        }
        private static void FinalIVerbOwner_GetTools(ref List<Tool> __result, (Dictionary<string, object>, CompChildNodeProccesser, Type) __state, Exception __exception)
        {
            (Dictionary<string, object> stats, CompChildNodeProccesser proccesser, Type type) = __state;
            if (stats != null &&
                proccesser != null &&
                type != null
            )
            {
                __result = (__result != null) ? new List<Tool>(__result) : new List<Tool>();
                __result = proccesser.FinalIVerbOwner_GetTools(type, __result, stats, __exception) ?? __result;
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
        public bool PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
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
            }
            return result;
        }

        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        public bool PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object> stats)
        {
            UpdateNode();
            bool result = true;
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
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
            }
            return result;
        }


        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        public List<VerbProperties> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties> verbProperties, Dictionary<string, object> stats)
        {
            UpdateNode();
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
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
            }
            return verbProperties;
        }

        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        public List<Tool> PostIVerbOwner_GetTools(Type ownerType, List<Tool> tools, Dictionary<string, object> stats)
        {
            UpdateNode();
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
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
            }
            return tools;
        }
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        public List<VerbProperties> FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties> verbProperties, Dictionary<string, object> stats, Exception exception)
        {
            UpdateNode();
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                List<VerbPropertiesRegiestInfo> verbPropertiesRegiestInfo;
                if (!regiestedNodeVerbPropertiesInfos.TryGetValue(ownerType, out verbPropertiesRegiestInfo))
                {
                    verbPropertiesRegiestInfo = new List<VerbPropertiesRegiestInfo>(verbProperties.Count);
                    foreach (VerbProperties verbProperty in verbProperties)
                    {
                        verbPropertiesRegiestInfo.Add(new VerbPropertiesRegiestInfo(null, verbProperty, verbProperty));
                    }
                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        try
                        {
                            verbPropertiesRegiestInfo = comp.internal_FinalIVerbOwner_GetVerbProperties(ownerType, verbPropertiesRegiestInfo, stats, exception) ?? verbPropertiesRegiestInfo;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                    for (int i = verbPropertiesRegiestInfo.Count - 1; i >= 0; i--)
                    {
                        if (!verbPropertiesRegiestInfo[i].Vaildity)
                        {
                            verbPropertiesRegiestInfo.RemoveAt(i);
                        }
                    }
                    regiestedNodeVerbPropertiesInfos.Add(ownerType, verbPropertiesRegiestInfo);
                }
                verbProperties = new List<VerbProperties>(verbPropertiesRegiestInfo.Count);
                foreach(VerbPropertiesRegiestInfo regiestInfo in verbPropertiesRegiestInfo)
                {
                    verbProperties.Add(regiestInfo.afterConvertProperties);
                }
            }
            return verbProperties;
        }

        /// <summary>
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        public List<Tool> FinalIVerbOwner_GetTools(Type ownerType, List<Tool> tools, Dictionary<string, object> stats, Exception exception)
        {
            UpdateNode();
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                List<VerbToolRegiestInfo> verbToolRegiestInfo;
                if (!regiestedNodeVerbToolInfos.TryGetValue(ownerType,out verbToolRegiestInfo))
                {
                    verbToolRegiestInfo = new List<VerbToolRegiestInfo>(tools.Count);
                    foreach (Tool tool in tools)
                    {
                        verbToolRegiestInfo.Add(new VerbToolRegiestInfo(null, tool, tool));
                    }
                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        try
                        {
                            verbToolRegiestInfo = comp.internal_FinalIVerbOwner_GetTools(ownerType, verbToolRegiestInfo, stats, exception) ?? verbToolRegiestInfo;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                    for (int i = verbToolRegiestInfo.Count - 1; i >= 0; i--)
                    {
                        if (!verbToolRegiestInfo[i].Vaildity)
                        {
                            verbToolRegiestInfo.RemoveAt(i);
                        }
                    }
                    for(int i = 0; i < verbToolRegiestInfo.Count; i++)
                    {
                        VerbToolRegiestInfo regiestInfo = verbToolRegiestInfo[i];
                        regiestInfo.afterCobvertTool = Gen.MemberwiseClone(regiestInfo.afterCobvertTool);
                        regiestInfo.afterCobvertTool.id = i.ToString();
                        verbToolRegiestInfo[i] = regiestInfo;
                    }
                    regiestedNodeVerbToolInfos.Add(ownerType, verbToolRegiestInfo);
                }
                tools = new List<Tool>(verbToolRegiestInfo.Count);
                foreach (VerbToolRegiestInfo regiestInfo in verbToolRegiestInfo)
                {
                    tools.Add(regiestInfo.afterCobvertTool);
                }
            }
            return tools;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual bool PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual bool PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object> stats)
        {
            return true;
        }
        protected virtual List<VerbProperties> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties> verbProperties, Dictionary<string, object> stats)
        {
            return verbProperties;
        }
        protected virtual List<Tool> PostIVerbOwner_GetTools(Type ownerType, List<Tool> tools, Dictionary<string, object> stats)
        {
            return tools;
        }
        protected virtual List<VerbPropertiesRegiestInfo> FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }
        protected virtual List<VerbToolRegiestInfo> FinalIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> stats, Exception exception)
        {
            return result;
        }
        internal bool internal_PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object> stats)
            => PreIVerbOwner_GetVerbProperties(ownerType, stats);
        internal bool internal_PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object> stats)
            => PreIVerbOwner_GetTools(ownerType, stats);
        internal List<VerbProperties> internal_PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties> verbProperties, Dictionary<string, object> stats)
            => PostIVerbOwner_GetVerbProperties(ownerType, verbProperties, stats);
        internal List<Tool> internal_PostIVerbOwner_GetTools(Type ownerType, List<Tool> tools, Dictionary<string, object> stats)
            => PostIVerbOwner_GetTools(ownerType, tools, stats);
        internal List<VerbPropertiesRegiestInfo> internal_FinalIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> stats, Exception exception)
            => FinalIVerbOwner_GetVerbProperties(ownerType, result, stats, exception);
        internal List<VerbToolRegiestInfo> internal_FinalIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> stats, Exception exception)
            => FinalIVerbOwner_GetTools(ownerType, result, stats, exception);
    }
}