using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public MaterialPropertyBlock properties;
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
        public Transform probeAnchor;
        /// <summary>
        /// the parms lightProbeUsage in DrawMesh or DrawMeshInstanced
        /// </summary>
        public LightProbeUsage lightProbeUsage;
        /// <summary>
        /// the parms lightProbeProxyVolume in DrawMesh or DrawMeshInstanced
        /// </summary>
        public LightProbeProxyVolume lightProbeProxyVolume;
        /// <summary>
        /// the parms count in DrawMesh or DrawMeshInstanced
        /// </summary>
        public int count;
        /// <summary>
        /// Used to select the drawing method between DrawMesh and DrawMeshInstanced.If is true,it will call DrawMeshInstanced, else will select DrawMesh
        /// </summary>
        public bool DrawMeshInstanced;

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, int layer, bool DrawMeshInstanced = false)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = new Matrix4x4[] { matrix };
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
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, int layer, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, Transform probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = new Matrix4x4[] { matrix };
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
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, int layer, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume, bool DrawMeshInstanced = true)
        {
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
        }

        public bool CanUseFastDrawingMode
        {
            get
            {
                return properties == null && probeAnchor == null && lightProbeProxyVolume == null && castShadows == ShadowCastingMode.Off && lightProbeUsage == LightProbeUsage.Off && !receiveShadows;
            }
            set
            {
                if (value)
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
            if(matrices != null && matrices.Length > 0)
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
        public void DrawInfo(Camera camera)
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

        public void DrawInfoFast(RenderTexture target, Matrix4x4 ProjectionMatrix, Color backgroundColor, bool clearDepth, bool clearColor)
        {
            if (target != null && CanUseFastDrawingMode)
            {
                RenderTexture backUp = RenderTexture.active;
                int passCount = material.passCount;

                RenderTexture.active = target;

                GL.PushMatrix();

                Camera camera = Camera.current;
                GL.LoadProjectionMatrix(ProjectionMatrix * (camera != null ? camera.cameraToWorldMatrix : Matrix4x4.identity));
                GL.Clear(clearDepth, clearColor, backgroundColor);

                //GL.LoadIdentity();
                for (int i = 0; i < matrices.Length && i < count; ++i)
                {
                    for (int j = 0; j < passCount; ++j)
                    {
                        material.SetPass(j);
                        Graphics.DrawMeshNow(mesh, matrices[i], submeshIndex);
                    }
                }

                GL.PopMatrix();
                RenderTexture.active = backUp;

            }
        }
    }
}
