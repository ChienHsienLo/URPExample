using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace PostProcessVolumetric
{
    public class VolumetricLightFeature : ScriptableRendererFeature
    {
        VolumetricLightPass m_ScriptablePass;

        public VolumetricLightSettings settings;

        public override void Create()
        {
            m_ScriptablePass = new VolumetricLightPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_ScriptablePass.SetCameraColorTarget(renderer);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
