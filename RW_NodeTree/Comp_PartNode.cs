using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public class Comp_ThingsNode : ThingComp, IThingHolder
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
                ((Node)GetDirectlyHeldThings())[index] = value;
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

        public RenderTexture CombinedIconTexture(Rot4 rot,Vector2Int TextureSize)
        {
            RenderTexture result = new RenderTexture(TextureSize.x, TextureSize.y, 0);
            Vector2 startPoint = Vector2.zero;
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.DrawTexture(ref result, ref startPoint);
            }
            for(int i = 0; i < node.Count;i++)
            {
                Vector2 pos = IconTexturePostion(rot, i) * TextureSize;
                Comp_ThingsNode nodeComp = node[i];
                if(nodeComp != null)
                {
                    RenderTexture childTex = nodeComp.CombinedIconTexture(rot, TextureSize);
                    RenderTexture boxedCache = result;
                    Vector2 endPoint = startPoint + boxedCache.texelSize;
                    endPoint.x = Math.Max(endPoint.x, childTex.width + (int)pos.x);
                    endPoint.y = Math.Max(endPoint.y, childTex.height + (int)pos.y);
                    startPoint.x = Math.Min(startPoint.x, pos.x);
                    startPoint.y = Math.Min(startPoint.y, pos.y);
                    result = new RenderTexture(
                        (int)(endPoint.x - startPoint.x),
                        (int)(endPoint.y - startPoint.y),
                        0);

                    Vector2 scale = boxedCache.texelSize / result.texelSize;
                    Vector2 offset = -(startPoint / result.texelSize);
                    UnityEngine.Graphics.Blit(boxedCache, result, scale, offset);
                    boxedCache.Release();

                    scale = childTex.texelSize / result.texelSize;
                    offset = (pos - startPoint) / result.texelSize;
                    UnityEngine.Graphics.Blit(childTex, result, scale, offset);
                    childTex.Release();
                }
            }
            return result;
        }

        public Vector2 IconTexturePostion(Rot4 rot,int index)
        {
            Vector2 result = Vector2.zero;
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
        public bool AllowNode(Comp_ThingsNode node, int index = -1)
        {
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                if (!comp.AllowNode(node, index)) return false;
            }
            return true;
        }

        public void UpdateNode(Comp_ThingsNode actionNode = null)
        {
            if (actionNode == null) actionNode = this;
            foreach (ThingComp_BasicNodeComp comp in AllNodeComp)
            {
                comp.UpdateNode(actionNode);
            }
            foreach(Thing node in this.node)
            {
                ((Comp_ThingsNode)node)?.UpdateNode(actionNode);
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            if(node == null)
            {
                node = new Node(this);
            }
            return node;
        }

        #region operator
        public static explicit operator ThingWithComps(Comp_ThingsNode node)
        {
            return node.parent;
        }

        public static explicit operator Node(Comp_ThingsNode node)
        {
            return (Node)(node.GetDirectlyHeldThings());
        }

        public static implicit operator Comp_ThingsNode(Thing thing)
        {
            return thing.TryGetComp<Comp_ThingsNode>();
        }

        public static implicit operator Comp_ThingsNode(Node node)
        {
            return node.Comp;
        }
        #endregion

        private Node node;
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
