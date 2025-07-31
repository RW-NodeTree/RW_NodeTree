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
            CompChildNodeProccesser? proccesser = statRequest.Thing.RootNode();
            if (proccesser != null &&
                __originalMethod.MethodHandle == GetMethodInfo_GetInfoCardHyperlinks_OfType(__instance.GetType()).MethodHandle
            )
                __result = proccesser.PostStatWorker_GetInfoCardHyperlinks(__instance, statRequest, __result) ?? __result;
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
}

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {


        /// <summary>
        /// event proccesser after StatWorker.GetInfoCardHyperlinks()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetInfoCardHyperlinks)
        /// </summary>
        /// <param name="result">result of StatWorker.GetInfoCardHyperlinks(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="reqstatRequest">parm 'reqstatRequest' of StatWorker.GetInfoCardHyperlinks()</param>
        internal IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest statRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            UpdateNode();
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                try
                {
                    result = comp.internal_PostStatWorker_GetInfoCardHyperlinks(statWorker, statRequest, result) ?? result;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {

        protected virtual IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest statRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            return result;
        }
        internal IEnumerable<Dialog_InfoCard.Hyperlink> internal_PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest statRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
            => PostStatWorker_GetInfoCardHyperlinks(statWorker, statRequest, result);
    }
}