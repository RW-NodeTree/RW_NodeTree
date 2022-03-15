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


        public bool NeedUpdate
        {
            get => ChildNodes.NeedUpdate;
            set => ChildNodes.NeedUpdate = value;
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
        public IEnumerable<CompBasicNodeComp> AllNodeComp
        {
            get
            {
                foreach (ThingComp comp in parent.AllComps)
                {
                    CompBasicNodeComp c = comp as CompBasicNodeComp;
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
        /// <param name="verbOwner">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <returns>correct verb ownner</returns>
        public Thing GetVerbCorrespondingThing(Type ownerType, ref Verb verbBeforeConvert, ref Verb verbAfterConvert)
        {
            VerbProperties verbPropertiesBeforeConvert = verbBeforeConvert?.verbProps;
            Tool toolBeforeConvert = verbBeforeConvert?.tool;
            VerbProperties verbPropertiesAfterConvert = verbAfterConvert?.verbProps;
            Tool toolAfterConvert = verbAfterConvert?.tool;
            return GetVerbCorrespondingThing(ownerType, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert);
        }

        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbOwner">Verb container</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties of verbBeforeConvert</param>
        /// <param name="toolBeforeConvert">tool of verbBeforeConvert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>correct verb ownner</returns>
        public Thing GetVerbCorrespondingThing(Type ownerType, ref Tool toolBeforeConvert, ref VerbProperties verbPropertiesBeforeConvert, ref Tool toolAfterConvert, ref VerbProperties verbPropertiesAfterConvert)
        {
            Verb verbBeforeConvert = null;
            Verb verbAfterConvert = null;
            return GetVerbCorrespondingThing(ownerType, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbOwner">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties of verbBeforeConvert</param>
        /// <param name="toolBeforeConvert">tool of verbBeforeConvert</param>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>correct verb ownner</returns>
        public Thing GetVerbCorrespondingThing(Type ownerType, ref Verb verbBeforeConvert, ref Tool toolBeforeConvert, ref VerbProperties verbPropertiesBeforeConvert, ref Verb verbAfterConvert, ref Tool toolAfterConvert, ref VerbProperties verbPropertiesAfterConvert)
        {
            Thing result = null;
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                List<VerbProperties> toolBeforeConvertVerbsProperties = toolBeforeConvert?.VerbsProperties.ToList();
                List<VerbProperties> toolAfterConvertVerbsProperties = toolAfterConvert?.VerbsProperties.ToList();

                if (verbBeforeConvert != null)
                {
                    verbPropertiesBeforeConvert = verbBeforeConvert.verbProps;
                    toolBeforeConvert = verbBeforeConvert.tool;
                }
                else if (toolBeforeConvertVerbsProperties != null && (verbPropertiesBeforeConvert == null || !toolBeforeConvertVerbsProperties.Contains(verbPropertiesBeforeConvert)))
                {
                    verbPropertiesBeforeConvert = toolBeforeConvertVerbsProperties.FirstOrDefault();
                }

                if (verbAfterConvert != null)
                {
                    verbPropertiesAfterConvert = verbAfterConvert.verbProps;
                    toolAfterConvert = verbAfterConvert.tool;
                }
                else if (toolAfterConvertVerbsProperties != null && (verbPropertiesAfterConvert != null || !toolAfterConvertVerbsProperties.Contains(verbPropertiesAfterConvert)))
                {
                    verbPropertiesAfterConvert = toolAfterConvertVerbsProperties.FirstOrDefault();
                }

                if((verbPropertiesBeforeConvert != null && verbPropertiesAfterConvert != null)
                || (toolBeforeConvert != null && toolAfterConvert != null))
                {
                    return result;
                }

                Verb verbCache = null;
                Tool toolCache = null;
                VerbProperties verbPropertiesCache = null;
                if (verbAfterConvert != null || toolAfterConvert != null || verbPropertiesAfterConvert != null)
                {
                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        result = comp.GetBeforeConvertVerbCorrespondingThing(ownerType, result, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert) ?? result;
                        if (verbBeforeConvert != null)
                        {
                            verbPropertiesBeforeConvert = verbBeforeConvert.verbProps;
                            toolBeforeConvert = verbBeforeConvert.tool;
                        }
                        else if (toolBeforeConvertVerbsProperties != null && (verbPropertiesBeforeConvert == null || !toolBeforeConvertVerbsProperties.Contains(verbPropertiesBeforeConvert)))
                        {
                            verbPropertiesBeforeConvert = toolBeforeConvertVerbsProperties.FirstOrDefault();
                        }
                    }

                    IVerbOwner verbOwner = GetSameTypeVerbOwner(ownerType, result ?? parent);

                    verbCache = verbBeforeConvert;
                    toolCache = toolBeforeConvert;
                    verbPropertiesCache = verbPropertiesBeforeConvert;

                    if (verbCache != null) verbBeforeConvert = verbOwner?.VerbTracker?.AllVerbs.Find(x => x == verbCache);
                    else if (toolCache != null) verbBeforeConvert = verbOwner?.VerbTracker?.AllVerbs.Find(x => x.tool == toolCache);
                    else verbBeforeConvert = verbOwner?.VerbTracker?.AllVerbs.Find(x => x.verbProps == verbPropertiesCache);

                    if (verbBeforeConvert != null)
                    {
                        verbPropertiesBeforeConvert = verbBeforeConvert.verbProps;
                        toolBeforeConvert = verbBeforeConvert.tool;
                    }
                    if (result != null && result != parent)
                    {
                        verbCache = null;
                        toolCache = null;
                        verbPropertiesCache = null;
                        result = ((CompChildNodeProccesser)result)?.GetVerbCorrespondingThing(ownerType, ref verbCache, ref toolCache, ref verbPropertiesCache, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert) ?? result;
                        verbBeforeConvert = verbCache;
                        toolBeforeConvert = toolCache;
                        verbPropertiesBeforeConvert = verbPropertiesCache;
                    }
                }
                else if (verbBeforeConvert != null || toolBeforeConvert != null || verbPropertiesBeforeConvert != null)
                {
                    foreach (CompBasicNodeComp comp in AllNodeComp)
                    {
                        result = comp.GetAfterConvertVerbCorrespondingThing(ownerType, result, verbBeforeConvert, toolBeforeConvert, verbPropertiesBeforeConvert, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert) ?? result;
                        if (verbAfterConvert != null)
                        {
                            verbPropertiesAfterConvert = verbAfterConvert.verbProps;
                            toolAfterConvert = verbAfterConvert.tool;
                        }
                        else if (toolAfterConvertVerbsProperties != null && (verbPropertiesAfterConvert != null || !toolAfterConvertVerbsProperties.Contains(verbPropertiesAfterConvert)))
                        {
                            verbPropertiesAfterConvert = toolAfterConvertVerbsProperties.FirstOrDefault();
                        }
                    }

                    IVerbOwner verbOwner = GetSameTypeVerbOwner(ownerType, result ?? parent);

                    verbCache = verbAfterConvert;
                    toolCache = toolAfterConvert;
                    verbPropertiesCache = verbPropertiesAfterConvert;

                    if (verbCache != null) verbAfterConvert = verbOwner?.VerbTracker?.AllVerbs.Find(x => x == verbCache);
                    else if (toolCache != null) verbAfterConvert = verbOwner?.VerbTracker?.AllVerbs.Find(x => x.tool == toolCache);
                    else verbAfterConvert = verbOwner?.VerbTracker?.AllVerbs.Find(x => x.verbProps == verbPropertiesCache);

                    if (verbAfterConvert != null)
                    {
                        verbPropertiesAfterConvert = verbAfterConvert.verbProps;
                        toolAfterConvert = verbAfterConvert.tool;
                    }
                    if (result != null && result != parent)
                    {
                        verbCache = null;
                        toolCache = null;
                        verbPropertiesCache = null;
                        result = ((CompChildNodeProccesser)result)?.GetVerbCorrespondingThing(ownerType, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert, ref verbCache, ref toolCache, ref verbPropertiesCache) ?? result;
                        verbAfterConvert = verbCache;
                        toolAfterConvert = toolCache;
                        verbPropertiesAfterConvert = verbPropertiesCache;
                    }
                }
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
                    owner?.Remove(node);
                    Thing nodeBefore = child[id];
                    child[id] = node;
                    if (child[id] == node)
                    {
                        return true;
                    }
                    else
                    {
                        owner?.TryAdd(node);
                        child[id] = nodeBefore;
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
                    ids.Insert(0, null);
                }
                catch (Exception ex)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    Log.Error(ex.ToString());
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.AdapteDrawSteep(ref ids, ref nodes, ref RenderInfos);
            }

            List<RenderInfo> final = new List<RenderInfo>();
            foreach(List<RenderInfo> infos in RenderInfos)
            {
                final.AddRange(infos);
            }

            RenderTexture cachedRenderTarget = null;
            RenderingTools.RenderToTarget(final, ref cachedRenderTarget, ref textures[rot_int]);
            GameObject.Destroy(cachedRenderTarget);

            textures[rot_int].wrapMode = TextureWrapMode.Clamp;
            //textures[rot_int].filterMode = FilterMode.Point;

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
            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                if (!comp.AllowNode(node, id)) return false;
            }
            return true;
        }

        public bool UpdateNode(CompChildNodeProccesser actionNode = null)
        {
            return ChildNodes.UpdateNode(actionNode);
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

        public static IVerbOwner GetSameTypeVerbOwner(Type ownerType, Thing thing)
        {
            if(thing != null && ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType))
            {
                IVerbOwner verbOwner = null;
                ThingWithComps t = thing as ThingWithComps;
                if (ownerType.IsAssignableFrom(thing.GetType()))
                {
                    verbOwner = (thing as IVerbOwner);
                }
                else if (t != null)
                {
                    foreach (ThingComp comp in t.AllComps)
                    {
                        if (ownerType.IsAssignableFrom(comp.GetType()))
                        {
                            verbOwner = (comp as IVerbOwner);
                            break;
                        }
                    }
                }
                return verbOwner;
            }
            return null;
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
}
