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
        public Graphic_ChildNode(INodeProcesser proccesser, Action<RenderTexture>? PostFX = null)
        {
            if (proccesser == null) throw new ArgumentNullException(nameof(proccesser));
            if (proccesser is not Thing) throw new ArgumentException("Invalid processer type", nameof(proccesser));
            currentProcesser = proccesser;
            postFX = PostFX;
            //base.drawSize = _THING.DrawSize(_THING.parent.Rotation);
            //base.data = _GRAPHIC.data;
        }

        public override Material? MatSingle => MatAt(((Thing)currentProcesser).Rotation);

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
            thing = (Thing)currentProcesser;

            (bool needUpdate, Material? material, Texture2D? texture, RenderTexture? cachedRenderTarget) = defaultRenderingCache[rot];

            if (needUpdate || material == null || texture == null || cachedRenderTarget == null)
            {
                currentProcesser.ChildNodes.GetNodeRenderingResult(rot, ref cachedRenderTarget, ref texture, this, postFX);
                Shader shader = thing.DefaultGraphic.Shader;
                texture!.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = currentProcesser.TextureFilterMode;

                if (material == null)
                {
                    material = new Material(shader);
                }
                else if (shader != null)
                {
                    material.shader = shader;
                }
                material.mainTexture = texture;
                defaultRenderingCache[rot] = (false, material, texture, cachedRenderTarget);
            }

            drawSize = new Vector2(texture.width, texture.height) / currentProcesser.TextureSizeFactor;
            return material;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef? thingDef, Thing? thing, float extraRotation)
        {
            MatAt(rot);
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            MatAt(thing.Rotation);
            base.Print(layer, thing, extraRotation);
        }

        public void ForceUpdate(Rot4 rot)
        {
            (bool needUpdate, Material? material, Texture2D? texture, RenderTexture? cachedRenderTarget) = defaultRenderingCache[rot];
            defaultRenderingCache[rot] = (true, material, texture, cachedRenderTarget);
        }

        public void ForceUpdateAll()
        {
            ForceUpdate(Rot4.North);
            ForceUpdate(Rot4.East);
            ForceUpdate(Rot4.South);
            ForceUpdate(Rot4.West);
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
            public (bool, Material?, Texture2D?, RenderTexture?) this[Rot4 index]
            {
                get
                {
                    switch (index.AsByte)
                    {
                        case 0: return (needUpdateNorth, materialNorth, textureNorth, cachedRenderTargetNorth);
                        case 1: return (needUpdateEast, materialEast, textureEast, cachedRenderTargetEast);
                        case 2: return (needUpdateSouth, materialSouth, textureSouth, cachedRenderTargetSouth);
                        case 3: return (needUpdateWest, materialWest, textureWest, cachedRenderTargetWest);
                        default: return (needUpdateNorth, materialNorth, textureNorth, cachedRenderTargetNorth);
                    }
                }
                set
                {
                    switch (index.AsByte)
                    {
                        case 0:
                            needUpdateNorth = value.Item1;
                            materialNorth = value.Item2;
                            textureNorth = value.Item3;
                            cachedRenderTargetNorth = value.Item4;
                            break;
                        case 1:
                            needUpdateEast = value.Item1;
                            materialEast = value.Item2;
                            textureEast = value.Item3;
                            cachedRenderTargetEast = value.Item4;
                            break;
                        case 2:
                            needUpdateSouth = value.Item1;
                            materialSouth = value.Item2;
                            textureSouth = value.Item3;
                            cachedRenderTargetSouth = value.Item4;
                            break;
                        case 3:
                            needUpdateWest = value.Item1;
                            materialWest = value.Item2;
                            textureWest = value.Item3;
                            cachedRenderTargetWest = value.Item4;
                            break;
                        default:
                            needUpdateNorth = value.Item1;
                            materialNorth = value.Item2;
                            textureNorth = value.Item3;
                            cachedRenderTargetNorth = value.Item4;
                            break;
                    }

                }
            }

            public bool needUpdateNorth, needUpdateEast, needUpdateSouth, needUpdateWest;

            public Material? materialNorth, materialEast, materialSouth, materialWest;

            public Texture2D? textureNorth, textureEast, textureSouth, textureWest;

            public RenderTexture? cachedRenderTargetNorth, cachedRenderTargetEast, cachedRenderTargetSouth, cachedRenderTargetWest;
        }

        private readonly INodeProcesser currentProcesser;
        private readonly Action<RenderTexture>? postFX;
        private readonly OffScreenRenderingCache defaultRenderingCache = new OffScreenRenderingCache();
    }
}
