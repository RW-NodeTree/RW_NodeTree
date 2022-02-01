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
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                ChildNodes.ThingOwnerTickRare();
            }
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                ChildNodes.ThingOwnerTickLong();
            }
            for (int i = 0; i < IsRandereds.Length; i++) IsRandereds[i] = false;
        }

        public override void CompTickRare()
        {
            UpdateNode();
            ChildNodes.ThingOwnerTickRare();
            if (Find.TickManager.TicksGame % 2000 < 250)
            {
                ChildNodes.ThingOwnerTickLong();
            }
            for (int i = 0; i < IsRandereds.Length; i++) IsRandereds[i] = false;
        }

        public override void CompTickLong()
        {
            UpdateNode();
            ChildNodes.ThingOwnerTickLong();
            for (int i = 0; i < IsRandereds.Length; i++) IsRandereds[i] = false;
        }

        public void PostStatWorker_GetValueUnfinalized(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_GetValueUnfinalized(ref result, statWorker, req, applyPostProcess);
            }
        }

        public void PostStatWorker_FinalizeValue(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_FinalizeValue(ref result, statWorker, req, applyPostProcess);
            }
        }

        public void PostStatWorker_GetExplanationUnfinalized(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_GetExplanationUnfinalized(ref result, statWorker, req, numberSense);
            }
        }

        public void PostStatWorker_GetExplanationFinalizePart(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_GetExplanationFinalizePart(ref result, statWorker, req, numberSense, finalVal);
            }
        }

        public void PostIVerbOwner_GetVerbProperties(IVerbOwner owner, ref List<VerbProperties> verbProperties)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostIVerbOwner_GetVerbProperties(owner, ref verbProperties);
            }
        }

        public void PostIVerbOwner_GetTools(IVerbOwner owner, ref List<Tool> tools)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostIVerbOwner_GetTools(owner, ref tools);
            }
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

        public Material ChildCombinedTexture(Rot4 rot, Shader shader = null)
        {

            int rot_int = rot.AsInt;
            if (IsRandereds[rot_int] && materials[rot_int] != null && ((shader != null) ? materials[rot_int].shader = shader : true)) return materials[rot_int];
            List<Thing> nodes = new List<Thing>(childNodes.InnerListForReading);
            List<string> ids = new List<string>(childNodes.InnerIdListForReading);
            List<List<RenderInfo>> RenderInfos = new List<List<RenderInfo>>(nodes.Count);


            //ORIGIN
            Graphic_ChildNode graphic = parent.Graphic as Graphic_ChildNode;
            if(graphic != null)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    Vector3 pos = parent.DrawPos;
                    graphic._DRAW_ORIGIN = true;
                    parent.Draw();
                    graphic._DRAW_ORIGIN = false;
                    List<RenderInfo> forAdd = RenderingTools.RenderInfos;
                    for (int i = 0; i < forAdd.Count; i++)
                    {
                        RenderInfo info = forAdd[i];
                        if (info.mesh != null)
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
                    nodes.Insert(0, this);
                    ids.Insert(0, "_THIS");
                }
                catch (Exception ex)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    throw ex;
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }


            foreach (Thing child in nodes)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    if (child != null)
                    {
                        Rot4 rotCache = child.Rotation;
                        child.Rotation = new Rot4((rot.AsInt + rotCache.AsInt) & 3);
                        Vector3 pos = child.DrawPos;
                        child.Draw();
                        child.Rotation = rotCache;
                        List<RenderInfo> forAdd = RenderingTools.RenderInfos;
                        for (int i = 0; i < forAdd.Count; i++)
                        {
                            RenderInfo info = forAdd[i];
                            if (info.mesh != null)
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
                catch (Exception ex)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    throw ex;
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.AdapteDrawSteep(ref ids, ref nodes, ref RenderInfos);
            }

            List<RenderInfo> final = new List<RenderInfo>();
            foreach(List<RenderInfo> infos in RenderInfos)
            {
                final.AddRange(infos);
            }
            RenderTexture cache = textures[rot_int];
            textures[rot_int] = RenderingTools.RenderToTarget(final, textures[rot_int]);
            if (materials[rot_int] == null)
            {
                if (shader == null) shader = ShaderDatabase.Cutout;
                materials[rot_int] = new Material(shader);
                materials[rot_int].mainTexture = textures[rot_int];
            }
            else if (cache != textures[rot_int])
            {
                materials[rot_int].mainTexture = textures[rot_int];
            }
            IsRandereds[rot_int] = true;
            return materials[rot_int];
        }


        public Vector2 DrawSize(Rot4 rot)
        {
            int rot_int = rot.AsInt;
            if (!IsRandereds[rot_int] || textures[rot_int] == null) ChildCombinedTexture(rot);
            return textures[rot_int].texelSize / RenderingTools.TexSizeFactor;
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
            ChildNodes.UpdateNode(actionNode);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.ChildNodes);
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
            return node?.parent;
        }

        public static implicit operator Comp_ChildNodeProccesser(Thing thing)
        {
            return thing?.TryGetComp<Comp_ChildNodeProccesser>();
        }
        #endregion

        private NodeContainer childNodes;

        private RenderTexture[] textures = new RenderTexture[4];

        private Material[] materials = new Material[4];

        private bool[] IsRandereds = new bool[4];



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
