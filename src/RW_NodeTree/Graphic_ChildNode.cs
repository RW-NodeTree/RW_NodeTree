using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public class Graphic_ChildNode : Graphic
    {
        public Graphic_ChildNode(Thing thing, Graphic org)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));
            if (org == null) throw new ArgumentNullException(nameof(org));
            if (thing is not INodeProccesser nodeProccesser) throw new ArgumentNullException(nameof(thing));
            else currentProccesser = nodeProccesser;
            subGraphic = org;
            //base.drawSize = _THING.DrawSize(_THING.parent.Rotation);
            //base.data = _GRAPHIC.data;
        }

        public Graphic SubGraphic => subGraphic;

        public override Material? MatSingle => MatAt(((Thing)currentProccesser).Rotation);

        public override Material? MatNorth => MatAt(Rot4.North);

        public override Material? MatEast => MatAt(Rot4.East);

        public override Material? MatSouth => MatAt(Rot4.South);

        public override Material? MatWest => MatAt(Rot4.West);

        public override bool WestFlipped => false;

        public override bool UseSameGraphicForGhost => false;

        public override bool ShouldDrawRotated => false;

        public override bool EastFlipped => false;

        public override float DrawRotatedExtraAngleOffset => 0f;

        public override Material MatAt(Rot4 rot, Thing? thing = null)
        {
            thing = thing ?? (Thing)currentProccesser;
            if ((thing as INodeProccesser) != currentProccesser) return SubGraphic.MatAt(rot, thing);


            (Material? material, Texture2D? texture, RenderTexture? cachedRenderTarget) = defaultRenderingCache[rot];

            ReadOnlyDictionary<string, ReadOnlyCollection<RenderInfo>> commands = currentProccesser.ChildNodes.GetNodeRenderingInfos(rot, out bool needUpdate, subGraphic);
            if (needUpdate || material == null || texture == null || cachedRenderTarget == null)
            {
                List<RenderInfo> final = new List<RenderInfo>();
                foreach (var infos in commands)
                {
                    if (!infos.Value.NullOrEmpty()) final.AddRange(infos.Value);
                }
                RenderingTools.RenderToTarget(final, ref cachedRenderTarget!, ref texture!, default(Vector2Int), currentProccesser.TextureSizeFactor, currentProccesser.ExceedanceFactor, currentProccesser.ExceedanceOffset, currentProccesser.HasPostFX(true) ? currentProccesser.PostFX : default);
                Shader shader = subGraphic.Shader;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = currentProccesser.TextureFilterMode;

                if (material == null)
                {
                    material = new Material(shader);
                }
                else if (shader != null)
                {
                    material.shader = shader;
                }
                material.mainTexture = texture;
                defaultRenderingCache[rot] = (material, texture, cachedRenderTarget);
            }

            Vector2 size = new Vector2(texture.width, texture.height) / currentProccesser.TextureSizeFactor;

            Graphic? graphic = thing.Graphic;
            //if (graphic.GetGraphic_ChildNode() == this)
            while (graphic != null && graphic != this)
            {
                graphic.drawSize = size;
                graphic = graphic.GetSubGraphic();
            }
            this.drawSize = size;

            return material;
        }

        public override Material? MatSingleFor(Thing? thing)
        {
            thing = thing ?? (Thing)currentProccesser;
            if ((thing as INodeProccesser) != currentProccesser) return SubGraphic.MatSingleFor(thing);
            return MatAt(thing.Rotation, thing);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef? thingDef, Thing? thing, float extraRotation)
        {
            thing = thing ?? (Thing)currentProccesser;
            if ((thing as INodeProccesser) != currentProccesser) SubGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else if (!RenderingTools.StartOrEndDrawCatchingBlock || currentProccesser.HasPostFX(false)) base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else
            {
                MatAt(rot, thing);
                List<RenderInfo> final = new List<RenderInfo>();
                foreach (var infos in currentProccesser.ChildNodes.GetNodeRenderingInfos(rot, out _, subGraphic))
                {
                    if (!infos.Value.NullOrEmpty()) final.AddRange(infos.Value);
                }
                Matrix4x4 matrix = Matrix4x4.TRS(loc, Quaternion.AngleAxis(extraRotation, Vector3.up), Vector3.one);
                for (int i = 0; i < final.Count; i++)
                {
                    RenderInfo info = final[i];
                    Matrix4x4[] matrices = new Matrix4x4[info.matrices.Length];
                    for (int j = 0; j < info.matrices.Length; j++)
                    {
                        matrices[j] = matrix * info.matrices[j];
                    }
                    info.matrices = matrices;
                    info.DrawInfo(null);
                }
            }
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            thing = thing ?? (Thing)currentProccesser;
            if ((thing as INodeProccesser) != currentProccesser) SubGraphic?.Print(layer, thing, extraRotation);
            else
            {
                MatAt(thing.Rotation, thing);
                base.Print(layer, thing, extraRotation);
            }
        }


        private class OffScreenRenderingCache
        {
            ~OffScreenRenderingCache()
            {
                if (materialNorth != null) GameObject.Destroy(materialNorth);
                if (materialEast != null) GameObject.Destroy(materialEast);
                if (materialSouth != null) GameObject.Destroy(materialSouth);
                if (materialWest != null) GameObject.Destroy(materialWest);
                if (textureNorth != null) GameObject.Destroy(textureNorth);
                if (textureEast != null) GameObject.Destroy(textureEast);
                if (textureSouth != null) GameObject.Destroy(textureSouth);
                if (textureWest != null) GameObject.Destroy(textureWest);
                if (cachedRenderTargetNorth != null) GameObject.Destroy(cachedRenderTargetNorth);
                if (cachedRenderTargetEast != null) GameObject.Destroy(cachedRenderTargetEast);
                if (cachedRenderTargetSouth != null) GameObject.Destroy(cachedRenderTargetSouth);
                if (cachedRenderTargetWest != null) GameObject.Destroy(cachedRenderTargetWest);
            }
            public (Material?, Texture2D?, RenderTexture?) this[Rot4 index]
            {
                get
                {
                    switch (index.AsByte)
                    {
                        case 0: return (materialNorth, textureNorth, cachedRenderTargetNorth);
                        case 1: return (materialEast, textureEast, cachedRenderTargetEast);
                        case 2: return (materialSouth, textureSouth, cachedRenderTargetSouth);
                        case 3: return (materialWest, textureWest, cachedRenderTargetWest);
                        default: return (materialNorth, textureNorth, cachedRenderTargetNorth);
                    }
                }
                set
                {
                    switch (index.AsByte)
                    {
                        case 0:
                            materialNorth = value.Item1;
                            textureNorth = value.Item2;
                            cachedRenderTargetNorth = value.Item3;
                            break;
                        case 1:
                            materialEast = value.Item1;
                            textureEast = value.Item2;
                            cachedRenderTargetEast = value.Item3;
                            break;
                        case 2:
                            materialSouth = value.Item1;
                            textureSouth = value.Item2;
                            cachedRenderTargetSouth = value.Item3;
                            break;
                        case 3:
                            materialWest = value.Item1;
                            textureWest = value.Item2;
                            cachedRenderTargetWest = value.Item3;
                            break;
                        default:
                            materialNorth = value.Item1;
                            textureNorth = value.Item2;
                            cachedRenderTargetNorth = value.Item3;
                            break;
                    }

                }
            }

            public Material? materialNorth, materialEast, materialSouth, materialWest;

            public Texture2D? textureNorth, textureEast, textureSouth, textureWest;

            public RenderTexture? cachedRenderTargetNorth, cachedRenderTargetEast, cachedRenderTargetSouth, cachedRenderTargetWest;
        }

        private readonly OffScreenRenderingCache defaultRenderingCache = new OffScreenRenderingCache();
        private INodeProccesser currentProccesser;
        private Graphic subGraphic;
    }
}
