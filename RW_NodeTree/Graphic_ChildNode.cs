using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public class Graphic_ChildNode : Graphic
    {
        public Graphic_ChildNode(Comp_ChildNodeProccesser thing, Graphic org)
        {
            currentProccess = thing;
            subGraphic = org;
            //base.drawSize = _THING.DrawSize(_THING.parent.Rotation);
            //base.data = _GRAPHIC.data;
        }

        public override Material MatSingle
        {
            get
            {
                if (currentProccess == null) return subGraphic?.MatSingle;
                base.drawSize = currentProccess.DrawSize(currentProccess.parent.Rotation, subGraphic);
                return currentProccess.ChildCombinedTexture(currentProccess.parent.Rotation, subGraphic);
            }
        }

        public override Material MatNorth
        {
            get
            {
                if (currentProccess == null) return subGraphic?.MatNorth;
                base.drawSize = currentProccess.DrawSize(Rot4.North, subGraphic);
                return currentProccess.ChildCombinedTexture(Rot4.North, subGraphic);
            }
        }

        public override Material MatEast
        {
            get
            {
                if (currentProccess == null) return subGraphic?.MatEast;
                base.drawSize = currentProccess.DrawSize(Rot4.East, subGraphic);
                return currentProccess.ChildCombinedTexture(Rot4.East, subGraphic);
            }
        }

        public override Material MatSouth
        {
            get
            {
                if (currentProccess == null) return subGraphic?.MatSouth;
                base.drawSize = currentProccess.DrawSize(Rot4.South, subGraphic);
                return currentProccess.ChildCombinedTexture(Rot4.South, subGraphic);
            }
        }

        public override Material MatWest
        {
            get
            {
                if (currentProccess == null) return subGraphic.MatWest;
                base.drawSize = currentProccess.DrawSize(Rot4.West, subGraphic);
                return currentProccess.ChildCombinedTexture(Rot4.West, subGraphic);
            }
        }

        public override bool WestFlipped
        {
            get
            {
                if (subGraphic != null && (currentProccess == null)) return subGraphic.WestFlipped;
                return base.WestFlipped;
            }
        }

        public override bool UseSameGraphicForGhost
        {
            get
            {
                if (subGraphic != null && (currentProccess == null)) return subGraphic.UseSameGraphicForGhost;
                return base.UseSameGraphicForGhost;
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                if (subGraphic != null && (currentProccess == null)) return subGraphic.ShouldDrawRotated;
                return base.ShouldDrawRotated;
            }
        }

        public override bool EastFlipped
        {
            get
            {
                if (subGraphic != null && (currentProccess == null)) return subGraphic.EastFlipped;
                return base.EastFlipped;
            }
        }

        public override float DrawRotatedExtraAngleOffset
        {
            get
            {
                if (subGraphic != null && (currentProccess == null)) return subGraphic.DrawRotatedExtraAngleOffset;
                return base.DrawRotatedExtraAngleOffset;
            }
        }

        public override Mesh MeshAt(Rot4 rot)
        {
            if (currentProccess == null) return subGraphic?.MeshAt(rot);
            base.drawSize = currentProccess.DrawSize(rot, subGraphic);
            if (Prefs.DevMode) Log.Message(" DrawSize: currentProccess=" + currentProccess + "; Rot4=" + rot + "; size=" + base.drawSize + ";\n");
            return base.MeshAt(rot);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (comp_ChildNodeProccesser == null) return subGraphic?.MatAt(rot, thing);
            base.drawSize = comp_ChildNodeProccesser.DrawSize(rot, subGraphic);
            return comp_ChildNodeProccesser.ChildCombinedTexture(rot, subGraphic);
        }

        public override Material MatSingleFor(Thing thing)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (comp_ChildNodeProccesser == null) return subGraphic?.MatSingleFor(thing);
            base.drawSize = comp_ChildNodeProccesser.DrawSize(thing.Rotation, subGraphic);
            return comp_ChildNodeProccesser.ChildCombinedTexture(thing.Rotation, subGraphic);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (comp_ChildNodeProccesser == null) subGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (comp_ChildNodeProccesser == null) subGraphic?.Print(layer, thing, extraRotation);
            else
            {
                base.drawSize = comp_ChildNodeProccesser.DrawSize(thing.Rotation, subGraphic);
                base.Print(layer, thing, extraRotation);
            }
        }

        private Comp_ChildNodeProccesser currentProccess = null;
        private Graphic subGraphic = null;
    }
}
