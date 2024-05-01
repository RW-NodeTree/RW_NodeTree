using RW_NodeTree.Rendering;
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
            currentProccess = thing;
            subGraphic = org;
            //base.drawSize = _THING.DrawSize(_THING.parent.Rotation);
            //base.data = _GRAPHIC.data;
        }

        public Graphic SubGraphic => subGraphic;

        public override Material MatSingle
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatSingle ?? BaseContent.BadMat;
                return MatAt(currentProccess.parent.Rotation);
            }
        }

        public override Material MatNorth
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatNorth ?? BaseContent.BadMat;
                return MatAt(Rot4.North);
            }
        }

        public override Material MatEast
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatEast ?? BaseContent.BadMat;
                return MatAt(Rot4.East);
            }
        }

        public override Material MatSouth
        {
            get
            {
                if (currentProccess == null) return SubGraphic?.MatSouth ?? BaseContent.BadMat;
                return MatAt(Rot4.South);
            }
        }

        public override Material MatWest
        {
            get
            {
                if (currentProccess == null) return SubGraphic.MatWest ?? BaseContent.BadMat;
                return MatAt(Rot4.West);
            }
        }

        public override bool WestFlipped
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.WestFlipped;
                return false;
            }
        }

        public override bool UseSameGraphicForGhost
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.UseSameGraphicForGhost;
                return false;
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.ShouldDrawRotated;
                return false;
            }
        }

        public override bool EastFlipped
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.EastFlipped;
                return false;
            }
        }

        public override float DrawRotatedExtraAngleOffset
        {
            get
            {
                if (SubGraphic != null && (currentProccess == null)) return SubGraphic.DrawRotatedExtraAngleOffset;
                return 0;
            }
        }

        public override Mesh MeshAt(Rot4 rot)
        {
            if (currentProccess == null) return SubGraphic?.MeshAt(rot);
            MatAt(rot);
            //if (Prefs.DevMode) Log.Message(" DrawSize: currentProccess=" + currentProccess + "; Rot4=" + rot + "; size=" + base.drawSize + ";\n");
            return base.MeshAt(rot);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) return SubGraphic?.MatAt(rot, thing);


            (Material material, Texture2D texture, RenderTexture cachedRenderTarget) = defaultRenderingCache[rot];

            List<(string, Thing, List<RenderInfo>)> commands = comp_ChildNodeProccesser.GetNodeRenderingInfos(rot, out bool needUpdate, subGraphic);
            if (!needUpdate && material != null) goto ret;

            List<RenderInfo> final = new List<RenderInfo>();
            foreach ((string, Thing, List<RenderInfo>) infos in commands)
            {
                if (!infos.Item3.NullOrEmpty()) final.AddRange(infos.Item3);
            }
            RenderingTools.RenderToTarget(final, ref cachedRenderTarget, ref texture, default(Vector2Int), comp_ChildNodeProccesser.Props.TextureSizeFactor, comp_ChildNodeProccesser.Props.ExceedanceFactor, comp_ChildNodeProccesser.Props.ExceedanceOffset, comp_ChildNodeProccesser.HasPostFX(true) ? comp_ChildNodeProccesser.PostFX : default(Action<RenderTexture>));
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

            ret:;

            Vector2 size = new Vector2(texture.width, texture.height) / comp_ChildNodeProccesser.Props.TextureSizeFactor;

            Graphic graphic = comp_ChildNodeProccesser.parent.Graphic;
            //if (graphic.GetGraphic_ChildNode() == this)
            while (graphic != null && graphic != this)
            {
                graphic.drawSize = size;
                graphic = graphic.GetSubGraphic();
            }
            this.drawSize = size;

            return material;
        }

        public override Material MatSingleFor(Thing thing)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) return SubGraphic?.MatSingleFor(thing);
            return MatAt(thing.Rotation, thing);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) SubGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else if(!RenderingTools.StartOrEndDrawCatchingBlock || comp_ChildNodeProccesser.HasPostFX(false)) base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            else
            {
                MatAt(rot, thing);
                List<RenderInfo> final = new List<RenderInfo>();
                foreach ((string, Thing, List<RenderInfo>) infos in currentProccess.GetNodeRenderingInfos(rot, out _, subGraphic))
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
            CompChildNodeProccesser comp_ChildNodeProccesser = ((CompChildNodeProccesser)thing) ?? currentProccess;
            if (comp_ChildNodeProccesser != currentProccess) SubGraphic?.Print(layer, thing, extraRotation);
            else
            {
                MatAt(thing.Rotation);
                base.Print(layer, thing, extraRotation);
            }
        }


        private class OffScreenRenderingCache
        {
            ~OffScreenRenderingCache()
            {
                GameObject.Destroy(materialNorth);
                GameObject.Destroy(materialEast);
                GameObject.Destroy(materialSouth);
                GameObject.Destroy(materialWest);
                GameObject.Destroy(textureNorth);
                GameObject.Destroy(textureEast);
                GameObject.Destroy(textureSouth);
                GameObject.Destroy(textureWest);
                GameObject.Destroy(cachedRenderTargetNorth);
                GameObject.Destroy(cachedRenderTargetEast);
                GameObject.Destroy(cachedRenderTargetSouth);
                GameObject.Destroy(cachedRenderTargetWest);
            }
            public (Material, Texture2D, RenderTexture) this[Rot4 index]
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

            public Material materialNorth, materialEast, materialSouth, materialWest;

            public Texture2D textureNorth, textureEast, textureSouth, textureWest;

            public RenderTexture cachedRenderTargetNorth, cachedRenderTargetEast, cachedRenderTargetSouth, cachedRenderTargetWest;
        }

        private readonly OffScreenRenderingCache defaultRenderingCache = new OffScreenRenderingCache();
        private CompChildNodeProccesser currentProccess = null;
        private Graphic subGraphic = null;
    }
}
