using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(ThingWithComps))]
    internal static partial class ThingWithComps_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(ThingWithComps),
            "PreApplyDamage"
        )]
        public static void TranspilThingWithComps_PreApplyDamage(ThingWithComps __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed)
            {
                return;
            }
            if (__instance.AllComps != null)
            {
                for (int i = 0; i < __instance.AllComps.Count; i++)
                {
                    (__instance.AllComps[i] as CompBasicNodeComp)?.internal_PostPreApplyDamageWithRef(ref dinfo, out absorbed);
                    if (absorbed)
                    {
                        return;
                    }
                }
            }
        }
    }
}

namespace RW_NodeTree
{
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        protected virtual void PostPreApplyDamageWithRef(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
        }
        internal void internal_PostPreApplyDamageWithRef(ref DamageInfo dinfo, out bool absorbed) => PostPreApplyDamageWithRef(ref dinfo, out absorbed);

    }
}