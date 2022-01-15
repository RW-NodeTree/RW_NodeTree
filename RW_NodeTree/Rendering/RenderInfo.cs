using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RW_NodeTree.Rendering
{

    public struct RenderInfo
    {

        public Mesh mesh;
        public int submeshIndex;
        public Matrix4x4[] matrices;
        public Material material;
        public MaterialPropertyBlock properties;
        public ShadowCastingMode castShadows;
        public bool receiveShadows;
        public Transform probeAnchor;
        public LightProbeUsage lightProbeUsage;
        public LightProbeProxyVolume lightProbeProxyVolume;
        public int count;

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = new Matrix4x4[] { matrix };
            this.material = material;
            this.properties = null;
            this.castShadows = ShadowCastingMode.On;
            this.receiveShadows = true;
            this.probeAnchor = null;
            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.lightProbeProxyVolume = null;
            this.count = 1;
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Matrix4x4 matrix, Material material, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, Transform probeAnchor, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = new Matrix4x4[] { matrix };
            this.material = material;
            this.properties = properties;
            this.castShadows = castShadows;
            this.receiveShadows = receiveShadows;
            this.probeAnchor = probeAnchor;
            this.lightProbeUsage = lightProbeUsage;
            this.lightProbeProxyVolume = lightProbeProxyVolume;
            this.count = 1;
        }

        public RenderInfo(Mesh mesh, int submeshIndex, Material material, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties, ShadowCastingMode castShadows, bool receiveShadows, LightProbeUsage lightProbeUsage, LightProbeProxyVolume lightProbeProxyVolume)
        {
            this.mesh = mesh;
            this.submeshIndex = submeshIndex;
            this.matrices = matrices;
            this.material = material;
            this.properties = properties;
            this.castShadows = castShadows;
            this.receiveShadows = receiveShadows;
            this.probeAnchor = null;
            this.lightProbeUsage = lightProbeUsage;
            this.lightProbeProxyVolume = lightProbeProxyVolume;
            this.count = count;
        }
    }
}
