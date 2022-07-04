using RimWorld;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree.NodeComponent
{
    public class NodeComp_Test : CompBasicNodeComp
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
                    proccesser.AddOrSetChildById("Node_" + i,node);
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

        protected override void AdapteDrawSteep(ref List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos)
        {
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                if(nodeRenderingInfos[i].Item1 != parent)
                {
                    List<RenderInfo> infos = nodeRenderingInfos[i].Item3;
                    //infos.Add(new RenderInfo(thing.Graphic.MeshAt(Rot4.South), 0, Matrix4x4.identity, thing.Graphic.MatSingleFor(thing), 0));
                    for (int j = 0; j < infos.Count; j++)
                    {
                        RenderInfo info = infos[j];
                        for (int k = 0; k < info.matrices.Length; k++)
                        {
                            Vector4 cache = info.matrices[k].GetRow(0);
                            info.matrices[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, 0));

                            cache = info.matrices[k].GetRow(1);
                            info.matrices[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, i));

                            cache = info.matrices[k].GetRow(2);
                            info.matrices[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, 1));

                            info.matrices[k] = Matrix4x4.Rotate(Quaternion.Euler(0, i * 360 / ChildNodes.Count + frame * Props.roatedSpeed, 0)) * info.matrices[k];

                            cache = info.matrices[k].GetRow(0);
                            info.matrices[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                            cache = info.matrices[k].GetRow(1);
                            info.matrices[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                            cache = info.matrices[k].GetRow(2);
                            info.matrices[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));
                        }
                        info.CanUseFastDrawingMode = true;
                        infos[j] = info;
                        //Log.Message(ids[i] + " info :\n" + info);
                    }
                }
            }
            return;
        }

        protected override List<VerbPropertiesRegiestInfo> PostIVerbOwner_GetVerbProperties(Type ownerType, List<VerbPropertiesRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            NodeContainer conatiner = ChildNodes;
            for (int i = 0; i < conatiner.Count; i++)
            {
                List<VerbProperties> VerbPropertiesForAdd = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, conatiner[i])?.VerbProperties;
                if(VerbPropertiesForAdd != null)
                {
                    result.Capacity += VerbPropertiesForAdd.Count;
                    foreach (VerbProperties verbProperties in VerbPropertiesForAdd)
                    {
                        result.Add(new VerbPropertiesRegiestInfo(conatiner[(uint)i], verbProperties, verbProperties));
                    }
                }
            }
            return result;
        }

        protected override List<VerbToolRegiestInfo> PostIVerbOwner_GetTools(Type ownerType, List<VerbToolRegiestInfo> result, Dictionary<string, object> forPostRead)
        {
            NodeContainer conatiner = ChildNodes;
            for (int i = 0; i < conatiner.Count; i++)
            {
                List<Tool> ToolsForAdd = CompChildNodeProccesser.GetSameTypeVerbOwner(ownerType, conatiner[i])?.Tools;
                if (ToolsForAdd != null)
                {
                    result.Capacity += ToolsForAdd.Count;
                    foreach (Tool tool in ToolsForAdd)
                    {
                        result.Add(new VerbToolRegiestInfo(conatiner[(uint)i], tool, tool));
                    }
                }
            }
            return result;
        }

        protected override void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
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

        protected override float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
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

        protected override string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (statWorker is StatWorker_MeleeAverageDPS || statWorker is StatWorker_MeleeAverageArmorPenetration)
            {
                foreach (Thing thing in NodeProccesser.ChildNodes)
                {
                    stringBuilder.AppendLine("  " + thing.Label + ":");
                    string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(thing), numberSense);
                    exp = Regex.Replace(exp, "\n", "\n  ");
                    stringBuilder.AppendLine(exp);
                }
            }
            return result + "\n" + stringBuilder.ToString();
        }


        protected override void PostPreApplyDamageWithRef(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            int count = NodeProccesser.ChildNodes.Count + 1;
            dinfo.SetAmount(dinfo.Amount / count);
            foreach (Thing thing in NodeProccesser.ChildNodes)
            {
                thing.TakeDamage(dinfo);
            }
        }

        protected override IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest reqstatRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            foreach(Dialog_InfoCard.Hyperlink hyperlink in result)
            {
                yield return hyperlink;
            }
            foreach(Thing thing in NodeProccesser.ChildNodes)
            {
                yield return new Dialog_InfoCard.Hyperlink(thing);
            }
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

        public float roatedSpeed = 1;
    }
}
