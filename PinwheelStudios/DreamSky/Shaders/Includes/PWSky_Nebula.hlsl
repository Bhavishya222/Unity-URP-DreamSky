// ============================================================================
// Pinwheel DreamSky — Domain-Warped Procedural Nebula System
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_NEBULA_INCLUDED
#define PWSKY_NEBULA_INCLUDED

#include "PWSky_Common.hlsl"

// Evaluates organic, pinch-free dome nebulae using the baked 4-channel RGBA noise atlas:
//   R = Smooth Perlin fBm | G = Billowy Worley | B = Wispy Ridged Filaments | A = Warp Driver
// Returns: rgb = additive nebula colour, a = soft density mask (for depth blending)
half4 PWSky_EvaluateNebula(
    float3 viewDir,
    TEXTURE2D_PARAM(noiseTex, sampler_noiseTex),
    float scale,
    float distortion,
    float driftSpeed,
    half intensity,
    half opacity,
    half colorBlend,
    half3 colorPrimary,
    half3 colorSecondary,
    half3 colorAccent,
    float time)
{
    half horizonFade = smoothstep(-0.08h, 0.16h, (half)viewDir.y);
    if (horizonFade <= 0.001h || intensity <= 0.001h || opacity <= 0.001h)
        return half4(0.0h, 0.0h, 0.0h, 0.0h);

    // Seamless upper-hemisphere paraboloid projection: zero zenith pinching, zero seams
    float2 domeUV = viewDir.xz / (1.0 + max(viewDir.y, -0.35));

    // Gentle diagonal galactic sweep mask so nebula feels organic rather than uniform wallpaper
    float galPlane = abs(dot(viewDir, float3(0.36, 0.54, -0.76)));
    half  galMask  = lerp(0.55h, 1.25h, (half)exp(-galPlane * galPlane * 2.4));

    float2 drift1 = float2(time * driftSpeed * 0.0085, time * driftSpeed * 0.0045);
    float2 baseUV = domeUV * (scale * 0.42) + drift1;

    // Analytic mip avoids horizon shimmer while keeping early-out completely safe
    float mip = clamp((1.0 - saturate(viewDir.y)) * 1.4, 0.0, 3.0);

    half body;
    half clumps;
    half filaments;
    half colorMix;

    #if defined(_QUALITY_LOW)
        // Low Quality: 1 texture tap (fast, zero warp overhead)
        half4 n0 = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, baseUV, mip);
        body      = smoothstep(0.28h, 0.85h, n0.r);
        clumps    = n0.g;
        filaments = pow(saturate(n0.b), 2.0h) * body;
        colorMix  = n0.g;
    #else
        // Medium & High: 2-tap domain warping for painterly swirling nebula bands
        float2 warpUV = domeUV * (scale * 0.24) - drift1 * 0.65;
        half4  wSample = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, warpUV, mip + 0.5);
        float2 warpVec = (float2(wSample.a, wSample.r) - 0.5) * (distortion * 0.55);

        half4 nMain = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, baseUV + warpVec, mip);

        #if defined(_QUALITY_HIGH)
            // High Quality: 3rd counter-drifting fine detail tap for glowing filaments & dust lanes
            float2 detailUV = (domeUV * (scale * 0.88)) + warpVec * 1.45 - float2(drift1.y, -drift1.x) * 1.3;
            half4  nDetail  = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, detailUV, mip);

            half rawShape = saturate(nMain.r * 0.62h + nMain.g * 0.38h);
            half dustCarve = lerp(0.55h, 1.15h, nDetail.r);
            body      = smoothstep(0.24h, 0.84h, rawShape * dustCarve);
            clumps    = saturate(nMain.g * 0.65h + nDetail.g * 0.35h);
            filaments = pow(saturate(nMain.b * 0.55h + nDetail.b * 0.65h), 2.2h) * smoothstep(0.18h, 0.72h, rawShape);
            colorMix  = saturate(wSample.g * 0.55h + nDetail.g * 0.45h);
        #else
            // Medium Quality: Crisp swirling clouds + electric filament cores
            half rawShape = saturate(nMain.r * 0.65h + nMain.g * 0.35h);
            body      = smoothstep(0.26h, 0.84h, rawShape);
            clumps    = nMain.g;
            filaments = pow(saturate(nMain.b), 2.1h) * smoothstep(0.20h, 0.75h, rawShape);
            colorMix  = saturate(wSample.g * 0.6h + nMain.g * 0.4h);
        #endif
    #endif

    // Multi-stop organic palette blending (Primary -> Secondary -> Electric Accent)
    half blendT = saturate(lerp(clumps, colorMix, colorBlend));
    half3 nebulaCol = lerp(colorPrimary, colorSecondary, smoothstep(0.15h, 0.85h, blendT));

    // Bright electric accent streaks along ridged filament veins
    half accentMask = saturate(filaments * (0.65h + colorBlend * 0.75h));
    nebulaCol = lerp(nebulaCol, colorAccent, saturate(accentMask * 0.85h));

    // Luminous filament boost + subtle core contrast
    half density = saturate(body * galMask);
    half3 finalRGB = nebulaCol * (density * 0.75h + filaments * 1.35h) * (intensity * opacity * horizonFade);

    return half4(finalRGB, saturate(density * opacity * horizonFade));
}

#endif // PWSKY_NEBULA_INCLUDED