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

        /// <summary>
        /// as name, get or set needUpdate prop of this node
        /// </summary>
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

        /// <summary>
        /// parent node
        /// </summary>
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

        /// <summary>
        /// invoke by node proccesser,use to get verb corresponding thing before convert 
        /// </summary>
        /// <param name="ownerType">the type of verbowner</param>
        /// <param name="result">result of other node comp</param>
        /// <param name="verbAfterConvert">verb after convert</param>
        /// <param name="toolAfterConvert">tool after convert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties after convert</param>
        /// <param name="verbBeforeConvert">verb before convert</param>
        /// <param name="toolBeforeConvert">tool before convert</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties before convert</param>
        /// <returns>Corresponding thing before verb convert</returns>
        public virtual Thing GetBeforeConvertVerbCorrespondingThing(Type ownerType, Thing result, Verb verbAfterConvert, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert, ref Verb verbBeforeConvert, ref Tool toolBeforeConvert, ref VerbProperties verbPropertiesBeforeConvert)
        {
            return result;
        }


        /// <summary>
        /// invoke by node proccesser,use to get verb corresponding thing after convert 
        /// </summary>
        /// <param name="ownerType">the type of verbowner</param>
        /// <param name="result">result of other node comp</param>
        /// <param name="verbAfterConvert">verb after convert</param>
        /// <param name="toolAfterConvert">tool after convert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties after convert</param>
        /// <param name="verbBeforeConvert">verb before convert</param>
        /// <param name="toolBeforeConvert">tool before convert</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties before convert</param>
        /// <returns>Corresponding thing before verb convert</returns>
        public virtual Thing GetAfterConvertVerbCorrespondingThing(Type ownerType, Thing result, Verb verbBeforeConvert, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert, ref Verb verbAfterConvert, ref Tool toolAfterConvert, ref VerbProperties verbPropertiesAfterConvert)
        {
            return result;
        }

        /// <summary>
        /// update event
        /// </summary>
        /// <param name="actionNode">update event action node</param>
        /// <returns>stope bubble</returns>
        public virtual bool UpdateNode(CompChildNodeProccesser actionNode)
        {
            return false;
        }

        /// <summary>
        /// allow node to append into container
        /// </summary>
        /// <param name="node">node for add</param>
        /// <param name="id">id</param>
        /// <returns>able to add into container</returns>
        public virtual bool AllowNode(Thing node, string id = null)
        {
            return true;
        }

        /// <summary>
        /// Adapte draw steep of this node
        /// </summary>
        /// <param name="ids">Corresponding is</param>
        /// <param name="nodes">Corresponding node</param>
        /// <param name="renderInfos">Corresponding render infos</param>
        public virtual void AdapteDrawSteep(ref List<NodeRenderingInfos> nodeRenderingInfos)
        {
            return;
        }
    }
}
