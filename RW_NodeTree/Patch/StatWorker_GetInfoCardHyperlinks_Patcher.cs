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

            MethodInfo _GetInfoCardHyperlinks = __instance.GetType().GetMethod(
                "GetInfoCardHyperlinks",
                StatWorker_GetInfoCardHyperlinks_ParmsType
            );
            //if (Prefs.DevMode) Log.Message(_GetInfoCardHyperlinks.DeclaringType + "::" + _GetInfoCardHyperlinks + " def " + __instance);
            if (__originalMethod.DeclaringType
                ==
                _GetInfoCardHyperlinks.DeclaringType
            )
            __result = ((CompChildNodeProccesser)statRequest.Thing)?.PostStatWorker_GetInfoCardHyperlinks(__instance, statRequest, __result) ?? __result;
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
        public IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest reqstatRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                result = comp.PostStatWorker_GetInfoCardHyperlinks(statWorker, reqstatRequest, result) ?? result;
            }
            return result;
        }
    }
    public abstract partial class CompBasicNodeComp : ThingComp
    {

        public virtual IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest reqstatRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            return result;
        }
    }
}