using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public abstract class ThingComp_BasicNodeComp : ThingComp
    {
        public abstract Vector2 IconTexturePostionOffset(Rot4 rot);
        public abstract void UpdateNode();
        public abstract bool AllowNode(Comp_ThingsNode node);


    }
}
