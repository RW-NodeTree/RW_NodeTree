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
        public abstract void DrawTexture(ref RenderTexture renderTexture);
        public abstract Vector3 IconTexturePostionOffset(Rot4 rot, int index);
        public abstract void UpdateNode(Comp_ChildNodeProccesser actionNode);
        public abstract bool AllowNode(Comp_ChildNodeProccesser node, int index = -1);


    }
}
