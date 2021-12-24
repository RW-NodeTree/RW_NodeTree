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
    public class Comp_ChildNodeProccesser : ThingComp, IThingHolder
    {
        /// <summary>
        /// get currect node of index
        /// </summary>
        /// <param name="index">node index</param>
        /// <returns>currect node</returns>
        public Thing this[int index]
        {
            get
            {
                return GetDirectlyHeldThings()[index];
            }
            set
            {
                ThingOwner<Thing> things = (ThingOwner<Thing>)GetDirectlyHeldThings();
                if(this.AllowNode(value,index))
                {
                    if(things.TryAdd(value,false))
                    {
                        Thing droped;
                        if(things.TryDrop(this[index],ThingPlaceMode.Near,out droped))
                        {
                            List<Thing> innerList = innerListRef(things);
                            if (innerList != null)
                            {
                                innerList.RemoveAt(innerList.Count - 1);
                                innerList.Insert(index, value);
                            }
                        }
                        else
                        {
                            things.TryDrop(value, ThingPlaceMode.Near, out droped);
                        }
                    }
                }
            }
        }

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

        public RenderTexture CombinedIconTexture(Rot4 rot)
        {
            int TextureSize = 0;
            RenderTexture cache = RenderTexture.active;
            List<RenderInfo> RenderInfos = new List<RenderInfo>();
            for (int i = 0; i < childNodes.Count;i++)
            {
                RenderingTools.BlockingState = true;
                Vector3 pos = IconTexturePostion(rot, i);
                Thing child = childNodes[i];
                if(child != null)
                {
                    pos -= child.DrawPos;
                    child.Draw();
                    List<RenderInfo> forAdd = new List<RenderInfo>(RenderingTools.RenderInfos);
                    for (int j = 0; j < forAdd.Count; j++)
                    {
                        RenderInfo info = forAdd[j];
                        if(info.mesh != null)
                        {
                            info.matrix.m03 += pos.x;
                            info.matrix.m13 += pos.y;
                            info.matrix.m23 += pos.z;
                            //info.matrix *= matrix;
                            forAdd[j] = info;

                            Bounds bounds = info.mesh.bounds;

                            Vector3 max = bounds.max;
                            max = info.matrix * new Vector4(max.x, max.y, max.z, 1);

                            Vector3 min = bounds.min;
                            min = info.matrix * new Vector4(min.x, min.y, min.z, 1);

                            TextureSize = (int)Math.Max(TextureSize, Math.Abs(max.x) * 256);
                            TextureSize = (int)Math.Max(TextureSize, Math.Abs(max.y) * 256);
                            TextureSize = (int)Math.Max(TextureSize, Math.Abs(min.x) * 256);
                            TextureSize = (int)Math.Max(TextureSize, Math.Abs(min.y) * 256);
                        }
                    }
                    RenderInfos.AddRange(forAdd);
                }
            }
            RenderingTools.BlockingState = false;
            if (Surface == null || Surface.texelSize.x != TextureSize || Surface.texelSize.y != TextureSize)
            {
                if(Surface != null) GameObject.Destroy(Surface);
                Surface = new RenderTexture(TextureSize, TextureSize, 16);
            }

            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.DrawTexture(ref Surface);
            }

            RenderingTools.RenderToTarget(RenderInfos, Surface, TextureSize / 256);

            return Surface;
        }

        public Vector3 IconTexturePostion(Rot4 rot,int index)
        {
            Vector3 result = Vector3.zero;
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                result += comp.IconTexturePostionOffset(rot, index);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool AllowNode(Thing node, int index = -1)
        {
            if(index < 0)
            {
                index = this.childNodes.Count;
            }
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                if (!comp.AllowNode(node, index)) return false;
            }
            return true;
        }

        public void UpdateNode(Comp_ChildNodeProccesser actionNode = null)
        {
            if (actionNode == null) actionNode = this;
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.UpdateNode(actionNode);
            }
            foreach(Thing node in this.childNodes)
            {
                ((Comp_ChildNodeProccesser)node)?.UpdateNode(actionNode);
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
                childNodes = new ThingOwner<Thing>(this);
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

        private ThingOwner<Thing> childNodes;

        private RenderTexture Surface = null;

        private static AccessTools.FieldRef<ThingOwner<Thing>, List<Thing>> innerListRef = AccessTools.FieldRefAccess<ThingOwner<Thing>, List<Thing>>("innerList");

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
}
