using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;
using RW_NodeTree.Rendering;

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public class Comp_ChildNodeProccesser : ThingComp, IThingHolder
    {

        public Comp_ChildNodeProccesser()
        {
            childNodes = new NodeContainer(this);
        }

        public NodeContainer ChildNodes => (NodeContainer)GetDirectlyHeldThings();

        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public Comp_ChildNodeProccesser ParentProccesser => (Comp_ChildNodeProccesser)this.ParentHolder;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public Thing RootNode
        {
            get
            {
                Comp_ChildNodeProccesser proccesser = this;
                Comp_ChildNodeProccesser next = ParentProccesser;
                while (next != null) 
                {
                    proccesser = next;
                    next = next.ParentProccesser;
                }

                return proccesser;
            }
        }

        /// <summary>
        /// find all comp for node
        /// </summary>
        public IEnumerable<ThingComp_BasicNodeComp> AllNodeComp
        {
            get
            {
                foreach (ThingComp_BasicNodeComp comp in parent.AllComps)
                {
                    if (comp != null)
                    {
                        yield return comp;
                    }
                }
                yield break;
            }
        }

        public override bool AllowStackWith(Thing other)
        {
            return false;
        }

        public override void CompTick()
        {
            base.CompTick();
            UpdateNode();
            ChildNodes.ThingOwnerTick();
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look<NodeContainer>(ref this.childNodes, "innerContainer", this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AppendChild(Thing node, string id = null)
        {
            NodeContainer child = ChildNodes;
            if (child != null)
            {
                ThingOwner owner = node.holdingOwner;
                if(owner != null)
                {
                    owner.Remove(node);
                }
                if (child.TryAdd(node))
                {
                    if (id != null) child[node] = id;
                    return true;
                }
                else
                {
                    if (owner != null)
                    {
                        owner.TryAdd(node);
                    }
                }
            }
            return false;
        }

        public bool InsertNode(Thing node, int index, string id = null)
        {
            NodeContainer child = ChildNodes;
            if (child != null)
            {
                ThingOwner owner = node.holdingOwner;
                if (owner != null)
                {
                    owner.Remove(node);
                }
                if (child.Insert(index, node))
                {
                    if (id != null) child[node] = id;
                    return true;
                }
                else
                {
                    if (owner != null)
                    {
                        owner.TryAdd(node);
                    }
                }
            }
            return false;
        }

        public Material ChildCombinedTexture(Rot4 rot, Shader shader)
        {
            List<Thing> nodes = new List<Thing>(childNodes.InnerListForReading);
            List<string> ids = new List<string>(childNodes.InnerIdListForReading);
            List<List<RenderInfo>> RenderInfos = new List<List<RenderInfo>>(nodes.Count);

            foreach (string id in ids)
            {
                RenderingTools.BlockingState = true;
                Thing child = childNodes[id];
                if(child != null)
                {
                    child.Rotation = rot;
                    Vector3 pos = child.DrawPos;
                    child.Draw();
                    List<RenderInfo> forAdd = new List<RenderInfo>(RenderingTools.RenderInfos);
                    for (int i = 0; i < forAdd.Count; i++)
                    {
                        RenderInfo info = forAdd[i];
                        if(info.mesh != null)
                        {
                            for (int j = 0; j < info.matrices.Length; ++j)
                            {
                                info.matrices[j].m03 -= pos.x;
                                info.matrices[j].m13 -= pos.y;
                                info.matrices[j].m23 -= pos.z;
                            }
                            //info.matrix *= matrix;
                            forAdd[i] = info;
                        }
                    }
                    RenderInfos.Add(forAdd);
                }
            }
            RenderingTools.BlockingState = false;

            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.AdapteDrawSteep(ids, nodes, RenderInfos);
            }

            List<RenderInfo> final = new List<RenderInfo>();
            foreach(List<RenderInfo> infos in RenderInfos)
            {
                final.AddRange(infos);
            }

            switch (rot.AsInt)
            {
                case 0:
                    {
                        RenderTexture cache = texture_North;
                        texture_North = RenderingTools.RenderToTarget(final, texture_North);
                        if (material_North == null)
                        {
                            if (shader == null) shader = ShaderDatabase.Cutout;
                            material_North = new Material(shader);
                            material_North.mainTexture = texture_North;
                        }
                        else if (cache != texture_North)
                        {
                            material_North.mainTexture = texture_North;
                        }
                        return material_North;
                    }
                case 1:
                    {
                        RenderTexture cache = texture_East;
                        texture_East = RenderingTools.RenderToTarget(final, texture_East);
                        if (material_East == null)
                        {
                            if (shader == null) shader = ShaderDatabase.Cutout;
                            material_East = new Material(shader);
                            material_East.mainTexture = texture_East;
                        }
                        else if (cache != texture_East)
                        {
                            material_East.mainTexture = texture_East;
                        }
                        return material_East;
                    }
                case 2:
                    {
                        RenderTexture cache = texture_South;
                        texture_South = RenderingTools.RenderToTarget(final, texture_South);
                        if (material_South == null)
                        {
                            if (shader == null) shader = ShaderDatabase.Cutout;
                            material_South = new Material(shader);
                            material_South.mainTexture = texture_South;
                        }
                        else if (cache != texture_South)
                        {
                            material_South.mainTexture = texture_South;
                        }
                        return material_South;
                    }
                case 3:
                    {
                        RenderTexture cache = texture_West;
                        texture_West = RenderingTools.RenderToTarget(final, texture_West);
                        if (material_West == null)
                        {
                            if (shader == null) shader = ShaderDatabase.Cutout;
                            material_West = new Material(shader);
                            material_West.mainTexture = texture_West;
                        }
                        else if (cache != texture_West)
                        {
                            material_West.mainTexture = texture_West;
                        }
                        return material_West;
                    }
                default:
                    return BaseContent.BadMat;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowNode(Thing node, string id = null)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                if (!comp.AllowNode(node, id)) return false;
            }
            return true;
        }

        public void UpdateNode(Comp_ChildNodeProccesser actionNode = null)
        {
            if (ChildNodes.NeedUpdate)
            {
                if (actionNode == null) actionNode = this;
                foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
                {
                    comp.UpdateNode(actionNode);
                }
                foreach (Thing node in this.childNodes)
                {
                    ((Comp_ChildNodeProccesser)node)?.UpdateNode(actionNode);
                }
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            if(childNodes == null)
            {
                childNodes = new NodeContainer(this);
            }
            return childNodes;
        }

        #region operator
        public static implicit operator ThingWithComps(Comp_ChildNodeProccesser node)
        {
            return node.parent;
        }

        public static implicit operator Comp_ChildNodeProccesser(Thing thing)
        {
            return thing.TryGetComp<Comp_ChildNodeProccesser>();
        }
        #endregion

        private NodeContainer childNodes;

        private RenderTexture texture_North = null;

        private Material material_North = null;

        private RenderTexture texture_East = null;

        private Material material_East = null;

        private RenderTexture texture_South = null;

        private Material material_South = null;

        private RenderTexture texture_West = null;

        private Material material_West = null;

        /*
        private static Matrix4x4 matrix =
                            new Matrix4x4(
                                new Vector4(     1,      0,      0,      0      ),
                                new Vector4(     0,      0,     -0.001f, 0      ),
                                new Vector4(     0,      1,      0,      0      ),
                                new Vector4(     0,      0,      0.5f,   1      )
                            );
        */

    }
    /*
    public class CompProperties_PartNode : CompProperties
    {

        /// <summary>
        /// graphic data used when draw on parent node
        /// </summary>
        public GraphicData partIconGraphic = null;

        /// <summary>
        /// offset(+-) value for armor penetration
        /// </summary>
        public float armorPenetrationOffset = 0;

        /// <summary>
        /// offset(+-) value for melee weapon cooldown time
        /// </summary>
        public float meleeCooldownTimeOffset = 0;

        /// <summary>
        /// offset(+-) value for melee weapon damage
        /// </summary>
        public float meleeDamageOffset = 0;

        /// <summary>
        /// Multiplier value for ticks between burst shots,faster when value smaller
        /// </summary>
        public float ticksBetweenBurstShotsMultiplier = 1f;

        /// <summary>
        /// Multiplier value for muzzle flash scale,brighter when value biger
        /// </summary>
        public float muzzleFlashScaleMultiplier = 1f;

        /// <summary>
        /// Multiplier value for shooting rang,farther when value biger
        /// </summary>
        public float RangMultiplier = 1f;

        /// <summary>
        /// offset(+-) value for burst shot count
        /// </summary>
        public float burstShotCountOffset = 0;

        /// <summary>
        /// Multiplier value for shooting warmup time(the time need to take before whepon shoting),longer when value biger
        /// </summary>
        public float warmupTimeMultiplier = 1f;

        /// <summary>
        /// if is true,weapon can conly shoot one round when magazine capacity is 0
        /// </summary>
        public bool forceBrushShotWithOutMagCE = false;

        /// <summary>
        /// it set to not be affected by other part of parent node
        /// </summary>
        public bool verbPropertiesAffectByOtherPart = true;

        /// <summary>
        /// projectile replacement
        /// </summary>
        public ThingDef forcedProjectile = null;

        /// <summary>
        /// shooting sound replacement
        /// </summary>
        public SoundDef forceSound = null;

        /// <summary>
        /// Cast tail sound replacement
        /// </summary>
        public SoundDef forceSoundCastTail = null;


        public List<StatModifier> statMultiplier = new List<StatModifier>();


        public List<StatModifier> statOffset = new List<StatModifier>();
    }
    */
}
