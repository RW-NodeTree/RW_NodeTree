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
using RW_NodeTree.Tools;
using System.Diagnostics;
using System.Reflection;

namespace RW_NodeTree
{
    /// <summary>
    /// Node function proccesser
    /// </summary>
    public partial class CompChildNodeProccesser : ThingComp, IThingHolder
    {

        public CompChildNodeProccesser()
        {
            childNodes = new NodeContainer(this);
        }

        /// <summary>
        /// node container
        /// </summary>
        public NodeContainer ChildNodes => (NodeContainer)GetDirectlyHeldThings();


        /// <summary>
        /// get parent node if it is a node
        /// </summary>
        public CompChildNodeProccesser ParentProccesser => this.ParentHolder as CompChildNodeProccesser;

        /// <summary>
        /// root of this node tree
        /// </summary>
        public Thing RootNode
        {
            get
            {
                CompChildNodeProccesser proccesser = this;
                CompChildNodeProccesser next = ParentProccesser;
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
                foreach (ThingComp comp in parent.AllComps)
                {
                    ThingComp_BasicNodeComp c = comp as ThingComp_BasicNodeComp;
                    if (c != null)
                    {
                        yield return c;
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
            if(parent.def.tickerType == TickerType.Normal) UpdateNode();
            ChildNodes.ThingOwnerTick();
            IList<Thing> list = ChildNodes;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Thing t = list[i];
                if (t.def.tickerType == TickerType.Never)
                {
                    if((t is IVerbOwner) || (t as ThingWithComps)?.AllComps.Find(x => x is IVerbOwner) != null || (CompChildNodeProccesser)t != null)
                    {
                        t.Tick();
                        if (t.Destroyed)
                        {
                            list.Remove(t);
                        }
                    }
                }
            }
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                CompTickRare();
            }
        }

        public override void CompTickRare()
        {
            if (parent.def.tickerType == TickerType.Rare) UpdateNode();
            ChildNodes.ThingOwnerTickRare();
            if (Find.TickManager.TicksGame % 2000 < 250)
            {
                CompTickLong();
            }
        }

        public override void CompTickLong()
        {
            if (parent.def.tickerType == TickerType.Long) UpdateNode();
            ChildNodes.ThingOwnerTickLong();
        }

        #region Post
        #endregion

        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb
        /// </summary>
        /// <param name="verbTracker">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <returns>correct verb ownner</returns>
        public Thing GetVerbCorrespondingThing(VerbTracker verbTracker, ref Verb verbBeforeConvert, ref Verb verbAfterConvert)
        {
            Thing result = null;
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                result = comp.GetVerbCorrespondingThing(verbTracker, result, ref verbBeforeConvert,ref verbAfterConvert) ?? result;
            }
            return result;
        }

        /// <summary>
        /// set all texture need regenerate
        /// </summary>
        public void ResetRenderedTexture()
        {
            IsRandereds = 0;
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
            if(node != null)
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
            }
            return false;
        }

        /// <summary>
        /// Render all child things
        /// </summary>
        /// <param name="rot">rotate</param>
        /// <param name="subGraphic">orging Graphic of this</param>
        /// <returns>result of rendering</returns>
        public Material ChildCombinedTexture(Rot4 rot, Graphic subGraphic = null)
        {
            int rot_int = rot.AsInt;
            Shader shader = ShaderDatabase.Transparent;
            if (((IsRandereds >> rot_int) & 1) == 1 && materials[rot_int] != null)
            {
                return materials[rot_int]; 
            }
            List<Thing> nodes = new List<Thing>(childNodes.InnerListForReading);
            List<string> ids = new List<string>(childNodes.InnerIdListForReading);
            List<List<RenderInfo>> RenderInfos = new List<List<RenderInfo>>(nodes.Count);

            //if (Prefs.DevMode)
            //{
            //    StackTrace stack = new StackTrace();
            //    string stackReport = "";
            //    for(int i =0; i < 8; i++)
            //    {
            //        StackFrame sf = stack.GetFrame(i);
            //        MethodBase method = sf.GetMethod();
            //        stackReport += method.DeclaringType + " -> " + method + " " + sf + "\n";
            //    }
            //    Log.Message(parent + " graphic : " + parent.Graphic + ";\nstack : " + stackReport);
            //}

            foreach (Thing child in nodes)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    if (child != null)
                    {
                        Rot4 rotCache = child.Rotation;
                        child.Rotation = new Rot4((rot.AsInt + rotCache.AsInt) & 3);
                        child.DrawAt(Vector3.zero);
                        child.Rotation = rotCache;
                        RenderInfos.Add(RenderingTools.RenderInfos);
                    }
                }
                catch (Exception ex)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    Log.Error(ex.ToString());
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }

            //ORIGIN
            if (subGraphic == null) subGraphic = (parent.Graphic?.GetGraphic_ChildNode() as Graphic_ChildNode)?.SubGraphic;
            if (subGraphic != null)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    subGraphic.Draw(Vector3.zero, rot, parent);
                    RenderInfos.Insert(0, RenderingTools.RenderInfos);
                    nodes.Insert(0, this);
                    ids.Insert(0, "_THIS");
                }
                catch (Exception ex)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    Log.Error(ex.ToString());
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

            RenderingTools.RenderToTarget(final, ref cachedRenderTextures[rot_int], ref textures[rot_int]);


            if (materials[rot_int] == null)
            {
                materials[rot_int] = new Material(shader);
            }
            materials[rot_int].mainTexture = textures[rot_int];
            IsRandereds |= (byte)(1 << rot_int);
            return materials[rot_int];
        }


        public Vector2 DrawSize(Rot4 rot, Graphic subGraphic)
        {
            int rot_int = rot.AsInt;
            if (((IsRandereds >> rot_int) & 1) == 0 || textures[rot_int] == null) ChildCombinedTexture(rot, subGraphic);
            Vector2 result = new Vector2(textures[rot_int].width, textures[rot_int].height) / RenderingTools.TexSizeFactor;
            //if (Prefs.DevMode) Log.Message(" DrawSize: thing=" + parent + "; Rot4=" + rot + "; textureWidth=" + textures[rot_int].width + "; result=" + result + ";\n");
            return result;
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

        public void UpdateNode(CompChildNodeProccesser actionNode = null)
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
        public static implicit operator Thing(CompChildNodeProccesser node)
        {
            return node?.parent;
        }

        public static implicit operator CompChildNodeProccesser(Thing thing)
        {
            return thing?.TryGetComp<CompChildNodeProccesser>();
        }
        #endregion

        private NodeContainer childNodes;

        private Texture2D[] textures = new Texture2D[4];

        private RenderTexture[] cachedRenderTextures = new RenderTexture[4];

        private Material[] materials = new Material[4];

        private byte IsRandereds = 0;


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
