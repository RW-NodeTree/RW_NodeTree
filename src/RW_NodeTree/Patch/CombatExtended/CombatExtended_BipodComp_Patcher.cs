using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace RW_NodeTree.Patch.CombatExtended
{
    internal static class CombatExtended_BipodComp_Patcher
    {
        private static MethodInfo _PreBipodComp_ResetVerbProps = typeof(CombatExtended_BipodComp_Patcher).GetMethod("PreBipodComp_ResetVerbProps", BindingFlags.Static | BindingFlags.NonPublic);
        private static Type CombatExtended_BipodComp = GenTypes.GetTypeInAnyAssembly("CombatExtended.BipodComp");
        private static Type CombatExtended_VerbPropertiesCE = GenTypes.GetTypeInAnyAssembly("CombatExtended.VerbPropertiesCE");

        private static MethodInfo? BipodComp_AssignVerbProps;

        private static bool PreBipodComp_ResetVerbProps(ThingComp __instance, Thing? source)
        {
            CompChildNodeProccesser? comp = source;
            if (comp != null)
            {
                VerbProperties? props = source.TryGetComp<CompEquippable>()?.VerbProperties?.Find(x => CombatExtended_VerbPropertiesCE.IsAssignableFrom(x.GetType()));
                if (props != null)
                {
                    BipodComp_AssignVerbProps?.Invoke(__instance, new object?[] { source, props });
                    return false;
                }
            }
            return true;
        }


        public static void PatchCompResetVerbProps(Harmony patcher)
        {
            if (CombatExtended_BipodComp != null && CombatExtended_VerbPropertiesCE != null)
            {
                BipodComp_AssignVerbProps = CombatExtended_BipodComp.GetMethod("AssignVerbProps", BindingFlags.Instance | BindingFlags.Public);
                MethodInfo target = CombatExtended_BipodComp.GetMethod("ResetVerbProps", BindingFlags.Instance | BindingFlags.Public);
                patcher.Patch(
                    target,
                    prefix: new HarmonyMethod(_PreBipodComp_ResetVerbProps)
                    );
            }
        }
    }
}
