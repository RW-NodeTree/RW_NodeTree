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
        private readonly static MethodInfo _PostVerbTracker_InitVerbs_GetVerbProperties = typeof(IVerbOwner_Patcher).GetMethod("PostIVerbOwner_GetVerbProperties", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostVerbTracker_InitVerbs_GetTools = typeof(IVerbOwner_Patcher).GetMethod("PostIVerbOwner_GetTools", BindingFlags.NonPublic | BindingFlags.Static);



        private static void PostIVerbOwner_GetVerbProperties(IVerbOwner __instance, MethodInfo __originalMethod, ref List<VerbProperties> __result)
        {
            if (__originalMethod
                ==
                __instance.GetType().GetMethod("get_VerbProperties", BindingFlags.Public | BindingFlags.Instance)
            )
            {
                Comp_ChildNodeProccesser proccess = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
                proccess?.PostIVerbOwner_GetVerbProperties(__instance, ref __result);
            }
        }

        private static void PostIVerbOwner_GetTools(IVerbOwner __instance, MethodInfo __originalMethod, ref List<Tool> __result)
        {
            if (__originalMethod
                ==
                __instance.GetType().GetMethod("get_Tools", BindingFlags.Public | BindingFlags.Instance)
            )
            {
                Comp_ChildNodeProccesser proccess = (((__instance) as ThingComp)?.parent) ?? ((__instance) as Thing);
                proccess?.PostIVerbOwner_GetTools(__instance, ref __result);
            }
        }

        public static void PatchIVerbOwner(Type type, Harmony patcher)
        {
            if (typeof(IVerbOwner).IsAssignableFrom(type))
            {
                MethodInfo _get_VerbProperties = type.GetMethod("get_VerbProperties", BindingFlags.Public | BindingFlags.Instance);
                if (_get_VerbProperties?.DeclaringType == type && _get_VerbProperties.HasMethodBody())
                {
                    patcher.Patch(_get_VerbProperties, null, new HarmonyMethod(_PostVerbTracker_InitVerbs_GetVerbProperties));
                    //if (Prefs.DevMode) Log.Message(type + "::" + _get_VerbProperties + " PatchSuccess\n");
                }

                MethodInfo _get_Tools = type.GetMethod("get_Tools", BindingFlags.Public | BindingFlags.Instance);
                if (_get_Tools?.DeclaringType == type && _get_Tools.HasMethodBody())
                {
                    patcher.Patch(_get_Tools, null, new HarmonyMethod(_PostVerbTracker_InitVerbs_GetTools));
                    //if (Prefs.DevMode) Log.Message(type + "::" + _get_Tools + " PatchSuccess\n");
                }
            }
        }
    }
}
