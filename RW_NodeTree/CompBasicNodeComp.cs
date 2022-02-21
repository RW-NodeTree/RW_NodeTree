using RimWorld;
using RW_NodeTree.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public abstract partial class CompBasicNodeComp : ThingComp
    {
        public bool Validity => NodeProccesser != null;


        public bool NeedUpdate
        {
            get
            {
                NodeContainer ChildNodes = this.ChildNodes;
                return (ChildNodes != null) ? ChildNodes.NeedUpdate: false;
            }
            set
            {
                NodeContainer ChildNodes = this.ChildNodes;
                if (ChildNodes != null) ChildNodes.NeedUpdate = value;
            }
        }


        public CompChildNodeProccesser NodeProccesser => parent;
        /// <summary>
        /// node container
        /// </summary>
        public NodeContainer ChildNodes => NodeProccesser?.ChildNodes;


        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public CompChildNodeProccesser ParentProccesser => this.ParentHolder as CompChildNodeProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public Thing RootNode => NodeProccesser?.RootNode;


        /// <summary>
        /// find all comp for node
        /// </summary>
        public IEnumerable<CompBasicNodeComp> OtherNodeComp
        {
            get
            {
                foreach (ThingComp comp in parent.AllComps)
                {
                    CompBasicNodeComp c = comp as CompBasicNodeComp;
                    if (c != null && c != this)
                    {
                        yield return c;
                    }
                }
                yield break;
            }
        }
        public virtual Thing GetVerbCorrespondingThing(IVerbOwner verbOwner, Thing result, ref Verb verbBeforeConvert, ref Tool toolBeforeConvert, ref VerbProperties verbPropertiesBeforeConvert, ref Verb verbAfterConvert, ref Tool toolAfterConvert, ref VerbProperties verbPropertiesAfterConvert)
        {
            return result;
        }
        public virtual void UpdateNode(CompChildNodeProccesser actionNode)
        {
            return;
        }
        public virtual bool AllowNode(Thing node, string id = null)
        {
            return true;
        }
        public virtual void AdapteDrawSteep(ref List<string> ids, ref List<Thing> nodes, ref List<List<RenderInfo>> renderInfos)
        {
            return;
        }
    }
}
