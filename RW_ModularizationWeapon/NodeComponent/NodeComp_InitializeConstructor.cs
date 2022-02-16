using RimWorld;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon.NodeComponent
{
    internal class NodeComp_InitializeConstructor : ThingComp_BasicNodeComp
    {

        public CompProperties_InitializeConstructor Props => (CompProperties_InitializeConstructor)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (Validity && !respawningAfterLoad)
            {
                CompChildNodeProccesser proccesser = NodeProccesser;
                foreach(ThingDef def in Props.thingDefs)
                {
                    Thing node = ThingMaker.MakeThing(def);
                    proccesser.AppendChild(node);
                }
            }
        }
    }

    public class CompProperties_InitializeConstructor : CompProperties
    {
        public CompProperties_InitializeConstructor()
        {
            base.compClass = typeof(NodeComp_InitializeConstructor);
        }

        public List<ThingDef> thingDefs = new List<ThingDef>();
    }
}
