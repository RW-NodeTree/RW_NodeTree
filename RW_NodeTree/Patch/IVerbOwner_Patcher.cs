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
    internal static partial class IVerbOwner_Patcher
    {
        private readonly static MethodInfo _PreIVerbOwner_GetVerbProperties = typeof(IVerbOwner_Patcher).GetMethod("PreIVerbOwner_GetVerbProperties", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PreIVerbOwner_GetTools = typeof(IVerbOwner_Patcher).GetMethod("PreIVerbOwner_GetTools", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostIVerbOwner_GetVerbProperties = typeof(IVerbOwner_Patcher).GetMethod("PostIVerbOwner_GetVerbProperties", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostIVerbOwner_GetTools = typeof(IVerbOwner_Patcher).GetMethod("PostIVerbOwner_GetTools", BindingFlags.NonPublic | BindingFlags.Static);



        private static void PreIVerbOwner_GetVerbProperties(IVerbOwner __instance, MethodInfo __originalMethod, ref Dictionary<string ,object> __state)
        {
            Type type = __instance.GetType();
            if (__originalMethod.DeclaringType
                ==
                type.GetMethod("get_VerbProperties", BindingFlags.Public | BindingFlags.Instance).DeclaringType
            )
            {
                __state = new Dictionary<string ,object>();
                CompChildNodeProccesser proccess = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
                proccess?.PreIVerbOwner_GetVerbProperties(type, __state);
            }
        }

        private static void PreIVerbOwner_GetTools(IVerbOwner __instance, MethodInfo __originalMethod, ref Dictionary<string, object> __state)
        {
            Type type = __instance.GetType();
            if (__originalMethod.DeclaringType
                ==
                type.GetMethod("get_Tools", BindingFlags.Public | BindingFlags.Instance).DeclaringType
            )
            {
                __state = new Dictionary<string, object>();
                CompChildNodeProccesser proccess = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
                proccess?.PreIVerbOwner_GetTools(type, __state);
            }
        }
        private static void PostIVerbOwner_GetVerbProperties(IVerbOwner __instance, MethodInfo __originalMethod, ref List<VerbProperties> __result, ref Dictionary<string, object> __state)
        {
            Type type = __instance.GetType();
            if (__originalMethod.DeclaringType
                ==
                type.GetMethod("get_VerbProperties", BindingFlags.Public | BindingFlags.Instance).DeclaringType
            )
            {
                __result = (__result != null) ? new List<VerbProperties>(__result) : new List<VerbProperties>();
                CompChildNodeProccesser proccess = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
                __result = proccess?.PostIVerbOwner_GetVerbProperties(type, __result, __state) ?? __result;
            }
        }

        private static void PostIVerbOwner_GetTools(IVerbOwner __instance, MethodInfo __originalMethod, ref List<Tool> __result, ref Dictionary<string, object> __state)
        {
            Type type = __instance.GetType();
            if (__originalMethod.DeclaringType
                ==
                type.GetMethod("get_Tools", BindingFlags.Public | BindingFlags.Instance).DeclaringType
            )
            {
                __result = (__result != null) ? new List<Tool>(__result) : new List<Tool>();
                CompChildNodeProccesser proccess = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
                __result = proccess?.PostIVerbOwner_GetTools(type, __result, __state) ?? __result;
            }
        }

        public static void PatchIVerbOwner(Type type, Harmony patcher)
        {
            if (typeof(IVerbOwner).IsAssignableFrom(type))
            {
                MethodInfo _get_VerbProperties = type.GetMethod("get_VerbProperties", BindingFlags.Public | BindingFlags.Instance);
                if (_get_VerbProperties?.DeclaringType == type && _get_VerbProperties.HasMethodBody())
                {
                    patcher.Patch(_get_VerbProperties, new HarmonyMethod(_PreIVerbOwner_GetVerbProperties), new HarmonyMethod(_PostIVerbOwner_GetVerbProperties));
                    //if (Prefs.DevMode) Log.Message(type + "::" + _get_VerbProperties + " PatchSuccess\n");
                }

                MethodInfo _get_Tools = type.GetMethod("get_Tools", BindingFlags.Public | BindingFlags.Instance);
                if (_get_Tools?.DeclaringType == type && _get_Tools.HasMethodBody())
                {
                    patcher.Patch(_get_Tools, new HarmonyMethod(_PreIVerbOwner_GetTools), new HarmonyMethod(_PostIVerbOwner_GetTools));
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
        public void PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object> forPostRead)
        {
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    comp.PreIVerbOwner_GetVerbProperties(ownerType, forPostRead);
                }
            }
            return;
        }

        /// <summary>
        /// event proccesser before IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        public void PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object> forPostRead)
        {
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    comp.PreIVerbOwner_GetTools(ownerType, forPostRead);
                }
            }
            return;
        }
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        public List<VerbProperties> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties> verbProperties, Dictionary<string, object> forPostRead)
        {
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    verbProperties = comp.PostIVerbOwner_GetVerbProperties(ownerType, verbProperties, forPostRead) ?? verbProperties;
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
        public List<Tool> PostIVerbOwner_GetTools(Type ownerType, List<Tool> tools, Dictionary<string, object> forPostRead)
        {
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    tools = comp.PostIVerbOwner_GetTools(ownerType, tools, forPostRead) ?? tools;
                }
            }
            return tools;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        public virtual void PreIVerbOwner_GetVerbProperties(Type ownerType, Dictionary<string, object> forPostRead)
        {
            return;
        }
        public virtual void PreIVerbOwner_GetTools(Type ownerType, Dictionary<string, object> forPostRead)
        {
            return;
        }
        public virtual List<VerbProperties> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbProperties> result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
        public virtual List<Tool> PostIVerbOwner_GetTools(Type ownerType, List<Tool> result, Dictionary<string, object> forPostRead)
        {
            return result;
        }
    }
}