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
    internal class NodeComp_Test : ThingComp_BasicNodeComp
    {

        public CompProperties_Test Props => (CompProperties_Test)props;

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (Validity)
            {
                Comp_ChildNodeProccesser proccesser = NodeProccesser;
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

                            info.matrices[k] = Matrix4x4.Rotate(Quaternion.Euler(0, i * 90 + frame, 0)) * info.matrices[k];
                        }

                        //Log.Message(ids[i] + " info :\n" + info);
                    }
                }
            }
            return;
        }

        public override bool AllowNode(Comp_ChildNodeProccesser node, string id = null)
        {
            return true;
        }

        public override void UpdateNode(Comp_ChildNodeProccesser actionNode)
        {
            return;
        }

        public override void PostStatWorker_GetValueUnfinalized(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            return;
        }

        public override void PostStatWorker_FinalizeValue(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            return;
        }

        public override void PostStatWorker_GetExplanationUnfinalized(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense)
        {
            return;
        }

        public override void PostStatWorker_GetExplanationFinalizePart(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return;
        }

        public override void PostIVerbOwner_GetVerbProperties(IVerbOwner owner, ref List<VerbProperties> verbProperties)
        {
            return;
        }

        public override void PostIVerbOwner_GetTools(IVerbOwner owner, ref List<Tool> tools)
        {
            return;
        }

        public override Thing GetVerbCorrespondingThing(Verb verb)
        {
            return null;
        }

        public override IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(IEnumerable<Dialog_InfoCard.Hyperlink> result, StatWorker statWorker, StatRequest reqstatRequest)
        {
            return result;
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
