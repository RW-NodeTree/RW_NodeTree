using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;
using RW_NodeTree.Patch;

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
                if(this.AllowNode(value))
                {
                    value = value.SplitOff(1);
                    if(things.TryAdd(value))
                    {
                        List<Thing> innerList = innerListRef(things);
                        if(innerList != null)
                        {
                            innerList.RemoveAt(innerList.Count - 1);
                            innerList.Insert(index, value);
                        }
                    }
                }
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
            List<List<RenderInfo>> RenderInfos = new List<List<RenderInfo>>();
            for (int i = 0; i < childNodes.Count;i++)
            {
                RenderingPatch.BlockingState = true;
                Vector3 pos = IconTexturePostion(rot, i);
                Thing child = childNodes[i];
                if(child != null)
                {
                    pos -= child.DrawPos;
                    child.Draw();
                    List<RenderInfo> forAdd = new List<RenderInfo>(RenderingPatch.RenderInfos);
                    RenderInfos.Add(forAdd);
                    for (int j = 0; j < forAdd.Count; j++)
                    {
                        RenderInfo info = forAdd[j];
                        if(info.mesh != null)
                        {
                            info.matrix.m03 += pos.x;
                            info.matrix.m13 += pos.y;
                            info.matrix.m23 += pos.z;
                            info.matrix *= matrix;
                            forAdd[j] = info;
                            Bounds bounds = info.mesh.bounds;
                            Vector3 max = info.matrix * bounds.max;
                            Vector3 min = info.matrix * bounds.min;
                            TextureSize = Math.Max(TextureSize >> 1, (int)Math.Abs(max.x) >> 8) << 1;
                            TextureSize = Math.Max(TextureSize >> 1, (int)Math.Abs(min.x) >> 8) << 1;
                            TextureSize = Math.Max(TextureSize >> 1, (int)Math.Abs(max.y) >> 8) << 1;
                            TextureSize = Math.Max(TextureSize >> 1, (int)Math.Abs(min.y) >> 8) << 1;
                        }
                    }
                }
            }
            RenderingPatch.BlockingState = false;
            if (Surface == null || Surface.texelSize.x != TextureSize || Surface.texelSize.y != TextureSize)
            {
                Surface = new RenderTexture(TextureSize, TextureSize, 24);
            }
            else
            {
                Graphics.Blit(SolidColorMaterials.SimpleSolidColorMaterial(default(Color)).mainTexture, Surface);
            }

            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.DrawTexture(ref Surface);
            }
            RenderTexture.active = Surface;
            foreach (List<RenderInfo> renderInfos in RenderInfos)
            {
                foreach (RenderInfo info in renderInfos)
                {
                    if (info.material != null && info.mesh != null)
                    {
                        for (int i = 0; i < info.material.passCount; i++)
                        {
                            info.material.SetPass(i);
                            Graphics.DrawMeshNow(info.mesh, info.matrix * Matrix4x4.Scale(Vector3.one * (1f / (TextureSize >> 8))), info.submeshIndex);
                        }
                    }
                }
            }
            RenderTexture.active = cache;
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

        private static AccessTools.FieldRef<Thing, IntVec3> positionIntRef = AccessTools.FieldRefAccess<Thing, IntVec3>("positionInt");

        private static Matrix4x4 matrix =
                            new Matrix4x4(
                                new Vector4(     1,      0,      0,      0      ),
                                new Vector4(     0,      0,      1,      0      ),
                                new Vector4(     0,     -0.001f, 0,      0.5f   ),
                                new Vector4(     0,      0,      0,      1      )
                            );
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
