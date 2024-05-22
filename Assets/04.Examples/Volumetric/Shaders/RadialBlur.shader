Shader "Hidden/RadialBlur"
{
    SubShader
    {
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
          
            CBUFFER_START(UnityPerMaterial)
                float _BlurWidth;
                float _Intensity;
                float4 _Center;
                float4 _Tint;
            CBUFFER_END

            #define SAMPLE_COUNT 200

            half4 frag (Varyings i) : SV_Target
            {
                half4 color = half4(0.0, 0.0, 0.0, 1.0);

                float2 ray = i.texcoord - _Center.xy;
                
                float segment = _BlurWidth / float(SAMPLE_COUNT);

                [unroll]
                for (int i = 0; i < SAMPLE_COUNT; i++)
                {
                    //float scale = 1.0f - _BlurWidth * (float(i) / float(SAMPLE_COUNT - 1));
                    float scale = 1.0 - segment * (float)i;
                    color.xyz += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, (ray * scale) +  _Center.xy).xyz / float(SAMPLE_COUNT);
                }

                color = color * _Intensity * _Tint;
                color.a = 1.0;
                return color;
            }
            ENDHLSL
        }
    }
}
