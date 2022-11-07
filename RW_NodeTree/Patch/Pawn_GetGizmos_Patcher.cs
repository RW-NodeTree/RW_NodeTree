using HarmonyLib;
using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree.Patch
{

    [HarmonyPatch(typeof(Pawn))]
    internal static partial class Pawn_Patcher
    {

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(Pawn),
            "GetGizmos"
        )]
        private static void PrePawn_GetGizmos(Pawn __instance, ref Dictionary<Thing,List<VerbProperties>> __state)
        {
            ThingOwner list = __instance.equipment.GetDirectlyHeldThings();
            __state = new Dictionary<Thing, List<VerbProperties>>();
            foreach (Thing thing in list)
            {
                ThingDef_verbs(thing.def) = ThingDef_verbs(thing.def) ?? new List<VerbProperties>();
                __state.Add(thing, new List<VerbProperties>(thing.def.Verbs));
                List<Verb> verbs = thing.TryGetComp<CompEquippable>().AllVerbs;
                thing.def.Verbs.Clear();
                foreach (Verb verb in verbs)
                {
                    if (verb.tool == null) thing.def.Verbs.Add(verb.verbProps);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn),
            "GetGizmos"
        )]
        private static void PostPawn_GetGizmos(Pawn __instance, Dictionary<Thing, List<VerbProperties>> __state)
        {
            foreach ((Thing thing, List<VerbProperties> verbs) in __state)
            {
                thing.def.Verbs.Clear();
                thing.def.Verbs.AddRange(verbs);
            }
        }

        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
    }
}
