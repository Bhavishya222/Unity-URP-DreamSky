// ============================================================================
// Pinwheel DreamSky — Lower Hemisphere (Solid / Void Mist / Fake Night Water)
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_LOWER_HEMISPHERE_INCLUDED
#define PWSKY_LOWER_HEMISPHERE_INCLUDED

#include "PWSky_Common.hlsl"

half3 PWSky_EvaluateLowerHemisphere(
    float3 viewDir,
    float3 moonDir,
    half3 upperSkyColor,
    half3 horizonColor,
    half3 groundColor,
    half horizonHeight,
    float lowerMode,
    TEXTURE2D_PARAM(noiseTex, sampler_noiseTex),
    half3 voidMistColor,
    float voidMistScale,
    float voidMistSpeed,
    half voidMistIntensity,
    half3 waterDeepColor,
    half3 waterReflectColor,
    float waterScale,
    float waterSpeed,
    half waterMoonShimmer,
    half3 moonGlowColor,
    half moonEnabled,
    float time)
{
    // Mode 0: Solid Gradient (handled by base sky gradient — zero extra work)
    if (lowerMode < 0.5)
        return upperSkyColor;

    float yRel = viewDir.y - (float)horizonHeight;

    // Fast early exit: upper-hemisphere pixels pay zero GPU cost
    if (yRel >= 0.06)
        return upperSkyColor;

    half belowMask = (half)smoothstep(0.04, -0.025, yRel);
    float downDepth = max(-yRel, 0.0);

    // ------------------------------------------------------------------------
    // Mode 1: Swirling Dark-Fantasy Void Mist (Infinite Abyss Below Cliffs)
    // ------------------------------------------------------------------------
    if (lowerMode < 1.5)
    {
        // Seamless lower-hemisphere paraboloid UV (zero nadir pinching)
        float2 voidUV = viewDir.xz / (1.0 + downDepth * 1.35);
        float  mip    = clamp((1.0 - saturate(downDepth * 1.5)) * 1.8, 0.0, 3.5);
        float2 drift  = float2(time * voidMistSpeed * 0.018, -time * voidMistSpeed * 0.012);

        half mistPattern;
        half mistWisps;

        #if defined(_QUALITY_LOW)
            half4 n0 = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, voidUV * voidMistScale + drift, mip);
            mistPattern = n0.r * 0.65h + n0.g * 0.35h;
            mistWisps   = n0.b;
        #else
            half4 w0 = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, voidUV * (voidMistScale * 0.55) - drift * 0.6, mip + 0.5);
            float2 warp = (float2(w0.a, w0.r) - 0.5) * 0.42;
            half4 n1 = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, voidUV * voidMistScale + warp + drift, mip);
            mistPattern = saturate(n1.r * 0.55h + n1.g * 0.45h);
            mistWisps   = n1.b;
        #endif

        // Glowing swirling mist brightens near the horizon ledge and deepens into dark void below
        half abyssDepth = smoothstep(0.0h, 0.85h, (half)downDepth);
        half mistMask   = smoothstep(0.22h, 0.86h, mistPattern) * voidMistIntensity;
        half3 voidCol   = lerp(groundColor, voidMistColor, saturate(mistMask * lerp(1.0h, 0.45h, abyssDepth)));
        voidCol        += voidMistColor * (pow(mistWisps, 2.2h) * 0.35h * voidMistIntensity * (1.0h - abyssDepth * 0.65h));

        // Seamless horizon haze stitch
        half horizonHaze = exp(-(half)downDepth * 6.5h);
        voidCol = lerp(voidCol, lerp(horizonColor, upperSkyColor, 0.5h), horizonHaze * 0.72h);

        return lerp(upperSkyColor, voidCol, belowMask);
    }

    // ------------------------------------------------------------------------
    // Mode 2: Ultra-Light Perspective Fake Night Water + Moon Shimmer Ribbon
    // ------------------------------------------------------------------------
    float downY = max(-yRel, 0.014);
    float2 waterUV = (viewDir.xz / downY) * (waterScale * 0.065);

    // Analytic distance mip eliminates far-horizon aliasing completely
    float mip = clamp(log2(0.085 / downY) * 1.15, 0.0, 5.5);
    half  horizonDist = (half)exp(-downY * 9.0);

    float2 scroll1 = float2(time * waterSpeed * 0.032, time * waterSpeed * 0.021);
    half4  r1      = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, waterUV + scroll1, mip);

    half2 rippleGrad;
    half  rippleHeight;

    #if defined(_QUALITY_LOW)
        rippleHeight = r1.g;
        rippleGrad   = (half2(r1.r, r1.b) - 0.5h) * 0.22h;
    #else
        float2 scroll2 = float2(-time * waterSpeed * 0.026, time * waterSpeed * 0.038);
        half4  r2      = SAMPLE_TEXTURE2D_LOD(noiseTex, sampler_noiseTex, waterUV * 1.42 + scroll2, mip);
        rippleHeight   = (r1.g + r2.b) * 0.5h;
        rippleGrad     = (half2(r1.r - r2.r, r1.g - r2.g)) * 0.28h;
    #endif

    // Dampen ripple normal noise right at the infinite horizon
    rippleGrad *= (1.0h - horizonDist * 0.85h);

    // Fresnel-like night ocean colour (deep abyss below, reflected sky toward horizon)
    half fresnel = pow(saturate(1.0h - (half)downY), 3.2h);
    half3 skyRefl = lerp(waterReflectColor, horizonColor, 0.45h);
    half3 waterCol = lerp(waterDeepColor, skyRefl, saturate(fresnel * 0.85h + rippleHeight * 0.14h));

    // Subtle glowing wave crest tint
    waterCol += waterReflectColor * (smoothstep(0.62h, 0.92h, rippleHeight) * 0.14h * (1.0h - horizonDist));

    // Fake Moon Reflection Path (automatically tracks Moon Azimuth & Elevation!)
    if (moonEnabled > 0.5h && waterMoonShimmer > 0.001h && moonDir.y > -0.06)
    {
        float3 flatRefl = float3(viewDir.x, max(-viewDir.y, 0.002), viewDir.z);
        float3 pertRefl = normalize(flatRefl + float3((float)rippleGrad.x, 0.0, (float)rippleGrad.y));

        half moonAlign = saturate((half)dot(pertRefl, moonDir));
        half broadPath = pow(moonAlign, 18.0h) * 0.48h;
        half sparkles  = pow(moonAlign, 130.0h) * (1.5h + rippleHeight * 1.8h);

        half3 shimmerTint = lerp(moonGlowColor, half3(1.0h, 1.0h, 1.0h), 0.45h);
        waterCol += shimmerTint * ((broadPath + sparkles) * waterMoonShimmer * (1.0h - horizonDist * 0.35h));
    }

    // Stitch cleanly into the upper horizon so the player never sees a sharp cut
    waterCol = lerp(waterCol, upperSkyColor, horizonDist * 0.78h);

    return lerp(upperSkyColor, waterCol, belowMask);
}

#endif // PWSKY_LOWER_HEMISPHERE_INCLUDED