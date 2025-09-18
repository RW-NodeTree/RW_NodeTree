using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch
{
    internal static partial class StatWorker_Patcher
    {
        private static readonly MethodInfo _PostStatWorker_GetInfoCardHyperlinks = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetInfoCardHyperlinks", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type[] StatWorker_GetInfoCardHyperlinks_ParmsType = new Type[] { typeof(StatRequest) };

        private static readonly Dictionary<Type, MethodInfo> MethodInfo_GetInfoCardHyperlinks_OfType = new Dictionary<Type, MethodInfo>();

        private static MethodInfo GetMethodInfo_GetInfoCardHyperlinks_OfType(Type type)
        {
            MethodInfo result;
            if (!MethodInfo_GetInfoCardHyperlinks_OfType.TryGetValue(type, out result))
            {
                MethodInfo_GetInfoCardHyperlinks_OfType.Add(type,
                    result = type.GetMethod(
                        "GetInfoCardHyperlinks",
                        StatWorker_GetInfoCardHyperlinks_ParmsType
                    )
                );
            }
            return result;
        }

        private static void PostStatWorker_GetInfoCardHyperlinks(StatWorker __instance, MethodInfo __originalMethod, StatRequest statRequest, ref IEnumerable<Dialog_InfoCard.Hyperlink> __result)
        {
            //if (Prefs.DevMode) Log.Message("__originalMethod.GetType() : " + __originalMethod.GetType() + "; _GetInfoCardHyperlinks.GetType() : " + _GetInfoCardHyperlinks.GetType() + "; same : " + (_GetInfoCardHyperlinks == __originalMethod));
            IStatHyperlinksPatcher? processer = statRequest.Thing as IStatHyperlinksPatcher;
            if (processer != null &&
                __originalMethod.MethodHandle == GetMethodInfo_GetInfoCardHyperlinks_OfType(__instance.GetType()).MethodHandle
            )
                __result = processer.PostStatWorker_GetInfoCardHyperlinks(__instance, StatWorker_stat(__instance), statRequest, __result) ?? __result;
        }

        public static void PatchGetInfoCardHyperlinks(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetInfoCardHyperlinks = GetMethodInfo_GetInfoCardHyperlinks_OfType(type);
                if (_GetInfoCardHyperlinks?.DeclaringType == type && _GetInfoCardHyperlinks.HasMethodBody())
                {
                    patcher.Patch(_GetInfoCardHyperlinks, null, new HarmonyMethod(_PostStatWorker_GetInfoCardHyperlinks));
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetInfoCardHyperlinks + " PatchSuccess\n");
                }
            }
        }
    }
    
    public partial interface IStatHyperlinksPatcher
    {

        IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatDef stateDef, StatRequest statRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result);
    }
}