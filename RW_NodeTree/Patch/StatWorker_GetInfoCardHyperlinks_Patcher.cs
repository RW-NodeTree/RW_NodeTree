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
        private readonly static MethodInfo _PostStatWorker_GetInfoCardHyperlinks = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetInfoCardHyperlinks", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static Type[] StatWorker_GetInfoCardHyperlinks_ParmsType = new Type[] { typeof(StatRequest)};

        private static void PostStatWorker_GetInfoCardHyperlinks(StatWorker __instance, MethodInfo __originalMethod, StatRequest statRequest, ref IEnumerable<Dialog_InfoCard.Hyperlink> __result)
        {
            if (__originalMethod
                ==
                __instance.GetType().GetMethod(
                "GetInfoCardHyperlinks",
                StatWorker_GetInfoCardHyperlinks_ParmsType
            ))
                ((Comp_ChildNodeProccesser)statRequest.Thing)?.PostStatWorker_GetInfoCardHyperlinks(ref __result, __instance, statRequest);
        }

        public static void PatchGetInfoCardHyperlinks(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetInfoCardHyperlinks = type.GetMethod(
                    "GetInfoCardHyperlinks",
                    StatWorker_GetInfoCardHyperlinks_ParmsType
                );
                if (_GetInfoCardHyperlinks?.DeclaringType == type && _GetInfoCardHyperlinks.HasMethodBody())
                {
                    patcher.Patch(_GetInfoCardHyperlinks, null, new HarmonyMethod(_PostStatWorker_GetInfoCardHyperlinks));
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetValueUnfinalized + " PatchSuccess\n");
                }
            }
        }
    }
}
