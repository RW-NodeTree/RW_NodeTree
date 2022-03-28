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

        public CompProperties_ChildNodeProccesser Props => (CompProperties_ChildNodeProccesser)props;


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
        public Thing GetBeforeConvertVerbCorrespondingThing(Type ownerType, Verb verbAfterConvert, out Verb verbBeforeConvert)
        {
            return GetBeforeConvertVerbCorrespondingThing(ownerType, verbAfterConvert, null, null, out verbBeforeConvert, out _, out _);
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
        public Thing GetBeforeConvertVerbCorrespondingThing(Type ownerType, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert, out Tool toolBeforeConvert, out VerbProperties verbPropertiesBeforeConvert)
        {
            return GetBeforeConvertVerbCorrespondingThing(ownerType, null, toolAfterConvert, verbPropertiesAfterConvert, out _, out toolBeforeConvert, out verbPropertiesBeforeConvert);
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
        public Thing GetBeforeConvertVerbCorrespondingThing(Type ownerType, Verb verbAfterConvert, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert, out Verb verbBeforeConvert, out Tool toolBeforeConvert, out VerbProperties verbPropertiesBeforeConvert)
        {
            verbBeforeConvert = null;
            toolBeforeConvert = null;
            verbPropertiesBeforeConvert = null;
            List<VerbProperties> verbsPropertiesCache = toolAfterConvert?.VerbsProperties.ToList();
            List<Verb> allVerbs = GetAllOriginalVerbs(GetSameTypeVerbOwner(ownerType, parent)?.VerbTracker);
            Thing result = parent;

            if (verbAfterConvert != null)
            {
                if (allVerbs != null && !allVerbs.Contains(verbAfterConvert))
                {
                    return null;
                }
                else
                {
                    toolAfterConvert = verbAfterConvert.tool;
                    verbPropertiesAfterConvert = verbAfterConvert.verbProps;
                }
            }
            else if (verbsPropertiesCache != null && (verbPropertiesAfterConvert == null || !verbsPropertiesCache.Contains(verbPropertiesAfterConvert)))
            {
                verbPropertiesAfterConvert = verbsPropertiesCache.FirstOrDefault();
            }

            if(allVerbs != null)
            {
                verbAfterConvert = allVerbs.Find(x => x.tool == toolAfterConvert && x.verbProps == verbPropertiesAfterConvert);
                if (verbAfterConvert == null)
                {
                    return null;
                }
            }

            verbBeforeConvert = verbAfterConvert;
            toolBeforeConvert = toolAfterConvert;
            verbPropertiesBeforeConvert = verbPropertiesAfterConvert;
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesAfterConvert != null)
            {
                Verb verbCache = null;
                Tool toolCache = null;
                VerbProperties verbPropertiesCache = null;
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    result = comp.GetBeforeConvertVerbCorrespondingThing(ownerType, result, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert, ref verbCache, ref toolCache, ref verbPropertiesCache) ?? result;
                }


                verbsPropertiesCache = toolCache?.VerbsProperties.ToList();
                allVerbs = GetAllOriginalVerbs(GetSameTypeVerbOwner(ownerType, result)?.VerbTracker);

                if (verbCache != null)
                {
                    if (allVerbs != null && !allVerbs.Contains(verbCache))
                    {
                        return result;
                    }
                    else
                    {
                        toolCache = verbCache.tool;
                        verbPropertiesCache = verbCache.verbProps;
                    }
                }
                else if (verbsPropertiesCache != null && (verbPropertiesCache == null || !verbsPropertiesCache.Contains(verbPropertiesCache)))
                {
                    verbPropertiesCache = verbsPropertiesCache.FirstOrDefault();
                }

                if (allVerbs != null)
                {
                    verbCache = allVerbs.Find(x => x.tool == toolCache && x.verbProps == verbPropertiesCache);
                    if (verbCache == null)
                    {
                        return result;
                    }
                }

                verbBeforeConvert = verbCache;
                toolBeforeConvert = toolCache;
                verbPropertiesBeforeConvert = verbPropertiesCache;

                if (result != null && result != parent)
                {
                    result = ((CompChildNodeProccesser)result)?.GetBeforeConvertVerbCorrespondingThing(ownerType, verbBeforeConvert, toolBeforeConvert, verbPropertiesBeforeConvert, out verbBeforeConvert, out toolBeforeConvert, out verbPropertiesBeforeConvert) ?? result;
                }
            }
            return result;
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb
        /// </summary>
        /// <param name="verbOwner">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <returns>correct verb ownner</returns>
        public Thing GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert, out Verb verbAfterConvert)
        {
            return GetAfterConvertVerbCorrespondingThing(ownerType, verbBeforeConvert, null, null, out verbAfterConvert, out _, out _);
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
        public Thing GetAfterConvertVerbCorrespondingThing(Type ownerType, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert, out Tool toolAfterConvert, out VerbProperties verbPropertiesAfterConvert)
        {
            return GetAfterConvertVerbCorrespondingThing(ownerType, null, toolBeforeConvert, verbPropertiesBeforeConvert, out _, out toolAfterConvert, out verbPropertiesAfterConvert);
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
        public Thing GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert, out Verb verbAfterConvert, out Tool toolAfterConvert, out VerbProperties verbPropertiesAfterConvert)
        {
            verbAfterConvert = null;
            toolAfterConvert = null;
            verbPropertiesAfterConvert = null;
            List<VerbProperties> verbsPropertiesCache = toolBeforeConvert?.VerbsProperties.ToList();
            List<Verb> allVerbs = GetAllOriginalVerbs(GetSameTypeVerbOwner(ownerType, parent)?.VerbTracker);
            Thing result = parent;

            if (verbBeforeConvert != null)
            {
                if (allVerbs != null && !allVerbs.Contains(verbBeforeConvert))
                {
                    return null;
                }
                else
                {
                    toolBeforeConvert = verbBeforeConvert.tool;
                    verbPropertiesBeforeConvert = verbBeforeConvert.verbProps;
                }
            }
            else if (verbsPropertiesCache != null && (verbPropertiesBeforeConvert == null || !verbsPropertiesCache.Contains(verbPropertiesBeforeConvert)))
            {
                verbPropertiesBeforeConvert = verbsPropertiesCache.FirstOrDefault();
            }

            if (allVerbs != null)
            {
                verbBeforeConvert = allVerbs?.Find(x => x.tool == toolBeforeConvert && x.verbProps == verbPropertiesBeforeConvert);
                if (verbBeforeConvert == null)
                {
                    return null;
                }
            }

            verbAfterConvert = verbBeforeConvert;
            toolAfterConvert = toolBeforeConvert;
            verbPropertiesAfterConvert = verbPropertiesBeforeConvert;
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesBeforeConvert != null)
            {
                Verb verbCache = null;
                Tool toolCache = null;
                VerbProperties verbPropertiesCache = null;
                foreach (CompBasicNodeComp comp in AllNodeComp)
                {
                    result = comp.GetAfterConvertVerbCorrespondingThing(ownerType, result, verbBeforeConvert, toolBeforeConvert, verbPropertiesBeforeConvert, ref verbCache, ref toolCache, ref verbPropertiesCache) ?? result;
                }


                verbsPropertiesCache = toolCache?.VerbsProperties.ToList();
                allVerbs = GetAllOriginalVerbs(GetSameTypeVerbOwner(ownerType, result)?.VerbTracker);

                if (verbCache != null)
                {
                    if (allVerbs != null && !allVerbs.Contains(verbCache))
                    {
                        return result;
                    }
                    else
                    {
                        toolCache = verbCache.tool;
                        verbPropertiesCache = verbCache.verbProps;
                    }
                }
                else if (verbsPropertiesCache != null && (verbPropertiesCache == null || !verbsPropertiesCache.Contains(verbPropertiesCache)))
                {
                    verbPropertiesCache = verbsPropertiesCache.FirstOrDefault();
                }

                if (allVerbs != null)
                {
                    verbCache = allVerbs.Find(x => x.tool == toolCache && x.verbProps == verbPropertiesCache);
                    if (verbCache == null)
                    {
                        return result;
                    }
                }

                verbAfterConvert = verbCache;
                toolAfterConvert = toolCache;
                verbPropertiesAfterConvert = verbPropertiesCache;

                if (result != null && result != parent)
                {
                    result = ((CompChildNodeProccesser)result)?.GetAfterConvertVerbCorrespondingThing(ownerType, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert, out verbAfterConvert, out toolAfterConvert, out verbPropertiesAfterConvert) ?? result;
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
            if (((IsRandereds >> rot_int) & 1) == 1 && materials[rot_int] != null)
            {
                return materials[rot_int]; 
            }
            List<NodeRenderingInfos> nodeRenderingInfos = new List<NodeRenderingInfos>(childNodes.Count + 1);

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


            //ORIGIN
            if (subGraphic == null) subGraphic = (parent.Graphic?.GetGraphic_ChildNode() as Graphic_ChildNode)?.SubGraphic;
            if (subGraphic != null)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    subGraphic.Draw(Vector3.zero, rot, parent);
                    nodeRenderingInfos.Add(new NodeRenderingInfos(this, null, RenderingTools.RenderInfos));
                }
                catch (Exception ex)
                {
                    RenderingTools.StartOrEndDrawCatchingBlock = false;
                    Log.Error(ex.ToString());
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }

            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                Thing child = container[i];
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    if (child != null)
                    {
                        Rot4 rotCache = child.Rotation;
                        child.Rotation = new Rot4((rot.AsInt + rotCache.AsInt) & 3);
                        child.DrawAt(Vector3.zero);
                        child.Rotation = rotCache;
                        nodeRenderingInfos.Add(new NodeRenderingInfos(child, container[(uint)i], RenderingTools.RenderInfos));
                    }
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
                comp.AdapteDrawSteep(ref nodeRenderingInfos);
            }

            List<RenderInfo> final = new List<RenderInfo>();
            foreach(NodeRenderingInfos infos in nodeRenderingInfos)
            {
                final.AddRange(infos.renderInfos);
            }

            RenderTexture cachedRenderTarget = null;
            RenderingTools.RenderToTarget(final, ref cachedRenderTarget, ref textures[rot_int], TextureSizeFactor: Props.TextureSizeFactor);
            GameObject.Destroy(cachedRenderTarget);


            Shader shader = subGraphic.Shader;

            textures[rot_int].wrapMode = TextureWrapMode.Clamp;
            textures[rot_int].filterMode = Props.TextureFilterMode;

            if (materials[rot_int] == null)
            {
                materials[rot_int] = new Material(shader);
            }
            else if(shader != null)
            {
                materials[rot_int].shader = shader;
            }
            materials[rot_int].mainTexture = textures[rot_int];
            IsRandereds |= (byte)(1 << rot_int);
            return materials[rot_int];
        }


        public Vector2 DrawSize(Rot4 rot, Graphic subGraphic)
        {
            int rot_int = rot.AsInt;
            if (((IsRandereds >> rot_int) & 1) == 0 || textures[rot_int] == null) ChildCombinedTexture(rot, subGraphic);
            Vector2 result = new Vector2(textures[rot_int].width, textures[rot_int].height) / Props.TextureSizeFactor;
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

    public class CompProperties_ChildNodeProccesser : CompProperties
    {
        public CompProperties_ChildNodeProccesser()
        {
            base.compClass = typeof(CompChildNodeProccesser);
        }

        public int TextureSizeFactor = (int)RenderingTools.DefaultTextureSizeFactor;
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
    }
}
