#ifndef INSTANCED_SPACE_TRANSFORMS_INCLUDED
#define INSTANCED_SPACE_TRANSFORMS_INCLUDED

#ifdef INSTANCED_ENABLE
inline uint GetVisibleID(uint instanceID)
{
    //return instanceID;
    return _VisibleIDBuffer[instanceID];
}

float4x4 GetObjectToWorldMatrix(uint instanceID)
{
    uint id = GetVisibleID(instanceID);

    return _O2WBuffer[id];
}

float4x4 GetWorldToObjectMatrix(uint instanceID)
{
    uint id = GetVisibleID(instanceID);

    return _W2OBuffer[id];
}

float4 TransformWorldToHClip(float3 positionWS, uint instanceID)
{
    return mul(GetWorldToHClipMatrix(), float4(positionWS, 1.0));
}

float3 InstancedTransformObjectToWorld(float3 positionOS, uint instanceID)
{
    return mul(GetObjectToWorldMatrix(instanceID), float4(positionOS, 1.0)).xyz;
}

float4 InstancedTransformObjectToHClip(float3 positionOS, uint instanceID)
{
    // More efficient than computing M*VP matrix product
    return mul(GetWorldToHClipMatrix(), mul(GetObjectToWorldMatrix(instanceID), float4(positionOS, 1.0)));
}

float3 TransformObjectToWorldNormal(float3 normalOS, uint instanceID, bool doNormalize = true)
{
    #ifdef UNITY_ASSUME_UNIFORM_SCALING
        return TransformObjectToWorldDir(normalOS, instanceID, doNormalize);
    #else
    // Normal need to be multiply by inverse transpose
    float3 normalWS = mul(normalOS, (float3x3)GetWorldToObjectMatrix(instanceID));
    if (doNormalize)
        return SafeNormalize(normalWS);

    return normalWS;
    #endif
}

// Normalize to support uniform scaling
float3 TransformObjectToWorldDir(float3 dirOS, uint instanceID, bool doNormalize = true)
{
    //#ifndef SHADER_STAGE_RAY_TRACING
    float3 dirWS = mul((float3x3)GetObjectToWorldMatrix(instanceID), dirOS);
    //#else
    //float3 dirWS = mul((float3x3)ObjectToWorld3x4(), dirOS);
    //#endif
    if (doNormalize)
        return SafeNormalize(dirWS);

    return dirWS;
}
#endif


#endif
