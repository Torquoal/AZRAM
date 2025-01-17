Shader "Custom/EmissiveSphere"
{
    Properties
    {
        _EmissionColor ("Emission Color", Color) = (1,1,0,1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 1
        _Transparency ("Transparency", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "Forward"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _EmissionIntensity;
                float _Transparency;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDirWS = GetWorldSpaceViewDir(input.positionWS);
                float3 normalWS = normalize(input.normalWS);
                float NdotV = saturate(dot(normalWS, normalize(viewDirWS)));
                
                // Stronger fresnel effect
                float fresnel = pow(1.0 - NdotV, 3.0);
                
                // Calculate final color with emission
                float3 color = _EmissionColor.rgb * _EmissionIntensity;
                
                // Calculate alpha based on view angle and transparency setting
                float alpha = lerp(_Transparency * 0.2, _Transparency, fresnel);
                
                // Apply fog
                color = MixFog(color, input.fogFactor);
                
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
} 