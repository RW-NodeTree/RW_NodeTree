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
        public override Mesh MeshAt(Rot4 rot)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = _THIS;
            if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
            {
                return _GRAPHIC?.MeshAt(rot);
            }
            base.drawSize = comp_ChildNodeProccesser.DrawSize(rot);
            return base.MeshAt(rot);
        }
        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
            {
                return _GRAPHIC?.MatAt(rot, thing);
            }
            return comp_ChildNodeProccesser.ChildCombinedTexture(rot, _GRAPHIC.MatAt(rot, thing).shader);
        }
        public override Material MatSingleFor(Thing thing)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
            {
                return _GRAPHIC?.MatSingleFor(thing);
            }
            return comp_ChildNodeProccesser.ChildCombinedTexture(Rot4.South, _GRAPHIC.MatSingleFor(thing).shader);
        }
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null) _GRAPHIC?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
        public override Material MatNorth
        {
            get
            {
                Comp_ChildNodeProccesser comp_ChildNodeProccesser = _THIS;
                if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
                {
                    return _GRAPHIC?.MatNorth;
                }
                base.drawSize = comp_ChildNodeProccesser.DrawSize(_THIS.Rotation);
                return comp_ChildNodeProccesser.ChildCombinedTexture(Rot4.North, _GRAPHIC.MatNorth.shader);
            }
        }
        public override Material MatEast
        {
            get
            {
                Comp_ChildNodeProccesser comp_ChildNodeProccesser = _THIS;
                if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
                {
                    return _GRAPHIC?.MatEast;
                }
                base.drawSize = comp_ChildNodeProccesser.DrawSize(_THIS.Rotation);
                return comp_ChildNodeProccesser.ChildCombinedTexture(Rot4.East, _GRAPHIC.MatEast.shader);
            }
        }
        public override Material MatSouth
        {
            get
            {
                Comp_ChildNodeProccesser comp_ChildNodeProccesser = _THIS;
                if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
                {
                    return _GRAPHIC?.MatSouth;
                }
                base.drawSize = comp_ChildNodeProccesser.DrawSize(_THIS.Rotation);
                return comp_ChildNodeProccesser.ChildCombinedTexture(Rot4.South, _GRAPHIC.MatSouth.shader);
            }
        }
        public override Material MatWest
        {
            get
            {
                Comp_ChildNodeProccesser comp_ChildNodeProccesser = _THIS;
                if (_DRAW_ORIGIN || comp_ChildNodeProccesser == null)
                {
                    return _GRAPHIC?.MatWest;
                }
                base.drawSize = comp_ChildNodeProccesser.DrawSize(_THIS.Rotation);
                return comp_ChildNodeProccesser.ChildCombinedTexture(Rot4.West, _GRAPHIC.MatWest.shader);
            }
        }

        internal Thing _THIS = null;
        internal Graphic _GRAPHIC = null;
        internal bool _DRAW_ORIGIN = false;
    }
}
