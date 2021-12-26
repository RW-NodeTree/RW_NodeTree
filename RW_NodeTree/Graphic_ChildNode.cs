using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public class Graphic_ChildNode : Graphic_Single
    {
        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if(comp_ChildNodeProccesser == null)
            {
                return base.MatAt(rot, thing);
            }
            else
            {
                return comp_ChildNodeProccesser.ChildCombinedTexture(rot, Shader);
            }
        }

        public override Material MatSingleFor(Thing thing)
        {
            Comp_ChildNodeProccesser comp_ChildNodeProccesser = thing;
            if (comp_ChildNodeProccesser == null)
            {
                return base.MatSingleFor(thing);
            }
            else
            {
                return comp_ChildNodeProccesser.ChildCombinedTexture(thing.Rotation, Shader);
            }
        }
    }
}
