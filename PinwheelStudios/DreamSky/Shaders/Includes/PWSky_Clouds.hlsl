// ============================================================================
// Pinwheel DreamSky — Multi-Layer Perspective Clouds & Moon Backlight System
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_CLOUDS_INCLUDED
#define PWSKY_CLOUDS_INCLUDED

#include "PWSky_Common.hlsl"

// Evaluates multi-layer curved-dome clouds with independent velocity and moonlit rim glow.
// Returns: rgb = shaded cloud colour, a = premultiplied/blending opacity
half4 PWSky_EvaluateClouds(
    float3 viewDir,
    float3 moonDir,
    TEXTURE2D_PARAM(noiseTex, sampler_noiseTex),
    half coverage,
    half density,
    half softness,
    half opacity,
    float layer1Scale,
    float2 layer1Speed,
    float layer2Scale,
    float2 layer2Speed,
    half3 baseColor,
    half3 litColor,
    half3 rimColor,
    half rimIntensity,
    half moonEnabled,
    float time)
{
    half horizonFade = smoothstep(-0.01h, 0.16h, (half)viewDir.y);
    if (horizonFade <= 0.001h || coverage <= 0.001h || opacity <= 0.001h)
        return half4(0.0h, 0.0h, 0.0h, 0.0h);

    // Curved-plane dome projection: natural horizon perspective & zero zenith pinching
    float domeY = max(viewDir.y, 0.0) + 0.26;
    float2 planeUV = viewDir.xz / domeY;

    // Analytic mip keeps far horizon clouds silky-smooth with zero aliasing
    float mip = clamp(log2(0.32 / domeY) * 1.1, 0.0, 4.5);

    float2 uv1 = planeUV * (layer1Scale * 0.14) + layer1Speed * (time * 0.012);
    half4  c1  = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, uv1, mip);

    half rawCloud;
    half wispDetail;
    half thickness;

    #if defined(_QUALITY_LOW)
        // Low Quality: 1 scrolling cloud layer
        rawCloud   = c1.r * 0.62h + c1.g * 0.38h;
        wispDetail = c1.b;
        thickness  = c1.r;
    #else
        // Medium & High: 2 independent cloud layers with distinct speed, direction & scale
        float2 uv2 = planeUV * (layer2Scale * 0.24) + layer2Speed * (time * 0.016);
        half4  c2  = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, uv2, mip);

        half layerA = c1.r * 0.58h + c1.g * 0.42h;
        half layerB = c2.g * 0.55h + c2.b * 0.45h;

        #if defined(_QUALITY_HIGH)
            // High Quality: 3rd fine tuft erosion tap for painterly backlit cloud silhouettes
            float2 uv3 = planeUV * (layer2Scale * 0.52) + (layer1Speed * 0.5 + layer2Speed * 0.5) * (time * 0.02);
            half4  c3  = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, uv3, mip);

            rawCloud   = saturate(layerA * 0.64h + layerB * 0.36h + (c3.g - 0.5h) * 0.16h);
            wispDetail = saturate(c2.b * 0.55h + c3.b * 0.45h);
            thickness  = saturate(layerA * 0.72h + c3.r * 0.28h);
        #else
            rawCloud   = saturate(layerA * 0.66h + layerB * 0.34h);
            wispDetail = c2.b;
            thickness  = layerA;
        #endif
    #endif

    // Coverage & softness shaping
    half threshold = lerp(0.86h, 0.16h, coverage);
    half edgeWidth = lerp(0.06h, 0.48h, softness);
    half shape     = smoothstep(threshold - edgeWidth * 0.5h, threshold + edgeWidth * 0.85h, rawCloud);

    if (shape <= 0.001h)
        return half4(0.0h, 0.0h, 0.0h, 0.0h);

    // Core cloud opacity (dense cores obscure stars/moon; thin wisps stay dreamy and translucent)
    half alpha = saturate(shape * density) * opacity * horizonFade;

    // Self-shading: thicker cloud bellies stay dark moody baseColor; thinner tops/edges catch light
    half edgeLit = saturate((1.0h - smoothstep(0.20h, 0.92h, thickness * shape)) * 0.75h + wispDetail * 0.25h);
    half3 col = lerp(baseColor, litColor, edgeLit);

    // Moon-driven backlit silver/violet rim glow (recreates dreamy dark-fantasy backlit clouds)
    if (moonEnabled > 0.5h && rimIntensity > 0.001h)
    {
        half moonDot   = saturate((half)dot(viewDir, moonDir));
        half broadGlow = pow(moonDot, 3.0h);
        half tightHalo = pow(moonDot, 14.0h);

        // Rim shines strongest on cloud borders (where shape is partial) and near the moon
        half rimEdge = saturate((1.0h - shape * 0.72h) * 1.15h + wispDetail * 0.35h);
        half rimFactor = (broadGlow * 0.55h + tightHalo * 1.45h) * rimEdge * rimIntensity;
        col += rimColor * rimFactor;
    }

    return half4(col, alpha);
}

#endif // PWSKY_CLOUDS_INCLUDED