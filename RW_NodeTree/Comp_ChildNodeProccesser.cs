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
    public class Comp_ChildNodeProccesser : ThingComp, IThingHolder
    {

        public Comp_ChildNodeProccesser()
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

        /// <summary>
        /// event proccesser after StatWorker.GetValueUnfinalized()
        /// (WARRING!!!: Don't invoke any method if that will invoke StatWorker.GetValueUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetValueUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetValueUnfinalized()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.GetValueUnfinalized()</param>
        public void PostStatWorker_GetValueUnfinalized(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_GetValueUnfinalized(ref result, statWorker, req, applyPostProcess);
            }
        }

        /// <summary>
        /// event proccesser after StatWorker.FinalizeValue()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.FinalizeValue)
        /// </summary>
        /// <param name="result">result of StatWorker.FinalizeValue(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.FinalizeValue()</param>
        /// <param name="applyPostProcess">parm 'applyPostProcess' of StatWorker.FinalizeValue()</param>
        public void PostStatWorker_FinalizeValue(ref float result, StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_FinalizeValue(ref result, statWorker, req, applyPostProcess);
            }
        }

        /// <summary>
        /// event proccesser after StatWorker.GetExplanationUnfinalized()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationUnfinalized)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationUnfinalized(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationUnfinalized()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationUnfinalized()</param>
        public void PostStatWorker_GetExplanationUnfinalized(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_GetExplanationUnfinalized(ref result, statWorker, req, numberSense);
            }
            result = result ?? "";
        }

        /// <summary>
        /// event proccesser after StatWorker.GetExplanationFinalizePart()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetExplanationFinalizePart)
        /// </summary>
        /// <param name="result">result of StatWorker.GetExplanationFinalizePart(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="req">parm 'req' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="numberSense">parm 'numberSense' of StatWorker.GetExplanationFinalizePart()</param>
        /// <param name="finalVal">parm 'finalVal' of StatWorker.GetExplanationFinalizePart()</param>
        public void PostStatWorker_GetExplanationFinalizePart(ref string result, StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostStatWorker_GetExplanationFinalizePart(ref result, statWorker, req, numberSense, finalVal);
            }
            result = result ?? "";
        }

        /// <summary>
        /// event proccesser after StatWorker.GetInfoCardHyperlinks()
        /// (WARRING!!!: Don't invoke any method if thet will invoke StatWorker.GetInfoCardHyperlinks)
        /// </summary>
        /// <param name="result">result of StatWorker.GetInfoCardHyperlinks(), modifiable</param>
        /// <param name="statWorker">StatWorker</param>
        /// <param name="reqstatRequest">parm 'reqstatRequest' of StatWorker.GetInfoCardHyperlinks()</param>
        public void PostStatWorker_GetInfoCardHyperlinks(ref IEnumerable<Dialog_InfoCard.Hyperlink> result, StatWorker statWorker, StatRequest reqstatRequest)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                result = comp.PostStatWorker_GetInfoCardHyperlinks(result, statWorker, reqstatRequest);
            }
        }

        /// <summary>
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.VerbProperties)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.VerbProperties</param>
        public void PostIVerbOwner_GetVerbProperties(IVerbOwner owner, ref List<VerbProperties> verbProperties)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostIVerbOwner_GetVerbProperties(owner, ref verbProperties);
            }
            verbProperties = verbProperties ?? new List<VerbProperties>();
        }

        /// <summary>
        /// event proccesser after IVerbOwner.VerbProperties
        /// (WARRING!!!: Don't invoke any method if thet will invoke IVerbOwner.Tools)
        /// </summary>
        /// <param name="owner">IVerbOwner source</param>
        /// <param name="verbProperties">result of IVerbOwner.Tools</param>
        public void PostIVerbOwner_GetTools(IVerbOwner owner, ref List<Tool> tools)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.PostIVerbOwner_GetTools(owner, ref tools);
            }
            tools = tools ?? new List<Tool>();
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="verb"></param>
        /// <returns></returns>
        public Thing GetVerbCorrespondingThing(Verb verb)
        {
            Thing result = null;
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                result = comp.GetVerbCorrespondingThing(verb);
                if(result != null)
                {
                    break;
                }
            }
            return result;
        }

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

        public Graphic CreateGraphic_ChildNode(Graphic OrgGraphic, GraphicData data)
        {
            Graphic_Linked graphic_Linked = OrgGraphic as Graphic_Linked;
            if (graphic_Linked != null)
            {
                OrgGraphic = Graphic_Linked_SubGraphic(graphic_Linked);
            }
            Graphic_RandomRotated graphic_RandomRotated = OrgGraphic as Graphic_RandomRotated;
            if (graphic_RandomRotated != null)
            {
                OrgGraphic = Graphic_RandomRotated_SubGraphic(graphic_RandomRotated);
            }
            OrgGraphic = new Graphic_ChildNode(this, OrgGraphic);
            if (graphic_RandomRotated != null) OrgGraphic = new Graphic_RandomRotated(OrgGraphic, Graphic_RandomRotated_MaxAngle(graphic_RandomRotated));
            if (data.Linked) OrgGraphic = GraphicUtility.WrapLinked(OrgGraphic, data.linkType);
            return OrgGraphic;
        }

        /// <summary>
        /// Render all child things
        /// </summary>
        /// <param name="rot">rotate</param>
        /// <param name="subGraphic">orging Graphic of this</param>
        /// <returns>result of rendering</returns>
        public Material ChildCombinedTexture(Rot4 rot, Graphic subGraphic)
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
            RenderTexture render = RenderingTools.RenderToTarget(final);


            if (render != null)
            {
                Texture2D tex = textures[rot_int];
                if (tex == null || tex.width != render.width || tex.height != render.height)
                {
                    if(tex != null) GameObject.Destroy(tex);
                    tex = new Texture2D(render.width, render.width, TextureFormat.ARGB32, false);
                    textures[rot_int] = tex;
                }
                //else if (tex.width != render.width || tex.height != render.height)
                //{
                //    tex.Resize(render.width, render.height, TextureFormat.ARGB32, false);
                //}
                Graphics.CopyTexture(render, tex);
                //RenderTexture cache = RenderTexture.active;
                //RenderTexture.active = render;

                //tex.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
                //tex.Apply();

                //RenderTexture.active = cache;
                GameObject.Destroy(render);
            }
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
            Vector2 result = new Vector2(textures[rot_int].width, textures[rot_int].height) * 2 / RenderingTools.TexSizeFactor;
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
        public static implicit operator Thing(Comp_ChildNodeProccesser node)
        {
            return node?.parent;
        }

        public static implicit operator Comp_ChildNodeProccesser(Thing thing)
        {
            return thing?.TryGetComp<Comp_ChildNodeProccesser>();
        }
        #endregion

        private NodeContainer childNodes;

        private Texture2D[] textures = new Texture2D[4];

        private Material[] materials = new Material[4];

        private byte IsRandereds = 0;

        private static AccessTools.FieldRef<Graphic_Linked, Graphic> Graphic_Linked_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_Linked), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, Graphic> Graphic_RandomRotated_SubGraphic = AccessTools.FieldRefAccess<Graphic>(typeof(Graphic_RandomRotated), "subGraphic");
        private static AccessTools.FieldRef<Graphic_RandomRotated, float> Graphic_RandomRotated_MaxAngle = AccessTools.FieldRefAccess<float>(typeof(Graphic_RandomRotated), "maxAngle");


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
