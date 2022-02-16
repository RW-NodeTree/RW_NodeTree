using RimWorld;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.NodeComponent
{
    internal class NodeComp_Test : CompBasicNodeComp
    {

        public CompProperties_Test Props => (CompProperties_Test)props;

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (Validity)
            {
                CompChildNodeProccesser proccesser = NodeProccesser;
                for (int i = 0; i < Props.thingDefs.Count; i++)
                {
                    ThingDef def = Props.thingDefs[i];
                    Thing node = ThingMaker.MakeThing(def);
                    proccesser.AppendChild(node,"Node_" + i);
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            base.NodeProccesser.ResetRenderedTexture();
            ++frame;
            frame %= 360;
            //if (Prefs.DevMode) Log.Message("\"" + ((parent.def.uiIconPath == null) ? "null" : parent.def.uiIconPath) + "\"");
        }

        public override void AdapteDrawSteep(ref List<string> ids, ref List<Thing> nodes, ref List<List<RenderInfo>> renderInfos)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                Thing thing = nodes[i];
                if(thing != parent)
                {
                    List<RenderInfo> infos = renderInfos[i];
                    //infos.Add(new RenderInfo(thing.Graphic.MeshAt(Rot4.South), 0, Matrix4x4.identity, thing.Graphic.MatSingleFor(thing), 0));
                    for (int j = 0; j < infos.Count; j++)
                    {
                        RenderInfo info = infos[j];
                        for (int k = 0; k < info.matrices.Length; k++)
                        {
                            Vector4 cache = info.matrices[k].GetRow(0);
                            info.matrices[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, 0));

                            cache = info.matrices[k].GetRow(1);
                            info.matrices[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0));

                            cache = info.matrices[k].GetRow(2);
                            info.matrices[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, 1));

                            info.matrices[k] = Matrix4x4.Rotate(Quaternion.Euler(0, i * 360 / NodeProccesser.ChildNodes.Count + frame, 0)) * info.matrices[k];

                            cache = info.matrices[k].GetRow(0);
                            info.matrices[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                            cache = info.matrices[k].GetRow(1);
                            info.matrices[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                            cache = info.matrices[k].GetRow(2);
                            info.matrices[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));
                        }

                        //Log.Message(ids[i] + " info :\n" + info);
                    }
                }
            }
            return;
        }

        public override Thing GetVerbCorrespondingThing(VerbTracker verbTracker, Thing result, ref Verb verbBeforeConvert, ref Verb verbAfterConvert)
        {
            Type type = verbTracker.directOwner.GetType();
            if(verbBeforeConvert != null && verbAfterConvert == null)
            {
                CompChildNodeProccesser proccess = NodeProccesser.ParentProccesser;
                ThingWithComps thing = proccess?.parent;
                if(thing != null)
                {
                    VerbTracker tracker = null;
                    Verb cache = verbBeforeConvert;
                    if (type.IsAssignableFrom(thing.GetType()))
                    {
                        tracker = (thing as IVerbOwner).VerbTracker;
                    }
                    else
                    {
                        foreach (ThingComp comp in thing.AllComps)
                        {
                            if (type.IsAssignableFrom(comp.GetType()))
                            {
                                tracker = (comp as IVerbOwner).VerbTracker;
                                break;
                            }
                        }
                    }
                    cache = tracker?.AllVerbs.Find(x => x.verbProps == cache.verbProps);
                    if (cache != null) result = proccess.GetVerbCorrespondingThing(tracker, ref cache, ref verbAfterConvert) ?? result;
                    else if (tracker == null) result = proccess.GetVerbCorrespondingThing(verbTracker, ref verbBeforeConvert, ref verbAfterConvert) ?? result;
                }
                if (verbAfterConvert == null) verbAfterConvert = verbBeforeConvert;
            }
            else if(verbAfterConvert != null && verbBeforeConvert == null)
            {
                foreach (Thing t in NodeProccesser.ChildNodes)
                {
                    ThingWithComps thing = t as ThingWithComps;
                    CompChildNodeProccesser proccess = thing;
                    VerbTracker tracker = null;
                    Verb cache = verbAfterConvert;
                    if (type.IsAssignableFrom(t.GetType()))
                    {
                        tracker = (t as IVerbOwner).VerbTracker;
                    }
                    else if(thing != null)
                    {
                        foreach (ThingComp comp in thing.AllComps)
                        {
                            if (type.IsAssignableFrom(comp.GetType()))
                            {
                                tracker = (comp as IVerbOwner).VerbTracker;
                                break;
                            }
                        }
                    }
                    cache = tracker?.AllVerbs.Find(x => x.verbProps == cache.verbProps);
                    if(proccess != null)
                    {
                        if(cache != null) result = proccess.GetVerbCorrespondingThing(tracker, ref verbBeforeConvert, ref cache) ?? result;
                        else if (tracker == null) result = proccess.GetVerbCorrespondingThing(verbTracker, ref verbBeforeConvert, ref verbAfterConvert) ?? result;
                    }
                    else if(cache != null)
                    {
                        verbBeforeConvert = cache;
                        result = t;
                    }
                    if (verbBeforeConvert != null && result != null) return result;
                }
                if (verbBeforeConvert == null) verbBeforeConvert = verbAfterConvert;
            }
            return result;
        }

        public override List<Verb> PostVerbTracker_AllVerbs(VerbTracker verbTracker, List<Verb> result)
        {
            if (Prefs.DevMode) Log.Message(" VerbTracker=" + verbTracker + "; result.Count=" + result.Count + ";\n");
            Type type = verbTracker.directOwner.GetType();
            foreach (Thing t in NodeProccesser.ChildNodes)
            {
                if(type.IsAssignableFrom(t.GetType()))
                {
                    result.AddRange((t as IVerbOwner).VerbTracker?.AllVerbs);
                }
                else
                {
                    ThingWithComps thingWithComps = t as ThingWithComps;
                    if(thingWithComps != null)
                    {
                        foreach(ThingComp comp in thingWithComps.AllComps)
                        {
                            if (type.IsAssignableFrom(comp.GetType()))
                            {
                                result.AddRange((comp as IVerbOwner).VerbTracker?.AllVerbs);
                                goto Jump;
                            }
                        }
                        result = ((CompChildNodeProccesser)thingWithComps)?.PostVerbTracker_AllVerbs(verbTracker, result) ?? result;
                        Jump:;
                    }
                }
            }
            return result;
        }

        public override void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            if(statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS)
            {
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if(eq != null)
                {
                    forPostRead.Add("test_verbs", new List<VerbProperties>(parent.def.Verbs));
                    forPostRead.Add("test_tools", new List<Tool>(parent.def.tools));
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    parent.def.Verbs.Clear();
                    parent.def.tools.Clear();
                    //if (Prefs.DevMode) Log.Message(" prefix before change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    foreach (Verb verb in verbs)
                    {
                        if(verb.tool != null)
                        {
                            parent.def.tools.Add(verb.tool);
                        }
                        else
                        {
                            parent.def.Verbs.Add(verb.verbProps);
                        }
                    }
                    //if (Prefs.DevMode) Log.Message(" prefix after change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                }
            }
        }

        public override float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            if (statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS)
            {
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    //if (Prefs.DevMode) Log.Message(" postfix before clear: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    parent.def.Verbs.Clear();
                    parent.def.tools.Clear();
                    //if (Prefs.DevMode) Log.Message(" postfix before change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
                    parent.def.Verbs.AddRange((List<VerbProperties>)forPostRead["test_verbs"]);
                    parent.def.tools.AddRange((List<Tool>)forPostRead["test_tools"]);
                    //if (Prefs.DevMode) Log.Message(" postfix after change: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                }
            }
            return result;
        }

        public override string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (statWorker is StatWorker_MeleeAverageDPS || statWorker is StatWorker_MeleeAverageArmorPenetration)
            {
                foreach (Thing thing in NodeProccesser.ChildNodes)
                {
                    stringBuilder.AppendLine(thing.Label + ":\n");
                    stringBuilder.AppendLine(statWorker.GetExplanationUnfinalized(StatRequest.For(thing), numberSense));
                }
            }
            return result + "\n" + stringBuilder.ToString();
        }

        private int frame = 0;
    }

    public class CompProperties_Test : CompProperties
    {
        public CompProperties_Test()
        {
            base.compClass = typeof(NodeComp_Test);
        }

        public List<ThingDef> thingDefs = new List<ThingDef>();
    }
}
