// Stylised cartoon flame for torches, candles and fireplaces.
// Put it on a Unity Quad: the quad always turns towards the camera (only around the Y axis),
// so the flame stays upright. The shape and the colour bands are made with noise in the shader,
// so no texture is needed. _FlameIntensity and _Seed are set per torch by TorchFlicker.cs.
Shader "15Braincells/StylizedFlame"
{
    Properties
    {
        [Header(Colours)]
        [HDR] _CoreColor ("Core Colour", Color) = (4.0, 3.0, 1.2, 1)
        [HDR] _MidColor ("Middle Colour", Color) = (3.0, 1.2, 0.2, 1)
        [HDR] _EdgeColor ("Edge Colour", Color) = (1.5, 0.3, 0.04, 1)

        [Header(Bands)]
        _CoreThreshold ("Core Threshold", Range(0, 1)) = 0.72
        _MidThreshold ("Middle Threshold", Range(0, 1)) = 0.45
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.16
        _BandSoftness ("Band Softness", Range(0.001, 0.2)) = 0.025

        [Header(Shape and Motion)]
        _Width ("Width", Range(0.1, 1)) = 0.75
        _NoiseScale ("Noise Scale", Float) = 4
        _ScrollSpeed ("Scroll Speed", Float) = 1.8
        _Distortion ("Sideways Wobble", Range(0, 1)) = 0.4
        _Erosion ("Top Break-up", Range(0, 1)) = 0.5

        [Header(Set by TorchFlicker)]
        _FlameIntensity ("Intensity", Float) = 1
        _Seed ("Seed", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            // the billboard needs the object's own position, so no batching
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Flame"
            Tags { "LightMode" = "UniversalForward" }

            // premultiplied alpha: clean colour bands that still glow with bloom
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _MidColor;
                half4 _EdgeColor;
                half _CoreThreshold;
                half _MidThreshold;
                half _EdgeThreshold;
                half _BandSoftness;
                half _Width;
                float _NoiseScale;
                float _ScrollSpeed;
                half _Distortion;
                half _Erosion;
                half _FlameIntensity;
                float _Seed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int k = 0; k < 3; k++)
                {
                    value += amplitude * ValueNoise(p);
                    p *= 2.03;
                    amplitude *= 0.5;
                }
                return value;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // turn the quad towards the camera, but keep it standing upright
                float3 center = TransformObjectToWorld(float3(0, 0, 0));
                float scaleX = length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20));
                float scaleY = length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21));

                float3 toCamera = _WorldSpaceCameraPos - center;
                toCamera.y = 0;
                toCamera = dot(toCamera, toCamera) > 1e-6 ? normalize(toCamera) : float3(0, 0, 1);
                float3 right = normalize(cross(float3(0, 1, 0), toCamera));

                float3 positionWS = center
                    + right * input.positionOS.x * scaleX
                    + float3(0, 1, 0) * input.positionOS.y * scaleY;

                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float h = uv.y;

                // noise that scrolls upwards, different per torch
                float t = _Time.y * _ScrollSpeed;
                float n = Fbm(float2(uv.x * _NoiseScale * 0.6 + _Seed, uv.y * _NoiseScale - t + _Seed * 1.7));

                // -1..1 across the quad, wobbling more towards the top
                float x = (uv.x - 0.5) * 2.0;
                x += (n - 0.5) * _Distortion * h * 2.0;

                // teardrop: round at the bottom, pointy at the top
                float width = _Width * sqrt(saturate(h / 0.18)) * pow(saturate(1.0 - h), 0.8);
                float shape = 1.0 - abs(x) / max(width, 1e-3);

                // break the top up with noise, keep the base solid
                float v = shape - n * _Erosion * h + (1.0 - h) * 0.15;
                v *= _FlameIntensity;

                float s = _BandSoftness;
                float edge = smoothstep(_EdgeThreshold - s, _EdgeThreshold + s, v);
                float mid = smoothstep(_MidThreshold - s, _MidThreshold + s, v);
                float core = smoothstep(_CoreThreshold - s, _CoreThreshold + s, v);

                half3 colour = _EdgeColor.rgb;
                colour = lerp(colour, _MidColor.rgb, mid);
                colour = lerp(colour, _CoreColor.rgb, core);

                return half4(colour * edge, edge);
            }
            ENDHLSL
        }
    }
}
