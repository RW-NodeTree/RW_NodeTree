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
        /// 
        /// </summary>
        /// <param name="ownerType"></param>
        /// <param name="thing"></param>
        /// <param name="verb"></param>
        /// <param name="tool"></param>
        /// <param name="verbProperties"></param>
        /// <returns></returns>
        public static bool CheckVerbDatasVaildityAndAdapt(Type ownerType, Thing thing, ref Verb verb, ref Tool tool, ref VerbProperties verbProperties)
        {
            if (ownerType == null || !typeof(IVerbOwner).IsAssignableFrom(ownerType) || thing == null || (verb == null && tool == null && verbProperties == null)) return false;

            List<VerbProperties> verbsPropertiesCache = tool?.VerbsProperties.ToList();
            List<Verb> allVerbs = GetAllOriginalVerbs(GetSameTypeVerbOwner(ownerType, thing)?.VerbTracker);

            if (verb != null)
            {
                if (allVerbs != null && !allVerbs.Contains(verb))
                {
                    return false;
                }
                else
                {
                    tool = verb.tool;
                    verbProperties = verb.verbProps;
                }
            }
            else if (verbsPropertiesCache != null && (verbProperties == null || !verbsPropertiesCache.Contains(verbProperties)))
            {
                verbProperties = verbsPropertiesCache.FirstOrDefault();
            }

            if (allVerbs != null)
            {
                Tool toolCache = tool;
                VerbProperties verbPropertiesCache = verbProperties;
                verb = allVerbs.Find(x => x.tool == toolCache && x.verbProps == verbPropertiesCache);
                if (verb == null)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb
        /// </summary>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <returns>Verb infos before convert</returns>
        public (Thing, Verb, Tool, VerbProperties) GetBeforeConvertVerbCorrespondingThing(Type ownerType, Verb verbAfterConvert)
        {
            return GetBeforeConvertVerbCorrespondingThing(ownerType, verbAfterConvert, null, null);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>Verb infos before convert</returns>
        public (Thing, Verb, Tool, VerbProperties) GetBeforeConvertVerbCorrespondingThing(Type ownerType, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert)
        {
            return GetBeforeConvertVerbCorrespondingThing(ownerType, null, toolAfterConvert, verbPropertiesAfterConvert);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="verbAfterConvert">Verb after convert</param>
        /// <param name="verbPropertiesAfterConvert">verbProperties of verbAfterConvert</param>
        /// <param name="toolAfterConvert">tool of verbAfterConvert</param>
        /// <returns>Verb infos before convert</returns>
        public (Thing, Verb, Tool, VerbProperties) GetBeforeConvertVerbCorrespondingThing(Type ownerType, Verb verbAfterConvert, Tool toolAfterConvert, VerbProperties verbPropertiesAfterConvert)
        {
            (Thing, Verb, Tool, VerbProperties) result = default((Thing, Verb, Tool, VerbProperties));

            if (!CheckVerbDatasVaildityAndAdapt(ownerType, parent, ref verbAfterConvert, ref toolAfterConvert, ref verbPropertiesAfterConvert)) return result;

            result = (parent, verbAfterConvert, toolAfterConvert, verbPropertiesAfterConvert);
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesAfterConvert != null)
            {
                (Thing, Verb, Tool, VerbProperties) cache = result;
                if(toolAfterConvert != null)
                {
                    List<VerbToolRegiestInfo> Registed;
                    if (!regiestedNodeVerbToolInfos.TryGetValue(ownerType, out Registed))
                    {
                        Registed = new List<VerbToolRegiestInfo>();
                        regiestedNodeVerbToolInfos.Add(ownerType, Registed);
                    }
                    for (int i = 0; i < Registed.Count; i++)
                    {
                        VerbToolRegiestInfo regiestInfo = Registed[i];
                        if (regiestInfo.afterCobvertTool == toolAfterConvert)
                        {
                            cache = (ChildNodes[regiestInfo.id] ?? parent, null, regiestInfo.berforConvertTool, null);
                            break;
                        }
                    }
                }
                else
                {
                    List<VerbPropertiesRegiestInfo> Registed;
                    if (!regiestedNodeVerbPropertiesInfos.TryGetValue(ownerType, out Registed))
                    {
                        Registed = new List<VerbPropertiesRegiestInfo>();
                        regiestedNodeVerbPropertiesInfos.Add(ownerType, Registed);
                    }
                    for (int i = 0; i < Registed.Count; i++)
                    {
                        VerbPropertiesRegiestInfo regiestInfo = Registed[i];
                        if (regiestInfo.afterConvertProperties == verbPropertiesAfterConvert)
                        {
                            cache = (ChildNodes[regiestInfo.id] ?? parent, null, null, regiestInfo.berforConvertProperties);
                            break;
                        }
                    }
                }
                //if (Prefs.DevMode) Log.Message(cache.ToString());

                if (!CheckVerbDatasVaildityAndAdapt(ownerType, cache.Item1, ref cache.Item2, ref cache.Item3, ref cache.Item4)) return result;

                result = cache;

                if (result.Item1 != null && result.Item1 != parent && ((CompChildNodeProccesser)result.Item1) != null)
                {
                    Thing before = result.Item1;
                    result = ((CompChildNodeProccesser)result.Item1).GetBeforeConvertVerbCorrespondingThing(ownerType, result.Item2, result.Item3, result.Item4);
                    result.Item1 = result.Item1 ?? before;
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
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert)
        {
            return GetAfterConvertVerbCorrespondingThing(ownerType, verbBeforeConvert, null, null);
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
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert)
        {
            return GetAfterConvertVerbCorrespondingThing(ownerType, null, toolBeforeConvert, verbPropertiesBeforeConvert);
        }


        /// <summary>
        /// Return the correct verb ownner and complemented before&after verb info
        /// </summary>
        /// <param name="ownerType">Verb container</param>
        /// <param name="verbBeforeConvert">Verb before convert</param>
        /// <param name="verbPropertiesBeforeConvert">verbProperties of verbBeforeConvert</param>
        /// <param name="toolBeforeConvert">tool of verbBeforeConvert</param>
        /// <returns>correct verb ownner</returns>
        public (Thing, Verb, Tool, VerbProperties) GetAfterConvertVerbCorrespondingThing(Type ownerType, Verb verbBeforeConvert, Tool toolBeforeConvert, VerbProperties verbPropertiesBeforeConvert)
        {
            (Thing, Verb, Tool, VerbProperties) result = default((Thing, Verb, Tool, VerbProperties));

            if (!CheckVerbDatasVaildityAndAdapt(ownerType, parent, ref verbBeforeConvert, ref toolBeforeConvert, ref verbPropertiesBeforeConvert)) return result;

            result = (parent, verbBeforeConvert, toolBeforeConvert, verbPropertiesBeforeConvert);
            if (ownerType != null && typeof(IVerbOwner).IsAssignableFrom(ownerType) && verbPropertiesBeforeConvert != null)
            {
                (Thing, Verb, Tool, VerbProperties) cache = result;
                if(ParentProccesser != null)
                {
                    if (toolBeforeConvert != null)
                    {
                        List<VerbToolRegiestInfo> Registed;
                        if (!regiestedNodeVerbToolInfos.TryGetValue(ownerType, out Registed))
                        {
                            Registed = new List<VerbToolRegiestInfo>();
                            regiestedNodeVerbToolInfos.Add(ownerType, Registed);
                        }
                        for (int i = 0; i < Registed.Count; i++)
                        {
                            VerbToolRegiestInfo regiestInfo = Registed[i];
                            if (regiestInfo.berforConvertTool == toolBeforeConvert)
                            {
                                cache = (ParentProccesser, null, regiestInfo.afterCobvertTool, null);
                                break;
                            }
                        }
                    }
                    else
                    {
                        List<VerbPropertiesRegiestInfo> Registed;
                        if (!regiestedNodeVerbPropertiesInfos.TryGetValue(ownerType, out Registed))
                        {
                            Registed = new List<VerbPropertiesRegiestInfo>();
                            regiestedNodeVerbPropertiesInfos.Add(ownerType, Registed);
                        }
                        for (int i = 0; i < Registed.Count; i++)
                        {
                            VerbPropertiesRegiestInfo regiestInfo = Registed[i];
                            if (regiestInfo.berforConvertProperties == verbPropertiesBeforeConvert)
                            {
                                cache = (ParentProccesser, null, null, regiestInfo.afterConvertProperties);
                                break;
                            }
                        }
                    }
                }

                if (!CheckVerbDatasVaildityAndAdapt(ownerType, cache.Item1, ref cache.Item2, ref cache.Item3, ref cache.Item4)) return result;

                result = cache;

                if (result.Item1 != null && result.Item1 != parent && ((CompChildNodeProccesser)result.Item1) != null)
                {
                    Thing before = result.Item1;
                    result = ((CompChildNodeProccesser)result.Item1).GetAfterConvertVerbCorrespondingThing(ownerType, result.Item2, result.Item3, result.Item4);
                    result.Item1 = result.Item1 ?? before;
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

        /// <summary>
        /// Rimworld Defined method, used for load and save game saves.
        /// </summary>
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
            List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos = new List<(Thing, string, List<RenderInfo>)>(childNodes.Count + 1);

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
            subGraphic = ((subGraphic ?? parent.Graphic)?.GetGraphic_ChildNode() as Graphic_ChildNode)?.SubGraphic ?? subGraphic;
            if (subGraphic != null)
            {
                RenderingTools.StartOrEndDrawCatchingBlock = true;
                try
                {
                    subGraphic.Draw(Vector3.zero, rot, parent);
                    nodeRenderingInfos.Add((this, null, RenderingTools.RenderInfos));
                }
                catch (Exception ex)
                {
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
                        nodeRenderingInfos.Add((child, container[(uint)i], RenderingTools.RenderInfos));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                RenderingTools.StartOrEndDrawCatchingBlock = false;
            }

            foreach (CompBasicNodeComp comp in AllNodeComp)
            {
                comp.internal_AdapteDrawSteep(ref nodeRenderingInfos);
            }

            List<RenderInfo> final = new List<RenderInfo>();
            foreach((Thing, string, List<RenderInfo>) infos in nodeRenderingInfos)
            {
                final.AddRange(infos.Item3);
            }

            RenderingTools.RenderToTarget(final, ref cachedRenderTargets[rot_int], ref textures[rot_int], default(Vector2Int), Props.TextureSizeFactor, Props.ExceedanceFactor, Props.ExceedanceOffset);


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
                if (!comp.internal_AllowNode(node, id)) return false;
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

        private RenderTexture[] cachedRenderTargets = new RenderTexture[4];

        private Material[] materials = new Material[4];

        private byte IsRandereds = 0;
        //a a b b b
        public readonly Dictionary<Type, List<VerbToolRegiestInfo>> regiestedNodeVerbToolInfos = new Dictionary<Type, List<VerbToolRegiestInfo>>();

        public readonly Dictionary<Type, List<VerbPropertiesRegiestInfo>> regiestedNodeVerbPropertiesInfos = new Dictionary<Type, List<VerbPropertiesRegiestInfo>>();


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

        public float ExceedanceFactor = 1f;
        public float ExceedanceOffset = 1f;
        public int TextureSizeFactor = (int)RenderingTools.DefaultTextureSizeFactor;
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
    }
}
