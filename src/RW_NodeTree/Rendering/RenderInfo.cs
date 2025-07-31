﻿using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RW_NodeTree.Rendering
{
    /// <summary>
    /// The parms of Graphics.DrawMesh(), but no camera
    /// </summary>
    public struct RenderInfo
    {
        /// <summary>
        /// the parms mesh in DrawMesh or DrawMeshInstanced
        /// </summary>
        public Mesh mesh;
        /// <summary>
        /// the parm submeshIndex in DrawMesh or DrawMeshInstanced
        /// </summary>
        public int submeshIndex;
        /// <summary>
        /// the parm matrices in DrawMeshInstanced or the parm matrix in DrawMesh
        /// </summary>
        public Matrix4x4[] matrices;
        /// <summary>
        /// the parms material in DrawMesh or DrawMeshInstanced
        /// </summary>
        public Material material;
        /// <summary>
        /// the parms properties in DrawMesh or DrawMeshInstanced
        /// </summary>
        public MaterialPropertyBlock? properties;
        /// <summary>
        /// the parms castShadows in DrawMesh or DrawMeshInstanced
        /// </summary>
        public ShadowCastingMode castShadows;
        /// <summary>
        /// the parms receiveShadows in DrawMesh or DrawMeshInstanced
        /// </summary>
        public bool receiveShadows;
        /// <summary>
        /// the parms layer in DrawMesh or DrawMeshInstanced
        /// </summary>
        public int layer;
        /// <summary>
        /// the parms probeAnchor in DrawMesh or DrawMeshInstanced
        /// </summary>
        public Transform? probeAnchor;
        /// <summary>
        /// the parms lightProbeUsage in DrawMesh or DrawMeshInstanced
        /// </summary>
        public LightProbeUsage lightProbeUsage;
        /// <summary>
        /// the parms lightProbeProxyVolume in DrawMesh or DrawMeshInstanced
        /// </summary>
        public LightProbeProxyVolume? lightProbeProxyVolume;
        /// <summary>
        /// the parms count in DrawMesh or DrawMeshInstanced
        /// </summary>
        public int count;
        /// <summary>
        /// Used to select the drawing method between DrawMesh and DrawMeshInstanced.If is true,it will call DrawMeshInstanced, else will select DrawMesh
        /// </summary>
        public bool DrawMeshInstanced;
        /// <summary>
        /// disable fast mode
        /// recommand disable when rendering many same thing, when is true, It will rendered by unity camera.
        /// </summary>
        private bool disableFastMode;

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, int layer, bool DrawMeshInstanced = false)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            if (material == null) throw new ArgumentNullException(nameof(material));
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = [matrix];
            this.material = material;
            this.layer = layer;
            this.properties = null;
            this.castShadows = ShadowCastingMode.On;
            this.receiveShadows = true;
            this.probeAnchor = null;
            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.lightProbeProxyVolume = null;
            this.count = 1;
            this.DrawMeshInstanced = DrawMeshInstanced;
            this.disableFastMode = false;
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, int layer, MaterialPropertyBlock? properties, ShadowCastingMode castShadows, bool receiveShadows, Transform? probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume? lightProbeProxyVolume)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            if (material == null) throw new ArgumentNullException(nameof(material));
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = [matrix];
            this.material = material;
            this.layer = layer;
            this.properties = properties;
            this.castShadows = castShadows;
            this.receiveShadows = receiveShadows;
            this.probeAnchor = probeAnchor;
            this.lightProbeUsage = lightProbeUsage;
            this.lightProbeProxyVolume = lightProbeProxyVolume;
            this.count = 1;
            this.DrawMeshInstanced = false;
            this.disableFastMode = false;
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock? properties, ShadowCastingMode castShadows, bool receiveShadows, int layer, LightProbeUsage lightProbeUsage, LightProbeProxyVolume? lightProbeProxyVolume, bool DrawMeshInstanced = true)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            if (material == null) throw new ArgumentNullException(nameof(material));
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = (Matrix4x4[])matrices.Clone();
            this.material = material;
            this.properties = properties;
            this.castShadows = castShadows;
            this.layer = layer;
            this.receiveShadows = receiveShadows;
            this.probeAnchor = null;
            this.lightProbeUsage = lightProbeUsage;
            this.lightProbeProxyVolume = lightProbeProxyVolume;
            this.count = count;
            this.DrawMeshInstanced = DrawMeshInstanced;
            this.disableFastMode = false;
        }

        public bool CanUseFastDrawingMode
        {
            get
            {
                return !disableFastMode && properties == null && probeAnchor == null && lightProbeProxyVolume == null && castShadows == ShadowCastingMode.Off && lightProbeUsage == LightProbeUsage.Off && !receiveShadows;
            }
            set
            {
                if (!(disableFastMode = !value))
                {
                    properties = null;
                    probeAnchor = null;
                    lightProbeProxyVolume = null;
                    castShadows = ShadowCastingMode.Off;
                    lightProbeUsage = LightProbeUsage.Off;
                    receiveShadows = false;
                }
            }
        }

        public override string ToString()
        {
            string str = base.ToString() + " :\n[\n\tmesh : " + mesh + ",\n\tsubmeshIndex : " + submeshIndex + ",\n\tmatrices :\n\t";
            if (matrices != null && matrices.Length > 0)
            {
                str += "[\n\t\t0 :\n" + matrices[0] + "\n\t\tcount : " + matrices.Length + "\n\t],";
            }
            else
            {
                str += "null,";
            }
            return str +
                "\n\tmaterial : " + material +
                ",\n\tproperties : " + properties +
                ",\n\tcastShadows : " + castShadows +
                ",\n\tlayer : " + layer +
                ",\n\treceiveShadows : " + receiveShadows +
                ",\n\tprobeAnchor : " + probeAnchor +
                ",\n\tlightProbeUsage : " + lightProbeUsage +
                ",\n\tlightProbeProxyVolume : " + lightProbeProxyVolume +
                ",\n\tcount : " + count +
                ",\n\tDrawMeshInstanced : " + DrawMeshInstanced + "\n]\n";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        public void DrawInfo(Camera? camera)
        {
            if (probeAnchor != null || !DrawMeshInstanced)
            {
                for (int j = 0; j < matrices.Length && j < count; ++j)
                    Graphics.DrawMesh(mesh, matrices[j], material, layer, camera, submeshIndex, properties, castShadows, receiveShadows, probeAnchor, lightProbeUsage, lightProbeProxyVolume);
            }
            else
            {
                Graphics.DrawMeshInstanced(mesh, submeshIndex, material, matrices, count, properties, castShadows, receiveShadows, layer, camera, lightProbeUsage, lightProbeProxyVolume);
            }
        }

        public void DrawInfoFast(CommandBuffer buffer)
        {
            if (buffer != null && CanUseFastDrawingMode)
            {
                if (probeAnchor != null || !DrawMeshInstanced)
                {
                    for (int j = 0; j < matrices.Length && j < count; ++j)
                        buffer.DrawMesh(mesh, matrices[j], material, submeshIndex, -1, properties);
                }
                else
                {
                    buffer.DrawMeshInstanced(mesh, submeshIndex, material, -1, matrices, count, properties);
                }

            }
        }
    }
}
