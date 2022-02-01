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

        private readonly static MethodInfo _PostStatWorker_GetExplanationUnfinalized = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetExplanationUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostStatWorker_GetExplanationFinalizePart = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetExplanationFinalizePart", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static Type[] StatWorker_GetExplanationUnfinalized_ParmsType = new Type[] { typeof(StatRequest), typeof(ToStringNumberSense) };
        private readonly static Type[] StatWorker_GetExplanationFinalizePart_ParmsType = new Type[] { typeof(StatRequest), typeof(ToStringNumberSense), typeof(float) };

        private static void PostStatWorker_GetExplanationUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, ToStringNumberSense numberSense, ref string __result)
        {
            if (__originalMethod
                !=
                __instance.GetType().GetMethod(
                "GetExplanationUnfinalized",
                StatWorker_GetExplanationUnfinalized_ParmsType
            ))
                return;
            Comp_ChildNodeProccesser proccess = req.Thing;
            if (proccess != null)
            {
                proccess.PostStatWorker_GetExplanationUnfinalized(ref __result, __instance, req, numberSense);
            }
        }
        private static void PostStatWorker_GetExplanationFinalizePart(StatWorker __instance, MethodBase __originalMethod, StatRequest req, ToStringNumberSense numberSense, float finalVal, ref string __result)
        {
            if (__originalMethod 
                !=
                __instance.GetType().GetMethod(
                "GetExplanationFinalizePart",
                StatWorker_GetExplanationFinalizePart_ParmsType
            )) 
                return;
            Comp_ChildNodeProccesser proccess = req.Thing;
            if (proccess != null)
            {
                proccess.PostStatWorker_GetExplanationFinalizePart(ref __result, __instance, req, numberSense, finalVal);
            }
        }

        public static void PatchExplanation(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetExplanationUnfinalized = type.GetMethod(
                    "GetExplanationUnfinalized",
                    StatWorker_GetExplanationUnfinalized_ParmsType
                );
                if (_GetExplanationUnfinalized?.DeclaringType == type && _GetExplanationUnfinalized.HasMethodBody())
                {
                    patcher.Patch(_GetExplanationUnfinalized, null, new HarmonyMethod(_PostStatWorker_GetExplanationUnfinalized));
                    if (Prefs.DevMode) Log.Message(type + "::" + _GetExplanationUnfinalized + " PatchSuccess\n");
                }
                MethodInfo _GetExplanationFinalizePart = type.GetMethod(
                    "GetExplanationFinalizePart",
                    StatWorker_GetExplanationFinalizePart_ParmsType
                );
                if (_GetExplanationFinalizePart?.DeclaringType == type && _GetExplanationFinalizePart.HasMethodBody())
                {
                    patcher.Patch(_GetExplanationFinalizePart, null, new HarmonyMethod(_PostStatWorker_GetExplanationFinalizePart));
                    if (Prefs.DevMode) Log.Message(type + "::" + _GetExplanationFinalizePart + " PatchSuccess\n");
                }
            }
        }
    }
}
