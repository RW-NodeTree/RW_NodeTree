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
        private readonly static MethodInfo _PostStatWorker_GetValueUnfinalized = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_GetValueUnfinalized", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static MethodInfo _PostStatWorker_FinalizeValue = typeof(StatWorker_Patcher).GetMethod("PostStatWorker_FinalizeValue", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly static Type[] StatWorker_GetValueUnfinalized_ParmsType = new Type[] { typeof(StatRequest), typeof(bool) };
        private readonly static Type[] StatWorker_FinalizeValue_ParmsType;
        static StatWorker_Patcher()
        {
            MethodInfo _FinalizeValue = typeof(StatWorker).GetMethod(
                "FinalizeValue"
            );
            ParameterInfo[] array = _FinalizeValue.GetParameters();
            StatWorker_FinalizeValue_ParmsType = new Type[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                StatWorker_FinalizeValue_ParmsType[i] = array[i].ParameterType;
            }
        }

        private static void PostStatWorker_GetValueUnfinalized(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float __result)
        {
            if (__originalMethod
                ==
                __instance.GetType().GetMethod(
                "GetValueUnfinalized",
                StatWorker_GetValueUnfinalized_ParmsType
            ))
                ((Comp_ChildNodeProccesser)req.Thing)?.PostStatWorker_GetValueUnfinalized(ref __result, __instance, req, applyPostProcess);
        }
        private static void PostStatWorker_FinalizeValue(StatWorker __instance, MethodInfo __originalMethod, StatRequest req, bool applyPostProcess, ref float val)
        {
            if (
                __originalMethod 
                ==
                __instance.GetType().GetMethod(
                "FinalizeValue",
                StatWorker_FinalizeValue_ParmsType
            ))
                ((Comp_ChildNodeProccesser)req.Thing)?.PostStatWorker_FinalizeValue(ref val, __instance, req, applyPostProcess);
        }

        public static void PatchValue(Type type, Harmony patcher)
        {
            if (typeof(StatWorker).IsAssignableFrom(type))
            {
                MethodInfo _GetValueUnfinalized = type.GetMethod(
                    "GetValueUnfinalized",
                    StatWorker_GetValueUnfinalized_ParmsType
                );
                if (_GetValueUnfinalized?.DeclaringType == type && _GetValueUnfinalized.HasMethodBody())
                {
                    patcher.Patch(_GetValueUnfinalized, null, new HarmonyMethod(_PostStatWorker_GetValueUnfinalized));
                    //if(Prefs.DevMode) Log.Message(type + "::" + _GetValueUnfinalized + " PatchSuccess\n");
                }
                MethodInfo _FinalizeValue = type.GetMethod(
                    "FinalizeValue",
                    StatWorker_FinalizeValue_ParmsType
                );
                if (_FinalizeValue?.DeclaringType == type && _FinalizeValue.HasMethodBody())
                {
                    patcher.Patch(_FinalizeValue, null, new HarmonyMethod(_PostStatWorker_FinalizeValue));
                    //if (Prefs.DevMode) Log.Message(type + "::" + _FinalizeValue + " PatchSuccess\n");
                }
            }
        }
    }
}
