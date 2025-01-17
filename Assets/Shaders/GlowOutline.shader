Shader "Custom/GlowOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.5)) = 0.07
        _OutlineIntensity ("Outline Intensity", Range(0.0, 1.0)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "Base"
            Tags { "LightMode"="UniversalForward" }
            
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(input.positionOS.xyz));
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float ndotl = saturate(dot(normalWS, mainLight.direction));
                
                // Calculate rim effect for outline
                float rim = 1.0 - abs(dot(viewDirWS, normalWS));
                // Adjust these values to control outline thickness (first number) and sharpness (second number)
                rim = smoothstep(0.2, 0.6, rim);
                
                // Increase outline intensity
                rim = pow(rim, 0.8); // Makes the outline more pronounced
                
                // Blend between base color and outline color based on rim
                float3 finalColor = lerp(texColor.rgb * _BaseColor.rgb * ndotl, 
                                      _OutlineColor.rgb, 
                                      rim * _OutlineIntensity);
                
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }
} 