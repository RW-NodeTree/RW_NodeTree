﻿using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_NodeTree
{
    public class Graphic_ChildNode : Graphic
    {
        public Graphic_ChildNode(CompChildNodeProccesser thing, Graphic org)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));
            if (org == null) throw new ArgumentNullException(nameof(org));
            currentProccess = thing;
            subGraphic = org;
            //base.drawSize = _THING.DrawSize(_THING.parent.Rotation);
            //base.data = _GRAPHIC.data;
        }

        public Graphic SubGraphic => subGraphic;

        public override Material? MatSingle => MatAt(currentProccess.parent.Rotation);

        public override Material? MatNorth => MatAt(Rot4.North);

        public override Material? MatEast => MatAt(Rot4.East);

        public override Material? MatSouth => MatAt(Rot4.South);

        public override Material? MatWest => MatAt(Rot4.West);

        public override bool WestFlipped => false;

        public override bool UseSameGraphicForGhost => false;

        public override bool ShouldDrawRotated => false;

        public override bool EastFlipped => false;

        public override float DrawRotatedExtraAngleOffset => 0f;

        public override Mesh MeshAt(Rot4 rot)
        {
            MatAt(rot);
            //if (Prefs.DevMode) Log.Message(" DrawSize: currentProccess=" + currentProccess + "; Rot4=" + rot + "; size=" + base.drawSize + ";\n");
            return base.MeshAt(rot);
        }

        public override Material MatAt(Rot4 rot, Thing? thing = null)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser?)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) return SubGraphic.MatAt(rot, thing);


            (Material? material, Texture2D? texture, RenderTexture? cachedRenderTarget) = defaultRenderingCache[rot];

            List<(string?, Thing, List<RenderInfo>)> commands = comp_ChildNodeProccesser.GetNodeRenderingInfos(rot, out bool needUpdate, subGraphic);
            if (needUpdate || material == null || texture == null || cachedRenderTarget == null)
            {
                List<RenderInfo> final = new List<RenderInfo>();
                foreach ((string?, Thing, List<RenderInfo>) infos in commands)
                {
                    if (!infos.Item3.NullOrEmpty()) final.AddRange(infos.Item3);
                }
                RenderingTools.RenderToTarget(final, ref cachedRenderTarget!, ref texture!, default(Vector2Int), comp_ChildNodeProccesser.Props.TextureSizeFactor, comp_ChildNodeProccesser.Props.ExceedanceFactor, comp_ChildNodeProccesser.Props.ExceedanceOffset, comp_ChildNodeProccesser.HasPostFX(true) ? comp_ChildNodeProccesser.PostFX : default);
                Shader shader = subGraphic.Shader;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = comp_ChildNodeProccesser.Props.TextureFilterMode;

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



            Vector2 size = new Vector2(texture.width, texture.height) / comp_ChildNodeProccesser.Props.TextureSizeFactor;

            Graphic? graphic = comp_ChildNodeProccesser.parent.Graphic;
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
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser?)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) return SubGraphic.MatSingleFor(thing);
            return MatAt(comp_ChildNodeProccesser.parent.Rotation, comp_ChildNodeProccesser);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef? thingDef, Thing? thing, float extraRotation)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser?)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) SubGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else if (!RenderingTools.StartOrEndDrawCatchingBlock || comp_ChildNodeProccesser.HasPostFX(false)) base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else
            {
                MatAt(rot, thing);
                List<RenderInfo> final = new List<RenderInfo>();
                foreach ((string?, Thing, List<RenderInfo>) infos in currentProccess.GetNodeRenderingInfos(rot, out _, subGraphic))
                {
                    if (!infos.Item3.NullOrEmpty()) final.AddRange(infos.Item3);
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
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser?)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) SubGraphic?.Print(layer, thing, extraRotation);
            else
            {
                MatAt(comp_ChildNodeProccesser.parent.Rotation, comp_ChildNodeProccesser);
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
        private CompChildNodeProccesser currentProccess;
        private Graphic subGraphic;
    }
}
