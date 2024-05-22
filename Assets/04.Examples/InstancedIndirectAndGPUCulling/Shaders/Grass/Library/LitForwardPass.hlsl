#ifndef INSTANCED_FORWARD_LIT_PASS_INCLUDED
#define INSTANCED_FORWARD_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "SpaceTransforms.hlsl"

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID

    #ifdef INSTANCED_ENABLE
        uint instanceID : SV_INSTANCEID;
    #endif
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
#endif

    float3 normalWS                 : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
#endif

   
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
#endif

    float4 positionOS : TEXCOORD10;
    float windColor : TEXCOORD11;
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
    #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

#ifdef INSTANCED_ENABLE
VertexPositionInputs InstancedGetVertexPositionInputs(float3 positionOS, uint instanceID)
{
    VertexPositionInputs input;
    input.positionWS = InstancedTransformObjectToWorld(positionOS, instanceID);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = InstancedTransformObjectToHClip(positionOS, instanceID);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs InstancedGetVertexNormalInputs(float3 normalOS, float4 tangentOS, uint instanceID)
{
    VertexNormalInputs tbn;

    // mikkts space compliant. only normalize when extracting normal at frag.
    real sign = real(tangentOS.w) * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS, instanceID);
    tbn.tangentWS = real3(TransformObjectToWorldDir(tangentOS.xyz, instanceID));
    tbn.bitangentWS = real3(cross(tbn.normalWS, float3(tbn.tangentWS))) * sign;
    return tbn;
}
#endif

float3 GetScale(Attributes input)
{
    #ifdef INSTANCED_ENABLE
        float4x4 mat = GetObjectToWorldMatrix(input.instanceID);
    #else
        float4x4 mat = GetObjectToWorldMatrix();
    #endif

    half3 scale = half3(
        length(mat._m00_m10_m20),
        length(mat._m01_m11_m21),
        length(mat._m02_m12_m22)
    );

    return scale;
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float scale = GetScale(input).y;
    
    float h = smoothstep(0, _GrassStiffness / scale, input.positionOS.y);

    float3 posOS = input.positionOS.xyz;
    
    #ifdef INSTANCED_ENABLE
        float3 posWS = InstancedTransformObjectToWorld(posOS, input.instanceID);
        float3 posWSOrigin = InstancedTransformObjectToWorld(float3(0.0, 0.0, 0.0), input.instanceID);
    #else
        float3 posWS = TransformObjectToWorld(posOS);
        float3 posWSOrigin = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    #endif

    //trample
    float dist = distance(_PlayerPosition.xyz + float3(0.0, _PlayerPositionHeightOffset, 0.0), posWS);
    float pushDown = saturate(1 - dist + _PlayerPosition.w);
    float3 bendDirection = _PlayerPosition.xyz - posWS;
    //bendDirection.y = 0.0;
    bendDirection = normalize(bendDirection);
    bendDirection.y = -0.5 * input.color.r * h;
    float3 pushDownVector = bendDirection * pushDown  * input.color.g * h * 2;
    input.positionOS.xyz += pushDownVector;


    float2 noiseUV = _Time.y * _SwaySpeed + input.texcoord;
    float noise = GradientNoise(noiseUV, 5);

    float2 windTextureUV = posWS.xz / _WindTextureScale;
    windTextureUV += _Time.y * _WindTextureSpeed;
    windTextureUV = RotateUV(windTextureUV, _WindDirection);

    float wind = tex2Dlod(_WindTexture, float4(windTextureUV, 0.0, 0.0)).r;
    wind = smoothstep(_WindContrast.x, _WindContrast.y, wind);

    noise *= _WindIntensity;
    noise += wind;

    float3 windVector = noise * float3(1.0, 0, 1.0) * lerp(0.0, 1.0, h) * (1 - pushDown);

    input.positionOS.xyz += windVector;

    output.windColor = wind;




    #ifdef INSTANCED_ENABLE
        VertexPositionInputs vertexInput = InstancedGetVertexPositionInputs(input.positionOS.xyz, input.instanceID);
    #else
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    #endif

 

    output.positionOS.xyz = input.positionOS.xyz;
    output.positionOS.w = scale;

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    #ifdef INSTANCED_ENABLE
        VertexNormalInputs normalInput = InstancedGetVertexNormalInputs(input.normalOS, input.tangentOS, input.instanceID);
    #else
        VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    #endif

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    return output;
}

// Used in Standard (Physically Based) shader
void LitPassFragment(
    Varyings input,
    float facing : VFACE
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 objPos = UNITY_MATRIX_M._m03_m13_m23;
    float3 cameraPos = _WorldSpaceCameraPos;
    float camDistance = 1 - distance(objPos, cameraPos);


//#if defined(_PARALLAXMAP)
//#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
//    half3 viewDirTS = input.viewDirTS;
//#else
//    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
//    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
//#endif
//    ApplyPerPixelDisplacement(viewDirTS, input.uv);
//#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);
    
    
    float2 positionWS = input.positionWS.xz;
    float2 uv = float2(positionWS.x / _TerrainSize.x, positionWS.y / _TerrainSize.y); 
    float3 terrainColor = tex2D(_TerrainColor, uv).rgb;
    
    float scale = input.positionOS.w;
    float h = smoothstep(0, _BottomColorHeight / scale, input.positionOS.y);
    float terrainBlendHeight = 1 - smoothstep(0, _TerrainBlendHeight / scale, input.positionOS.y);
    surfaceData.albedo = lerp(_BottomColor.rgb, saturate(surfaceData.albedo + _WindHighLight.rgb * input.windColor), h);
    float terrainBlend = saturate(_TerrainBlend + terrainBlendHeight);
    surfaceData.albedo  = surfaceData.albedo * (1 - terrainBlend) + terrainColor * terrainBlend;

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif
    
    input.normalWS.z *= facing;

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    

    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}



#endif
