using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.Universal.RenderObjects;

public class RenderObjectsToTexture : ScriptableRendererFeature
{
    [SerializeField] Settings settings = new Settings();
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

    public override void Create()
    {
        pass = new RenderObjectsToTexturePass(settings);
    }

    RenderObjectsToTexturePass pass;
}

[System.Serializable]
public class Settings
{
    public string profilingTag = "RenderObjectsFeature";

    public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

    public LayerMask layerMask;
    public Vector2Int renderQueueRange;

    public Material overrideMaterial = null;

    public int overrideMaterialPassIndex = 0;
    public int depthBufferBits;

    public List<string> shaderTags;
    public RenderTextureFormat colorFormat;
    public string renderTargetName;

    public bool overrideResolution;
    public Vector2Int resolution;

    public bool toRenderTexture = false;
    public RenderTexture renderTexture;
    public bool releaseRT = true;

    public bool setGlobalTexture = false;
    public string globalTextureName;

}

public class RenderObjectsToTexturePass : ScriptableRenderPass
{
    Settings settings;
    ProfilingSampler sampler;
    List<ShaderTagId> shaderTagIds = new List<ShaderTagId>() { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly")};
    FilteringSettings filteringSettings;

    RTHandle renderTargetHandle;

    public RenderTexture rt;
    internal RenderObjectsToTexturePass(Settings settings)
    {
        this.settings = settings;

        RenderQueueRange renderQueueRange = new RenderQueueRange(settings.renderQueueRange.x, settings.renderQueueRange.y);
        filteringSettings = new FilteringSettings(renderQueueRange, settings.layerMask);

        sampler = new ProfilingSampler(settings.profilingTag);
        if (settings.shaderTags != null && settings.shaderTags.Count > 0)
        {
            foreach (var passName in settings.shaderTags)
                shaderTagIds.Add(new ShaderTagId(passName));
        }
    }

    public void Setup(RTHandle destination)
    {
        renderTargetHandle = destination;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if(renderTargetHandle == null)
        {
            RenderTextureDescriptor descriptor = cameraTextureDescriptor;
            if (settings.overrideResolution)
            {
                descriptor.width = settings.resolution.x;
                descriptor.height = settings.resolution.y;
            }
            descriptor.colorFormat = settings.colorFormat;
            descriptor.depthBufferBits = settings.depthBufferBits;

            renderTargetHandle = RTHandles.Alloc(in descriptor, name: settings.renderTargetName);
        }

        if (settings.toRenderTexture && settings.renderTexture != null)
        {
            ConfigureTarget(settings.renderTexture);
        }
        else
        {
            ConfigureTarget(renderTargetHandle);
        }

        if (settings.setGlobalTexture)
        {
            Shader.SetGlobalTexture(settings.globalTextureName, renderTargetHandle);
        }
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        cmd.Clear();

        DrawingSettings drawSettings = CreateDrawingSettings(shaderTagIds, ref renderingData, SortingCriteria.CommonOpaque);
        drawSettings.overrideMaterial = settings.overrideMaterial;
        drawSettings.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;

        ref CameraData cameraData = ref renderingData.cameraData;
        Camera camera = cameraData.camera;

     

        using (new ProfilingScope(cmd, sampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);
        
        if(settings.releaseRT)
        {
            renderTargetHandle?.Release();
        }
    }

}