using HarmonyLib;
using System;
using System.Collections.Generic;
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
            List<(Thing, List<Tool>, List<VerbProperties>)> state = new List<(Thing, List<Tool>, List<VerbProperties>)>();
            ThingOwner list = instance.equipment?.GetDirectlyHeldThings();
            if (list != null)
            {
                state.Capacity += list.Count;
                foreach (Thing thing in list)
                {
                    state.Add((thing, thing.def.tools, ThingDef_verbs(thing.def)));
                    try
                    {
                        List<Verb> verbs = thing.TryGetComp<CompEquippable>().AllVerbs;
                        ThingDef_verbs(thing.def) = new List<VerbProperties>();
                        thing.def.tools = new List<Tool>();
                        foreach (Verb verb in verbs)
                        {
                            if (verb.tool == null) ThingDef_verbs(thing.def).Add(verb.verbProps);
                            else thing.def.tools.Add(verb.tool);
                        }
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }

            result = new List<Gizmo>(result);

            foreach (Gizmo gizmo in result) yield return gizmo;

            foreach ((Thing thing, List<Tool> tools, List<VerbProperties> verbs) in state)
            {
                thing.def.tools = tools;
                ThingDef_verbs(thing.def) = verbs;
            }
            // return result;
        }

        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
    }
}
