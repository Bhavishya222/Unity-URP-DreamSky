// ============================================================================
// Pinwheel DreamSky — Procedural Fantasy & Realistic Night Skybox (URP 17)
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

Shader "Pinwheel Studios/DreamSky/Night Skybox"
{
    Properties
    {
        // --- 1. General Settings ---
        [Header(1. General Settings)]
        [NoScaleOffset] _NoiseTex ("Packed Noise Atlas (RGBA)", 2D) = "gray" {}
        _Exposure ("Sky Exposure", Range(0.2, 3.0)) = 1.0
        _SkyRotation ("Sky Yaw Rotation", Range(0, 360)) = 0.0
        _DitherStrength ("Anti-Banding Dither", Range(0.0, 2.0)) = 1.0

        // --- 2. Sky Colours ---
        [Header(2. Sky Colours)]
        _ZenithColor ("Zenith Colour", Color) = (0.012, 0.014, 0.048, 1.0)
        _MidSkyColor ("Mid-Sky Colour", Color) = (0.055, 0.038, 0.145, 1.0)
        _HorizonColor ("Horizon Colour", Color) = (0.155, 0.085, 0.275, 1.0)
        _GroundColor ("Lower / Void Base Colour", Color) = (0.018, 0.014, 0.052, 1.0)
        _HorizonHeight ("Horizon Height Offset", Range(-0.25, 0.25)) = 0.0
        _HorizonFalloff ("Horizon Gradient Falloff", Range(0.25, 4.0)) = 1.25

        // --- 3. Stars ---
        [Header(3. Stars)]
        [Toggle] _EnableStars ("Enable Stars", Float) = 1.0
        _StarTint ("Star Overall Tint", Color) = (0.95, 0.96, 1.0, 1.0)
        _StarDensity ("Star Density", Range(0.0, 1.0)) = 0.65
        _StarSize ("Star Size", Range(0.2, 6.0)) = 1.15
        _StarSizeVariation ("Star Size Variation", Range(0.0, 1.0)) = 0.75
        _StarBrightness ("Star Brightness", Range(0.0, 4.0)) = 1.45
        _StarColorVariation ("Star Colour Variation", Range(0.0, 1.0)) = 0.75
        _StarTwinkleSpeed ("Twinkle Speed", Range(0.0, 6.0)) = 1.8
        _StarTwinkleIntensity ("Twinkle Intensity", Range(0.0, 1.0)) = 0.65
        _HeroStarAmount ("Bright Hero Stars", Range(0.0, 1.0)) = 0.65
        _HeroFlareIntensity ("Hero Star Cross Flares", Range(0.0, 1.5)) = 0.55

        // --- 4. Nebula ---
        [Header(4. Nebula)]
        [Toggle] _EnableNebula ("Enable Nebula", Float) = 1.0
        [HDR] _NebulaColorPrimary ("Nebula Primary Colour", Color) = (0.20, 0.10, 0.52, 1.0)
        [HDR] _NebulaColorSecondary ("Nebula Secondary Colour", Color) = (0.58, 0.16, 0.68, 1.0)
        [HDR] _NebulaColorAccent ("Nebula Filament Accent", Color) = (0.34, 0.62, 1.0, 1.0)
        _NebulaIntensity ("Nebula Intensity", Range(0.0, 3.0)) = 0.95
        _NebulaOpacity ("Nebula Opacity", Range(0.0, 1.0)) = 0.75
        _NebulaScale ("Nebula Scale", Range(0.4, 6.0)) = 1.65
        _NebulaDistortion ("Nebula Swirl Distortion", Range(0.0, 2.5)) = 0.95
        _NebulaDriftSpeed ("Nebula Drift Speed", Range(0.0, 3.0)) = 0.35
        _NebulaColorBlend ("Nebula Colour Blending", Range(0.0, 1.0)) = 0.65

        // --- 5. Clouds ---
        [Header(5. Clouds)]
        [Toggle] _EnableClouds ("Enable Clouds", Float) = 1.0
        _CloudBaseColor ("Cloud Shadow Colour", Color) = (0.045, 0.032, 0.105, 1.0)
        _CloudLitColor ("Cloud Lit Colour", Color) = (0.24, 0.15, 0.42, 1.0)
        [HDR] _CloudRimColor ("Cloud Moon-Rim Glow", Color) = (0.62, 0.48, 1.0, 1.0)
        _CloudRimIntensity ("Cloud Rim Glow Intensity", Range(0.0, 3.0)) = 1.15
        _CloudCoverage ("Cloud Coverage", Range(0.0, 1.0)) = 0.48
        _CloudDensity ("Cloud Density", Range(0.1, 2.5)) = 1.15
        _CloudSoftness ("Cloud Softness", Range(0.0, 1.0)) = 0.55
        _CloudOpacity ("Cloud Opacity", Range(0.0, 1.0)) = 0.85
        _CloudLayer1Scale ("Layer 1 Scale", Range(0.3, 5.0)) = 1.15
        _CloudLayer1Speed ("Layer 1 Velocity (XY)", Vector) = (0.35, 0.12, 0.0, 0.0)
        _CloudLayer2Scale ("Layer 2 Scale", Range(0.3, 6.0)) = 1.85
        _CloudLayer2Speed ("Layer 2 Velocity (XY)", Vector) = (-0.22, 0.18, 0.0, 0.0)

        // --- 6. Moon ---
        [Header(6. Moon)]
        [Toggle] _EnableMoon ("Enable Moon", Float) = 1.0
        [NoScaleOffset] _MoonTex ("Moon Surface Texture", 2D) = "white" {}
        _MoonAzimuth ("Moon Azimuth (Deg)", Range(0, 360)) = 12.0
        _MoonElevation ("Moon Elevation (Deg)", Range(-15, 88)) = 26.0
        _MoonSize ("Moon Size", Range(0.015, 0.28)) = 0.085
        [HDR] _MoonColor ("Moon Colour", Color) = (0.88, 0.93, 1.0, 1.0)
        _MoonBrightness ("Moon Brightness", Range(0.2, 4.0)) = 1.35
        _MoonSurfaceDetail ("Surface Detail Strength", Range(0.0, 1.0)) = 0.85
        _MoonPhase ("Moon Phase (0 Full - 0.5 New)", Range(0.0, 1.0)) = 0.0
        _MoonPhaseSoftness ("Phase Edge Softness", Range(0.02, 0.5)) = 0.14
        _MoonEarthshine ("Dark-Side Earthshine", Range(0.0, 0.35)) = 0.08
        [HDR] _MoonGlowColor ("Moon Glow Colour", Color) = (0.45, 0.54, 0.98, 1.0)
        _MoonGlowIntensity ("Moon Glow Intensity", Range(0.0, 3.0)) = 0.90
        _MoonGlowRadius ("Moon Glow Radius", Range(0.04, 0.9)) = 0.28

        // --- 7. Atmospheric Effects, Shooting Stars & Lower Hemisphere ---
        [Header(7. Atmospheric Effects and Lower Hemisphere)]
        [HDR] _AtmosphereGlowColor ("Horizon Glow Colour", Color) = (0.32, 0.16, 0.54, 1.0)
        _AtmosphereGlowIntensity ("Horizon Glow Intensity", Range(0.0, 2.0)) = 0.45
        _EnvColorVariation ("Subtle Sky Colour Variation", Range(0.0, 1.0)) = 0.30

        [Toggle] _EnableShootingStars ("Enable Shooting Stars", Float) = 1.0
        [HDR] _ShootingStarColor ("Shooting Star Colour", Color) = (0.82, 0.92, 1.0, 1.0)
        _ShootingStarFrequency ("Shooting Star Frequency", Range(0.05, 3.0)) = 0.65
        _ShootingStarSpeed ("Shooting Star Speed", Range(0.3, 3.0)) = 1.15
        _ShootingStarLength ("Shooting Star Trail Length", Range(0.2, 2.5)) = 1.0
        _ShootingStarBrightness ("Shooting Star Brightness", Range(0.0, 4.0)) = 1.65

        [Toggle] _EnableCelestial ("Enable Distant Celestial Object", Float) = 0.0
        _CelestialAzimuth ("Celestial Azimuth (Deg)", Range(0, 360)) = 320.0
        _CelestialElevation ("Celestial Elevation (Deg)", Range(5, 85)) = 42.0
        _CelestialSize ("Celestial Size", Range(0.01, 0.14)) = 0.045
        _CelestialColor ("Celestial Colour", Color) = (0.58, 0.46, 0.88, 1.0)
        _CelestialOpacity ("Celestial Opacity", Range(0.0, 1.0)) = 0.45
        _CelestialRingIntensity ("Planetary Ring Intensity", Range(0.0, 1.0)) = 0.65
        _CelestialRingTilt ("Planetary Ring Tilt (Deg)", Range(-60, 60)) = 22.0

        [Enum(Solid Gradient, 0, Void Mist, 1, Fake Water, 2)] _LowerMode ("Lower Hemisphere Mode", Float) = 1.0
        [HDR] _VoidMistColor ("Void Mist Colour", Color) = (0.26, 0.22, 0.65, 1.0)
        _VoidMistScale ("Void Mist Scale", Range(0.3, 6.0)) = 1.85
        _VoidMistSpeed ("Void Mist Swirl Speed", Range(0.0, 3.0)) = 0.45
        _VoidMistIntensity ("Void Mist Intensity", Range(0.0, 2.0)) = 0.95

        _WaterDeepColor ("Fake Water Deep Colour", Color) = (0.015, 0.022, 0.075, 1.0)
        _WaterReflectColor ("Fake Water Reflection Tint", Color) = (0.14, 0.12, 0.34, 1.0)
        _WaterScale ("Fake Water Ripple Scale", Range(0.3, 5.0)) = 1.6
        _WaterSpeed ("Fake Water Ripple Speed", Range(0.0, 3.0)) = 0.75
        _WaterMoonShimmer ("Fake Water Moon Shimmer", Range(0.0, 3.0)) = 1.25

        // --- 8. Animation & Retro Stylization ---
        [Header(8. Animation and Retro Mode)]
        _AnimationSpeed ("Global Animation Speed", Range(0.0, 4.0)) = 1.0

        [Toggle] _RetroEnabled ("Enable Retro Stylization (PS1 / PS2)", Float) = 0.0
        _RetroPixelate ("Sky Pixelation Blend (0 Off - 1 Full)", Range(0.0, 1.0)) = 1.0
        _RetroPixelResolution ("Sky Texel Resolution", Range(48, 480)) = 160.0
        _RetroColorSteps ("Colour Quantization Levels", Range(3, 48)) = 12.0
        _RetroDitherAmount ("Ordered Bayer Dither Strength", Range(0.0, 1.5)) = 0.95
        _RetroDitherScale ("Dither Matrix Pixel Scale", Range(1, 6)) = 2.0
        _RetroContrast ("Retro Shadow Contrast", Range(0.75, 1.6)) = 1.12

        // --- 9. Performance / Quality ---
        [Header(9. Performance and Quality)]
        [KeywordEnum(Low, Medium, High)] _Quality ("Quality Preset", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            Name "PinwheelDreamSkyPass"

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            // Strictly 3 variants total (Low / Medium / High) — zero variant bloat
            #pragma shader_feature_local_fragment _QUALITY_LOW _QUALITY_MEDIUM _QUALITY_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Includes/PWSky_Common.hlsl"
            #include "Includes/PWSky_Stars.hlsl"
            #include "Includes/PWSky_Nebula.hlsl"
            #include "Includes/PWSky_Clouds.hlsl"
            #include "Includes/PWSky_Celestial.hlsl"
            #include "Includes/PWSky_ShootingStars.hlsl"
            #include "Includes/PWSky_LowerHemisphere.hlsl"
            #include "Includes/PWSky_Retro.hlsl"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            TEXTURE2D(_MoonTex);
            SAMPLER(sampler_MoonTex);

            CBUFFER_START(UnityPerMaterial)
                // 1. General
                half  _Exposure;
                float _SkyRotation;
                half  _DitherStrength;

                // 2. Sky Colours
                half4 _ZenithColor;
                half4 _MidSkyColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                half  _HorizonHeight;
                half  _HorizonFalloff;

                // 3. Stars
                float _EnableStars;
                half4 _StarTint;
                float _StarDensity;
                float _StarSize;
                half  _StarSizeVariation;
                half  _StarBrightness;
                half  _StarColorVariation;
                float _StarTwinkleSpeed;
                half  _StarTwinkleIntensity;
                half  _HeroStarAmount;
                half  _HeroFlareIntensity;

                // 4. Nebula
                float _EnableNebula;
                half4 _NebulaColorPrimary;
                half4 _NebulaColorSecondary;
                half4 _NebulaColorAccent;
                half  _NebulaIntensity;
                half  _NebulaOpacity;
                float _NebulaScale;
                float _NebulaDistortion;
                float _NebulaDriftSpeed;
                half  _NebulaColorBlend;

                // 5. Clouds
                float  _EnableClouds;
                half4  _CloudBaseColor;
                half4  _CloudLitColor;
                half4  _CloudRimColor;
                half   _CloudRimIntensity;
                half   _CloudCoverage;
                half   _CloudDensity;
                half   _CloudSoftness;
                half   _CloudOpacity;
                float  _CloudLayer1Scale;
                float4 _CloudLayer1Speed;
                float  _CloudLayer2Scale;
                float4 _CloudLayer2Speed;

                // 6. Moon
                float _EnableMoon;
                float _MoonAzimuth;
                float _MoonElevation;
                float _MoonSize;
                half4 _MoonColor;
                half  _MoonBrightness;
                half  _MoonSurfaceDetail;
                float _MoonPhase;
                half  _MoonPhaseSoftness;
                half  _MoonEarthshine;
                half4 _MoonGlowColor;
                half  _MoonGlowIntensity;
                float _MoonGlowRadius;

                // 7. Atmospheric Effects, Shooting Stars & Lower Hemisphere
                half4 _AtmosphereGlowColor;
                half  _AtmosphereGlowIntensity;
                half  _EnvColorVariation;

                float _EnableShootingStars;
                half4 _ShootingStarColor;
                float _ShootingStarFrequency;
                float _ShootingStarSpeed;
                float _ShootingStarLength;
                half  _ShootingStarBrightness;

                float _EnableCelestial;
                float _CelestialAzimuth;
                float _CelestialElevation;
                float _CelestialSize;
                half4 _CelestialColor;
                half  _CelestialOpacity;
                half  _CelestialRingIntensity;
                float _CelestialRingTilt;

                float _LowerMode;
                half4 _VoidMistColor;
                float _VoidMistScale;
                float _VoidMistSpeed;
                half  _VoidMistIntensity;

                half4 _WaterDeepColor;
                half4 _WaterReflectColor;
                float _WaterScale;
                float _WaterSpeed;
                half  _WaterMoonShimmer;

                // 8. Animation & Retro
                float _AnimationSpeed;
                float _RetroEnabled;
                float _RetroPixelate;
                float _RetroPixelResolution;
                half  _RetroColorSteps;
                half  _RetroDitherAmount;
                half  _RetroDitherScale;
                half  _RetroContrast;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDirWS  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 RotateAroundY(float3 dir, float degrees)
            {
                float rad = degrees * PWSKY_DEG2RAD;
                float s, c;
                sincos(rad, s, c);
                return float3(dir.x * c - dir.z * s, dir.y, dir.x * s + dir.z * c);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDirWS  = RotateAroundY(input.positionOS.xyz, _SkyRotation);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 smoothDir  = normalize(input.viewDirWS);
                // Computed before any retro snapping so fwidth() never spikes on texel edges
                float  pixelAngle = max(length(fwidth(smoothDir)), 0.0004);
                float3 viewDir    = smoothDir;

                UNITY_BRANCH
                if (_RetroEnabled > 0.5 && _RetroPixelate > 0.01)
                {
                    float3 qDir = PWSky_QuantizeViewDirection(smoothDir, _RetroPixelResolution);
                    viewDir = normalize(lerp(smoothDir, qDir, _RetroPixelate));
                    // Match star angular width to retro texel size so stars snap cleanly to blocks
                    float texelAngle = (PWSKY_PI / max(_RetroPixelResolution, 32.0)) * 0.55 * _RetroPixelate;
                    pixelAngle = max(pixelAngle, texelAngle);
                }

                float  animTime = _Time.y * _AnimationSpeed;
                float3 moonDir  = PWSky_DirFromAzimuthElevation(_MoonAzimuth, _MoonElevation);

                // 1. Base Sky Gradient & Atmospheric Horizon Glow
                half3 skyColor = PWSky_EvaluateBaseGradient(
                    viewDir,
                    _ZenithColor.rgb,
                    _MidSkyColor.rgb,
                    _HorizonColor.rgb,
                    _GroundColor.rgb,
                    _HorizonHeight,
                    _HorizonFalloff,
                    _AtmosphereGlowIntensity,
                    _AtmosphereGlowColor.rgb,
                    _EnvColorVariation,
                    animTime);

                // 2. Domain-Warped Organic Nebula
                half nebulaDensity = 0.0h;
                UNITY_BRANCH
                if (_EnableNebula > 0.5)
                {
                    half4 nebRes = PWSky_EvaluateNebula(
                        viewDir,
                        TEXTURE2D_ARGS(_NoiseTex, sampler_NoiseTex),
                        _NebulaScale,
                        _NebulaDistortion,
                        _NebulaDriftSpeed,
                        _NebulaIntensity,
                        _NebulaOpacity,
                        _NebulaColorBlend,
                        _NebulaColorPrimary.rgb,
                        _NebulaColorSecondary.rgb,
                        _NebulaColorAccent.rgb,
                        animTime);

                    skyColor      += nebRes.rgb;
                    nebulaDensity  = nebRes.a;
                }

                // 3. Procedural Multi-Layer Twinkling Stars
                half3 starColor = half3(0.0h, 0.0h, 0.0h);
                UNITY_BRANCH
                if (_EnableStars > 0.5)
                {
                    starColor = PWSky_EvaluateStarfield(
                        viewDir,
                        pixelAngle,
                        _StarDensity,
                        _StarSize,
                        _StarSizeVariation,
                        _StarBrightness,
                        _StarTwinkleSpeed,
                        _StarTwinkleIntensity,
                        _StarColorVariation,
                        _HeroStarAmount,
                        _HeroFlareIntensity,
                        _StarTint.rgb,
                        animTime);

                    starColor *= lerp(1.0h, 0.72h, nebulaDensity);
                }

                // 4. Optional Procedural Shooting Stars
                half3 meteorColor = half3(0.0h, 0.0h, 0.0h);
                UNITY_BRANCH
                if (_EnableShootingStars > 0.5)
                {
                    meteorColor = PWSky_EvaluateShootingStars(
                        viewDir,
                        pixelAngle,
                        _ShootingStarFrequency,
                        _ShootingStarSpeed,
                        _ShootingStarLength,
                        _ShootingStarBrightness,
                        _ShootingStarColor.rgb,
                        animTime);
                }

                // 5. Optional Distant Celestial Object
                half  celestialOcclusion = 0.0h;
                half3 celestialColor     = half3(0.0h, 0.0h, 0.0h);
                #if !defined(_QUALITY_LOW)
                    UNITY_BRANCH
                    if (_EnableCelestial > 0.5)
                    {
                        float3 bodyDir = PWSky_DirFromAzimuthElevation(_CelestialAzimuth, _CelestialElevation);
                        PWSky_CelestialResult bodyRes = PWSky_EvaluateDistantCelestial(
                            viewDir,
                            pixelAngle,
                            bodyDir,
                            _CelestialSize,
                            _CelestialColor.rgb,
                            _CelestialOpacity,
                            _CelestialRingIntensity,
                            _CelestialRingTilt);

                        celestialColor     = bodyRes.color;
                        celestialOcclusion = bodyRes.discMask;
                    }
                #endif

                // 6. Customizable Moon
                half  moonOcclusion = 0.0h;
                half3 moonColor     = half3(0.0h, 0.0h, 0.0h);
                UNITY_BRANCH
                if (_EnableMoon > 0.5)
                {
                    PWSky_CelestialResult moonRes = PWSky_EvaluateMoon(
                        viewDir,
                        pixelAngle,
                        moonDir,
                        _MoonSize,
                        _MoonColor.rgb,
                        _MoonBrightness,
                        TEXTURE2D_ARGS(_MoonTex, sampler_MoonTex),
                        _MoonSurfaceDetail,
                        _MoonPhase,
                        _MoonPhaseSoftness,
                        _MoonEarthshine,
                        _MoonGlowIntensity,
                        _MoonGlowRadius,
                        _MoonGlowColor.rgb);

                    moonColor     = moonRes.color;
                    moonOcclusion = moonRes.discMask;
                }

                // Composite Upper Sky -> Stars & Meteors -> Distant Celestial -> Moon
                half starVisibility = (1.0h - celestialOcclusion) * (1.0h - moonOcclusion);
                skyColor += (starColor + meteorColor) * starVisibility;
                skyColor += celestialColor * (1.0h - moonOcclusion);
                skyColor  = lerp(skyColor, half3(0.0h, 0.0h, 0.0h), moonOcclusion * 0.85h) + moonColor;

                // 7. Multi-Layer Animated Clouds
                UNITY_BRANCH
                if (_EnableClouds > 0.5)
                {
                    half4 cloudRes = PWSky_EvaluateClouds(
                        viewDir,
                        moonDir,
                        TEXTURE2D_ARGS(_NoiseTex, sampler_NoiseTex),
                        _CloudCoverage,
                        _CloudDensity,
                        _CloudSoftness,
                        _CloudOpacity,
                        _CloudLayer1Scale,
                        _CloudLayer1Speed.xy,
                        _CloudLayer2Scale,
                        _CloudLayer2Speed.xy,
                        _CloudBaseColor.rgb,
                        _CloudLitColor.rgb,
                        _CloudRimColor.rgb,
                        _CloudRimIntensity,
                        (half)_EnableMoon,
                        animTime);

                    skyColor = lerp(skyColor, cloudRes.rgb, cloudRes.a);
                }

                // 8. Lower Hemisphere (Solid Gradient / Void Mist / Fake Night Water)
                skyColor = PWSky_EvaluateLowerHemisphere(
                    viewDir,
                    moonDir,
                    skyColor,
                    _HorizonColor.rgb,
                    _GroundColor.rgb,
                    _HorizonHeight,
                    _LowerMode,
                    TEXTURE2D_ARGS(_NoiseTex, sampler_NoiseTex),
                    _VoidMistColor.rgb,
                    _VoidMistScale,
                    _VoidMistSpeed,
                    _VoidMistIntensity,
                    _WaterDeepColor.rgb,
                    _WaterReflectColor.rgb,
                    _WaterScale,
                    _WaterSpeed,
                    _WaterMoonShimmer,
                    _MoonGlowColor.rgb,
                    (half)_EnableMoon,
                    animTime);

                // 9. Exposure + Retro Pass OR Clean Anti-Banding Dither
                skyColor *= _Exposure;

                UNITY_BRANCH
                if (_RetroEnabled > 0.5)
                {
                    skyColor = PWSky_ApplyRetroColorPass(
                        skyColor,
                        input.positionCS.xy,
                        _RetroColorSteps,
                        _RetroDitherAmount,
                        _RetroDitherScale,
                        _RetroContrast);
                }
                else
                {
                    half dither = PWSky_Bayer4x4((uint2)input.positionCS.xy) * (_DitherStrength / 255.0h);
                    skyColor = max(half3(0.0h, 0.0h, 0.0h), skyColor + dither);
                }

                return half4(skyColor, 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}