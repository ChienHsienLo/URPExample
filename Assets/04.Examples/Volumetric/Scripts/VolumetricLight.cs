using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace PostProcessVolumetric
{
    public class VolumetricLightPass : ScriptableRenderPass
    {
        RTHandle occluderMapHandle;
        RTHandle depthMapHandle;

        Material occluderMaterial;
        Material radialBlurMaterial;
        Material depthMaterial;

        bool useDepthTextureApproach;
        float resolutionScale;
        float intensity;
        float blurWidth;
        Color tint;

        List<ShaderTagId> shaderTagIds = new List<ShaderTagId>() { new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly"), new ShaderTagId("LightweightForward"), new ShaderTagId("SRPDefaultUnlit") };

        string profilerTag;

        string occluderMapName = "_OccluderMap";
        string depthMapName = "_DepthMap";

        int centerID = Shader.PropertyToID("_Center");
        int intensityID = Shader.PropertyToID("_Intensity");
        int blurWidthID = Shader.PropertyToID("_BlurWidth");
        int tintID = Shader.PropertyToID("_Tint");

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        RTHandle cameraColorHandle;


        ScriptableRenderer renderer;
        ProfilingSampler sampler;
        public void SetCameraColorTarget(ScriptableRenderer renderer)
        {
            this.renderer = renderer;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDescriptor.depthBufferBits = 0;
            cameraTextureDescriptor.width = Mathf.RoundToInt(cameraTextureDescriptor.width * resolutionScale);
            cameraTextureDescriptor.height = Mathf.RoundToInt(cameraTextureDescriptor.height * resolutionScale);

            RenderingUtils.ReAllocateIfNeeded(ref occluderMapHandle, cameraTextureDescriptor, name: occluderMapName);

            RenderTextureDescriptor depthMapDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            depthMapDescriptor.depthBufferBits = 0;
            depthMapDescriptor.width = renderingData.cameraData.cameraTargetDescriptor.width;
            depthMapDescriptor.height = renderingData.cameraData.cameraTargetDescriptor.height;

            RenderingUtils.ReAllocateIfNeeded(ref depthMapHandle, depthMapDescriptor, name: depthMapName);

            ConfigureTarget(occluderMapHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!occluderMaterial || !radialBlurMaterial)
            {
                return;
            }

            if (RenderSettings.sun == null || !RenderSettings.sun.enabled)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.Clear();

            using (new ProfilingScope(cmd, sampler))
            {

                cameraColorHandle = renderer.cameraColorTargetHandle;

                Camera camera = renderingData.cameraData.camera;
                context.DrawSkybox(camera);

                DrawingSettings drawSettings = CreateDrawingSettings(shaderTagIds, ref renderingData, SortingCriteria.CommonOpaque);
                drawSettings.overrideMaterial = occluderMaterial;

                if (!useDepthTextureApproach)
                {
                    context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
                }

                Vector3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;

                Vector3 cameraPositionWorldSpace = camera.transform.position;
                Vector3 sunPositionWorldSpace = cameraPositionWorldSpace + sunDirectionWorldSpace;
                Vector3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);
                sunPositionViewportSpace = sunPositionViewportSpace.normalized;

                radialBlurMaterial.SetVector(centerID, new Vector4(sunPositionViewportSpace.x, sunPositionViewportSpace.y, 0, 0));
                radialBlurMaterial.SetFloat(intensityID, intensity);
                radialBlurMaterial.SetFloat(blurWidthID, blurWidth);
                radialBlurMaterial.SetColor(tintID, tint);

                if (useDepthTextureApproach)
                {
                    Blitter.BlitCameraTexture(cmd, occluderMapHandle, depthMapHandle, depthMaterial, 0);
                    Blitter.BlitCameraTexture(cmd, depthMapHandle, cameraColorHandle, radialBlurMaterial, 0);
                }
                else
                {
                    Blitter.BlitCameraTexture(cmd, occluderMapHandle, cameraColorHandle, radialBlurMaterial, 0);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }

        public VolumetricLightPass(VolumetricLightSettings settings)
        {
            useDepthTextureApproach = settings.useDepthTextureApproach;
            occluderMaterial = settings.occluderMaterial;
            radialBlurMaterial = settings.radialBlurMaterial;
            depthMaterial = settings.depthMaterial;

            resolutionScale = settings.resolutionScale;
            intensity = settings.intensity;
            blurWidth = settings.blurWidth;
            tint = settings.lightTint;
            renderPassEvent = settings.renderPassEvent;

            if (settings.shaderTags != null && settings.shaderTags.Count > 0)
            {
                for (int i = 0; i < settings.shaderTags.Count; i++)
                {
                    shaderTagIds.Add(new ShaderTagId(settings.shaderTags[i]));
                }
            }

            profilerTag = settings.profilerTag;
            sampler = new ProfilingSampler(profilerTag);
        }
    }


}
