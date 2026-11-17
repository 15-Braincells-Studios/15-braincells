// Simple round, soft particle (no texture needed). Used for torch embers and smoke.
// Blend mode is set per material: Src Blend = SrcAlpha + Dst Blend = One gives glowing (additive) embers,
// Src Blend = SrcAlpha + Dst Blend = OneMinusSrcAlpha gives normal see-through smoke.
Shader "15Braincells/SoftParticle"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (1, 1, 1, 1)
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.6
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Pass
        {
            Name "SoftParticle"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Softness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // round dot that fades out towards the edge
                float d = length(input.uv - 0.5) * 2.0;
                half alpha = saturate((1.0 - d) / _Softness);
                half4 colour = input.color * _Color;
                return half4(colour.rgb, colour.a * alpha);
            }
            ENDHLSL
        }
    }
}
