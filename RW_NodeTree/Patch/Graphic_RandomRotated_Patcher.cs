using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{
    [HarmonyPatch(typeof(Graphic_RandomRotated))]
    internal static class Graphic_RandomRotated_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Graphic_RandomRotated),
            "DrawWorker"
        )]
        private static bool PreGraphic_RandomRotated_DrawWorker(Graphic_RandomRotated __instance, Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thing != null)
            {
                float maxAngle = Graphic_RandomRotated_MaxAngle(__instance);
                extraRotation += -maxAngle + (float)(thing.thingIDNumber * 542) % (maxAngle * 2f);
            }
            Graphic_RandomRotated_SubGraphic(__instance).DrawWorker(loc, rot, thingDef, thing, extraRotation);
            return false;
        }
        private static AccessTools.FieldRef<Graphic_RandomRotated, Graphic> Graphic_RandomRotated_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_RandomRotated), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, float> Graphic_RandomRotated_MaxAngle = AccessTools.FieldRefAccess<float>(typeof(Graphic_RandomRotated), "maxAngle");
    }
}
