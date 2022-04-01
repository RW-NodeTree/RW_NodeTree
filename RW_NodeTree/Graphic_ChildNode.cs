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
        public Graphic_ChildNode(CompChildNodeProccesser thing, Graphic org)
        {
            currentProccess = thing;
            subGraphic = org;
            //base.drawSize = _THING.DrawSize(_THING.parent.Rotation);
            //base.data = _GRAPHIC.data;
        }

        public Graphic SubGraphic => subGraphic;

        public override Material MatSingle
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatSingle;
                base.drawSize = currentProccess.DrawSize(currentProccess.parent.Rotation, this);
                return currentProccess.ChildCombinedTexture(currentProccess.parent.Rotation, this);
            }
        }

        public override Material MatNorth
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatNorth;
                base.drawSize = currentProccess.DrawSize(Rot4.North, this);
                return currentProccess.ChildCombinedTexture(Rot4.North, this);
            }
        }

        public override Material MatEast
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatEast;
                base.drawSize = currentProccess.DrawSize(Rot4.East, this);
                return currentProccess.ChildCombinedTexture(Rot4.East, this);
            }
        }

        public override Material MatSouth
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatSouth;
                base.drawSize = currentProccess.DrawSize(Rot4.South, this);
                return currentProccess.ChildCombinedTexture(Rot4.South, this);
            }
        }

        public override Material MatWest
        {
            get
            {
                if (currentProccess == null) return SubGraphic.MatWest;
                base.drawSize = currentProccess.DrawSize(Rot4.West, this);
                return currentProccess.ChildCombinedTexture(Rot4.West, this);
            }
        }

        public override bool WestFlipped
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.WestFlipped;
                return base.WestFlipped;
            }
        }

        public override bool UseSameGraphicForGhost
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.UseSameGraphicForGhost;
                return base.UseSameGraphicForGhost;
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.ShouldDrawRotated;
                return base.ShouldDrawRotated;
            }
        }

        public override bool EastFlipped
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.EastFlipped;
                return base.EastFlipped;
            }
        }

        public override float DrawRotatedExtraAngleOffset
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.DrawRotatedExtraAngleOffset;
                return base.DrawRotatedExtraAngleOffset;
            }
        }

        public override Mesh MeshAt(Rot4 rot)
        {
            if (currentProccess == null) return SubGraphic?.MeshAt(rot);
            base.drawSize = currentProccess.DrawSize(rot, this);
            //if (Prefs.DevMode) Log.Message(" DrawSize: currentProccess=" + currentProccess + "; Rot4=" + rot + "; size=" + base.drawSize + ";\n");
            return base.MeshAt(rot);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (thing == null) comp_ChildNodeProccesser = currentProccess;
            if (comp_ChildNodeProccesser == null) return SubGraphic?.MatAt(rot, thing);
            base.drawSize = comp_ChildNodeProccesser.DrawSize(rot, this);
            return comp_ChildNodeProccesser.ChildCombinedTexture(rot, this);
        }

        public override Material MatSingleFor(Thing thing)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (thing == null) comp_ChildNodeProccesser = currentProccess;
            if (comp_ChildNodeProccesser == null) return SubGraphic?.MatSingleFor(thing);
            base.drawSize = comp_ChildNodeProccesser.DrawSize(thing.Rotation, this);
            return comp_ChildNodeProccesser.ChildCombinedTexture(thing.Rotation, this);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (thing == null) comp_ChildNodeProccesser = currentProccess;
            if (comp_ChildNodeProccesser == null) SubGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (thing == null) comp_ChildNodeProccesser = currentProccess;
            if (comp_ChildNodeProccesser == null) SubGraphic?.Print(layer, thing, extraRotation);
            else
            {
                base.drawSize = comp_ChildNodeProccesser.DrawSize(thing.Rotation, this);
                base.Print(layer, thing, extraRotation);
            }
        }

        private CompChildNodeProccesser currentProccess = null;
        private Graphic subGraphic = null;
    }
}
