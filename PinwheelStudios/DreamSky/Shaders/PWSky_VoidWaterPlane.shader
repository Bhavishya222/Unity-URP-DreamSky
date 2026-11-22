// ============================================================================
// Pinwheel DreamSky — Ultra-Light World-Edge Void Mist & Fake Water Plane (URP 17)
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

Shader "Pinwheel Studios/DreamSky/Void & Water Plane"
{
    Properties
    {
        [Header(1. Surface Mode and Noise)]
        [Enum(Void Mist, 0, Fake Water, 1)] _PlaneMode ("Surface Mode", Float) = 0.0
        [NoScaleOffset] _NoiseTex ("Packed Noise Atlas (RGBA)", 2D) = "gray" {}
        _OverallOpacity ("Overall Opacity", Range(0.0, 1.0)) = 0.92
        _AnimationSpeed ("Animation Speed", Range(0.0, 4.0)) = 1.0

        [Header(2. Horizon and Cliff Edge Blending)]
        _HorizonColor ("Horizon Blend Colour", Color) = (0.155, 0.085, 0.275, 1.0)
        _FadeStartDist ("Horizon Fade Start Distance", Float) = 45.0
        _FadeEndDist ("Horizon Fade End Distance", Float) = 240.0
        _EdgeFadePower ("Outer Rim Alpha Fade", Range(0.2, 4.0)) = 1.4

        [Toggle] _UseDepthFade ("Soft Cliff Intersection (Requires URP Depth Tex)", Float) = 1.0
        _DepthFadeDistance ("Cliff Mist Softness (Meters)", Range(0.2, 30.0)) = 6.5
        _CliffRimGlow ("Cliff Edge Mist Glow", Range(0.0, 2.0)) = 0.45

        [Header(3. Void Mist Settings)]
        _VoidDeepColor ("Void Abyss Colour", Color) = (0.025, 0.022, 0.075, 1.0)
        [HDR] _VoidMistColor ("Swirling Mist Colour", Color) = (0.28, 0.26, 0.72, 1.0)
        [HDR] _VoidHighlightColor ("Mist Wisp Highlight", Color) = (0.55, 0.62, 1.0, 1.0)
        _VoidScale ("Mist Pattern Scale", Range(0.002, 0.15)) = 0.018
        _VoidWarp ("Mist Swirl Distortion", Range(0.0, 1.5)) = 0.45
        _VoidSpeed ("Mist Drift Speed", Range(0.0, 3.0)) = 0.40
        _VoidDensity ("Mist Density", Range(0.2, 2.5)) = 1.15

        [Header(4. Fake Night Water Settings)]
        _WaterDeepColor ("Water Deep Colour", Color) = (0.015, 0.022, 0.075, 1.0)
        _WaterReflectColor ("Water Sky Reflection Tint", Color) = (0.15, 0.13, 0.36, 1.0)
        _WaterScale ("Ripple Scale", Range(0.005, 0.25)) = 0.045
        _WaterSpeed ("Ripple Speed", Range(0.0, 3.0)) = 0.75
        _WaterFresnel ("Fresnel Reflection Power", Range(1.0, 6.0)) = 3.0

        [Header(5. Moon Reflection Sync)]
        _MoonAzimuth ("Moon Azimuth (Deg)", Range(0, 360)) = 12.0
        _MoonElevation ("Moon Elevation (Deg)", Range(-15, 88)) = 26.0
        [HDR] _MoonGlowColor ("Moon Reflection Colour", Color) = (0.48, 0.58, 1.0, 1.0)
        _MoonShimmerIntensity ("Moon Shimmer Intensity", Range(0.0, 3.0)) = 1.35

        [Header(6. Retro and Quality)]
        [Toggle] _RetroEnabled ("Match Skybox Retro Stylization", Float) = 0.0
        _RetroColorSteps ("Colour Quantization Levels", Range(3, 48)) = 12.0
        _RetroDitherAmount ("Ordered Bayer Dither Strength", Range(0.0, 1.5)) = 0.95
        _RetroDitherScale ("Dither Matrix Pixel Scale", Range(1, 6)) = 2.0
        _RetroContrast ("Retro Shadow Contrast", Range(0.75, 1.6)) = 1.12

        [KeywordEnum(Low, Medium, High)] _Quality ("Quality Preset", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PinwheelVoidWaterPlanePass"

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            // Strictly 3 variants total (Low / Medium / High)
            #pragma shader_feature_local_fragment _QUALITY_LOW _QUALITY_MEDIUM _QUALITY_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Includes/PWSky_Common.hlsl"
            #include "Includes/PWSky_Retro.hlsl"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float _PlaneMode;
                half  _OverallOpacity;
                float _AnimationSpeed;

                half4 _HorizonColor;
                float _FadeStartDist;
                float _FadeEndDist;
                half  _EdgeFadePower;

                float _UseDepthFade;
                float _DepthFadeDistance;
                half  _CliffRimGlow;

                half4 _VoidDeepColor;
                half4 _VoidMistColor;
                half4 _VoidHighlightColor;
                float _VoidScale;
                float _VoidWarp;
                float _VoidSpeed;
                half  _VoidDensity;

                half4 _WaterDeepColor;
                half4 _WaterReflectColor;
                float _WaterScale;
                float _WaterSpeed;
                half  _WaterFresnel;

                float _MoonAzimuth;
                float _MoonElevation;
                half4 _MoonGlowColor;
                half  _MoonShimmerIntensity;

                float _RetroEnabled;
                half  _RetroColorSteps;
                half  _RetroDitherAmount;
                half  _RetroDitherScale;
                half  _RetroContrast;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.uv         = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float  animTime  = _Time.y * _AnimationSpeed;
                float3 toCam     = _WorldSpaceCameraPos - input.positionWS;
                float  camDist   = length(toCam);
                float3 viewToCam = toCam / max(camDist, 1e-4);

                // Radial quad edge fade so the mesh border disappears invisibly into the skybox
                float2 centeredUV = input.uv * 2.0 - 1.0;
                float  quadDist   = length(centeredUV);
                half   rimAlpha   = pow(saturate(1.0h - (half)smoothstep(0.55, 0.98, quadDist)), _EdgeFadePower);
                if (rimAlpha <= 0.001h)
                    return half4(0.0h, 0.0h, 0.0h, 0.0h);

                half distFade = (half)saturate((camDist - _FadeStartDist) / max(_FadeEndDist - _FadeStartDist, 1.0));

                // Optional soft cliff/mountain intersection using URP Scene Depth Texture
                half cliffFade = 1.0h;
                half cliffRim  = 0.0h;
                #if !defined(_QUALITY_LOW)
                    UNITY_BRANCH
                    if (_UseDepthFade > 0.5)
                    {
                        float2 screenUV  = GetNormalizedScreenSpaceUV(input.positionCS);
                        float  sceneRawZ = SampleSceneDepth(screenUV);
                        float  sceneEyeZ = LinearEyeDepth(sceneRawZ, _ZBufferParams);
                        float  surfEyeZ  = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                        float  depthDiff = max(sceneEyeZ - surfEyeZ, 0.0);

                        half softT = (half)saturate(depthDiff / max(_DepthFadeDistance, 0.05));
                        cliffFade  = smoothstep(0.0h, 1.0h, softT);
                        cliffRim   = (1.0h - cliffFade) * smoothstep(0.0h, 0.25h, softT);
                    }
                #endif

                half3 surfaceCol   = half3(0.0h, 0.0h, 0.0h);
                half  surfaceAlpha = 1.0h;

                // Mode 0: Swirling Dark-Fantasy Void Mist
                if (_PlaneMode < 0.5)
                {
                    float2 worldUV = input.positionWS.xz * _VoidScale;
                    float2 drift1  = float2(animTime * _VoidSpeed * 0.035, -animTime * _VoidSpeed * 0.022);

                    half body;
                    half wisps;

                    #if defined(_QUALITY_LOW)
                        half4 n0 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV + drift1);
                        body  = n0.r * 0.62h + n0.g * 0.38h;
                        wisps = n0.b;
                    #else
                        half4 w0    = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV * 0.55 - drift1 * 0.65);
                        float2 warp = (float2(w0.a, w0.r) - 0.5) * _VoidWarp;
                        half4 n1    = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV + warp + drift1);

                        #if defined(_QUALITY_HIGH)
                            half4 n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV * 1.9 + warp * 1.3 - drift1.yx * 1.15);
                            body  = saturate(n1.r * 0.50h + n1.g * 0.32h + n2.g * 0.18h);
                            wisps = saturate(n1.b * 0.55h + n2.b * 0.45h);
                        #else
                            body  = saturate(n1.r * 0.58h + n1.g * 0.42h);
                            wisps = n1.b;
                        #endif
                    #endif

                    half mistShape = smoothstep(0.18h, 0.88h, body * _VoidDensity);
                    surfaceCol  = lerp(_VoidDeepColor.rgb, _VoidMistColor.rgb, mistShape);
                    surfaceCol += _VoidHighlightColor.rgb * (pow(wisps, 2.0h) * 0.48h * mistShape);
                    surfaceCol += _VoidHighlightColor.rgb * (cliffRim * _CliffRimGlow);
                    surfaceAlpha = saturate(lerp(0.68h, 1.0h, mistShape)) * cliffFade;
                }
                // Mode 1: Ultra-Light Fake Night Water + Moon Shimmer
                else
                {
                    float2 worldUV = input.positionWS.xz * _WaterScale;
                    float2 scroll1 = float2(animTime * _WaterSpeed * 0.045, animTime * _WaterSpeed * 0.028);
                    half4  r1      = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV + scroll1);

                    half  rippleH;
                    half2 rippleN;

                    #if defined(_QUALITY_LOW)
                        rippleH = r1.g;
                        rippleN = (half2(r1.r, r1.b) - 0.5h) * 0.22h;
                    #else
                        float2 scroll2 = float2(-animTime * _WaterSpeed * 0.032, animTime * _WaterSpeed * 0.052);
                        half4  r2      = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldUV * 1.48 + scroll2);
                        rippleH = (r1.g + r2.b) * 0.5h;
                        rippleN = (half2(r1.r - r2.r, r1.g - r2.g)) * 0.28h;
                    #endif

                    rippleN *= (1.0h - distFade * 0.8h);

                    half ndv     = saturate((half)viewToCam.y);
                    half fresnel = pow(1.0h - ndv, _WaterFresnel);

                    half3 reflTint = lerp(_WaterReflectColor.rgb, _HorizonColor.rgb, 0.4h);
                    surfaceCol  = lerp(_WaterDeepColor.rgb, reflTint, saturate(fresnel * 0.85h + rippleH * 0.14h));
                    surfaceCol += _WaterReflectColor.rgb * (smoothstep(0.64h, 0.94h, rippleH) * 0.16h * (1.0h - distFade));

                    float3 moonDir = PWSky_DirFromAzimuthElevation(_MoonAzimuth, _MoonElevation);
                    if (_MoonShimmerIntensity > 0.001h && moonDir.y > -0.06)
                    {
                        float3 waterNormal = normalize(float3((float)rippleN.x, 1.0, (float)rippleN.y));
                        float3 reflDir     = reflect(-viewToCam, waterNormal);

                        half moonAlign = saturate((half)dot(reflDir, moonDir));
                        half broadPath = pow(moonAlign, 20.0h) * 0.45h;
                        half sparkles  = pow(moonAlign, 140.0h) * (1.5h + rippleH * 1.8h);

                        half3 shimmerTint = lerp(_MoonGlowColor.rgb, half3(1.0h, 1.0h, 1.0h), 0.45h);
                        surfaceCol += shimmerTint * ((broadPath + sparkles) * _MoonShimmerIntensity * (1.0h - distFade * 0.35h));
                    }

                    surfaceCol += _WaterReflectColor.rgb * (cliffRim * _CliffRimGlow * 0.85h);
                    surfaceAlpha = cliffFade;
                }

                surfaceCol = lerp(surfaceCol, _HorizonColor.rgb, distFade * 0.72h);

                UNITY_BRANCH
                if (_RetroEnabled > 0.5)
                {
                    surfaceCol = PWSky_ApplyRetroColorPass(
                        surfaceCol,
                        input.positionCS.xy,
                        _RetroColorSteps,
                        _RetroDitherAmount,
                        _RetroDitherScale,
                        _RetroContrast);
                }

                half finalAlpha = saturate(surfaceAlpha * rimAlpha * _OverallOpacity);
                return half4(surfaceCol, finalAlpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}