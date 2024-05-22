using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PostProcessVolumetric
{
    [System.Serializable]
    public class VolumetricLightSettings
    {
        public Material occluderMaterial;
        public Material radialBlurMaterial;
        public Material depthMaterial;
        public bool useDepthTextureApproach = true;

        [Range(0.1f, 1f)]
        public float resolutionScale = 0.5f;

        [Range(0.0f, 1.0f)]
        public float intensity = 1.0f;

        [Range(0.0f, 1.0f)]
        public float blurWidth = 0.85f;

        public Color lightTint = Color.white;

        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        public List<string> shaderTags;

        public string profilerTag = "Volumetric";
    }
}
