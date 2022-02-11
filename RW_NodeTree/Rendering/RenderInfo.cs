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

        public Mesh mesh;
        public int submeshIndex;
        public Matrix4x4[] matrices;
        public Material material;
        public MaterialPropertyBlock properties;
        public ShadowCastingMode castShadows;
        public bool receiveShadows;
        public int layer;
        public Transform probeAnchor;
        public LightProbeUsage lightProbeUsage;
        public LightProbeProxyVolume lightProbeProxyVolume;
        public int count;
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
            this.matrices = matrices;
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
    }
}
