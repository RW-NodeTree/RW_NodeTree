using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
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
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn),
            "GetGizmos"
        )]
        private static void PostPawn_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = PerAndPostFixFor_Pawn_GetGizmos(__instance, __result) ?? __result;
        }

        private static IEnumerable<Gizmo> PerAndPostFixFor_Pawn_GetGizmos(Pawn instance, IEnumerable<Gizmo> result)
        {
            ThingOwner list = instance.equipment.GetDirectlyHeldThings();
            List<(Thing, List<VerbProperties>)> state = new List<(Thing, List<VerbProperties>)>(list.Count);
            foreach (Thing thing in list)
            {
                ThingDef_verbs(thing.def) = ThingDef_verbs(thing.def) ?? new List<VerbProperties>();
                state.Add((thing, new List<VerbProperties>(thing.def.Verbs)));
                List<Verb> verbs = thing.TryGetComp<CompEquippable>().AllVerbs;
                thing.def.Verbs.Clear();
                foreach (Verb verb in verbs)
                {
                    if (verb.tool == null) thing.def.Verbs.Add(verb.verbProps);
                }
            }

            foreach (Gizmo gizmo in result) yield return gizmo;

            foreach ((Thing thing, List<VerbProperties> verbs) in state)
            {
                thing.def.Verbs.Clear();
                thing.def.Verbs.AddRange(verbs);
            }
        }

        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
    }
}
