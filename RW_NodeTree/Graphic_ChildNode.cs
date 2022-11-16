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
                if (currentProccess == null) return SubGraphic?.MatSingle ?? BaseContent.BadMat;
                return MatAt(currentProccess.parent.Rotation);
            }
        }

        public override Material MatNorth
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatNorth ?? BaseContent.BadMat;
                return MatAt(Rot4.North);
            }
        }

        public override Material MatEast
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatEast ?? BaseContent.BadMat;
                return MatAt(Rot4.East);
            }
        }

        public override Material MatSouth
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatSouth ?? BaseContent.BadMat;
                return MatAt(Rot4.South);
            }
        }

        public override Material MatWest
        {
            get
            {
                if (currentProccess == null) return SubGraphic.MatWest ?? BaseContent.BadMat;
                return MatAt(Rot4.West);
            }
        }

        public override bool WestFlipped
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.WestFlipped;
                return false;
            }
        }

        public override bool UseSameGraphicForGhost
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.UseSameGraphicForGhost;
                return false;
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.ShouldDrawRotated;
                return false;
            }
        }

        public override bool EastFlipped
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.EastFlipped;
                return false;
            }
        }

        public override float DrawRotatedExtraAngleOffset
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.DrawRotatedExtraAngleOffset;
                return 0;
            }
        }

        public override Mesh MeshAt(Rot4 rot)
        {
            if (currentProccess == null) return SubGraphic?.MeshAt(rot);
            UpdateDrawSize(currentProccess.GetAndUpdateDrawSize(rot, this));
            //if (Prefs.DevMode) Log.Message(" DrawSize: currentProccess=" + currentProccess + "; Rot4=" + rot + "; size=" + base.drawSize + ";\n");
            return base.MeshAt(rot);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) return SubGraphic?.MatAt(rot, thing);
            UpdateDrawSize(comp_ChildNodeProccesser.GetAndUpdateDrawSize(rot, this));
            return comp_ChildNodeProccesser.GetAndUpdateChildTexture(rot, this);
        }

        public override Material MatSingleFor(Thing thing)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) return SubGraphic?.MatSingleFor(thing);
            UpdateDrawSize(comp_ChildNodeProccesser.GetAndUpdateDrawSize(thing.Rotation, this));
            return comp_ChildNodeProccesser.GetAndUpdateChildTexture(thing.Rotation, this);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) SubGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else base.DrawWorker(loc, Rot4.North, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) SubGraphic?.Print(layer, thing, extraRotation);
            else
            {
                UpdateDrawSize(comp_ChildNodeProccesser.GetAndUpdateDrawSize(thing.Rotation, this));
                base.Print(layer, thing, extraRotation);
            }
        }

        /// <summary>
        /// update all draw size of the parent graphic of this graphic and itself
        /// </summary>
        /// <param name="size">size for update</param>
        private void UpdateDrawSize(Vector2 size)
        {
            Graphic graphic = currentProccess.parent.Graphic;
            //if (graphic.GetGraphic_ChildNode() == this)
            while (graphic != null && graphic != this)
            {
                graphic.drawSize = size;
                graphic = graphic.SubGraphic();
            }
            this.drawSize = size;
        }

        private CompChildNodeProccesser currentProccess = null;
        private Graphic subGraphic = null;
    }
}
