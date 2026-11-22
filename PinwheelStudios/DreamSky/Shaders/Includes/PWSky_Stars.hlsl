// ============================================================================
// Pinwheel DreamSky — Clustered & Magnitude-Varied Spherical Starfield
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_STARS_INCLUDED
#define PWSKY_STARS_INCLUDED

#include "PWSky_Common.hlsl"

#define PWSKY_PI 3.14159265359

half3 PWSky_StarTintFromHash(float h, half variation)
{
    half3 hotBlue    = half3(0.64h, 0.82h, 1.00h);
    half3 blueWhite  = half3(0.86h, 0.93h, 1.00h);
    half3 pureWhite  = half3(1.00h, 0.99h, 0.96h);
    half3 warmGold   = half3(1.00h, 0.84h, 0.60h);
    half3 amberRose  = half3(1.00h, 0.74h, 0.78h);

    half3 tint;
    if (h < 0.22)
        tint = lerp(hotBlue, blueWhite, (half)(h / 0.22));
    else if (h < 0.55)
        tint = lerp(blueWhite, pureWhite, (half)((h - 0.22) / 0.33));
    else if (h < 0.84)
        tint = lerp(pureWhite, warmGold, (half)((h - 0.55) / 0.29));
    else
        tint = lerp(warmGold, amberRose, (half)((h - 0.84) / 0.16));

    return lerp(half3(1.0h, 1.0h, 1.0h), tint, variation);
}

float3 PWSky_TiltDirection(float3 d, float sinX, float cosX, float sinZ, float cosZ)
{
    float3 r1 = float3(d.x, d.y * cosX - d.z * sinX, d.y * sinX + d.z * cosX);
    return float3(r1.x * cosZ - r1.y * sinZ, r1.x * sinZ + r1.y * cosZ, r1.z);
}

// Analytic macro noise (~12 ALU ops, zero textures): galactic river + star clusters + dark voids
float PWSky_EvaluateStarClusterMask(float3 dir)
{
    float galLat  = abs(dot(dir, float3(0.415, 0.578, -0.703)));
    float galBand = exp(-galLat * galLat * 6.0);

    float3 p1 = dir * 3.4;
    float n1  = sin(p1.x + cos(p1.y * 1.4 + p1.z * 0.9)) * cos(p1.z * 1.25 - sin(p1.x * 0.85 + p1.y));

    float3 p2 = dir * 8.2 + float3(2.3, -1.7, 3.1);
    float n2  = sin(p2.y + sin(p2.z * 1.15 - p2.x)) * cos(p2.x * 1.35 + cos(p2.y * 0.85));

    float clump = saturate(0.48 + 0.38 * n1 + 0.24 * n2);
    return 0.10 + clump * clump * 1.15 + galBand * (0.35 + 0.75 * clump);
}

half3 PWSky_EvaluateSphericalStarLayer(
    float3 layerDir,
    float pixelAngle,
    float ringCount,
    float localDensity,
    float baseRadius,
    half sizeVariation,
    half brightness,
    float twinkleSpeed,
    half twinkleIntensity,
    half colorVariation,
    half haloStrength,
    half flareIntensity,
    float jitterMargin,
    float magPower,
    float time,
    float layerSeed)
{
    float el     = asin(clamp(layerDir.y, -1.0, 1.0));
    float ringF  = (el / PWSKY_PI + 0.5) * ringCount;
    float ringId = clamp(floor(ringF), 0.0, ringCount - 1.0);

    float cellHeight = PWSKY_PI / ringCount;
    float elMin  = (ringId / ringCount - 0.5) * PWSKY_PI;
    float elMax  = ((ringId + 1.0) / ringCount - 0.5) * PWSKY_PI;
    float elMid  = 0.5 * (elMin + elMax);

    bool isPoleCap = (ringId < 0.5) || (ringId > ringCount - 1.5);
    float cols     = isPoleCap ? 1.0 : max(6.0, floor(2.0 * ringCount * cos(elMid) + 0.5));

    float azNorm = atan2(layerDir.z, layerDir.x) / PWSKY_TWO_PI + 0.5;
    float colId  = isPoleCap ? 0.0 : clamp(floor(azNorm * cols), 0.0, cols - 1.0);

    float4 h = PWSky_Hash43(float3(colId, ringId, layerSeed));

    if (h.x > saturate(localDensity))
        return half3(0.0h, 0.0h, 0.0h);

    // Adaptive margin: tight pairs when stars are small, generous room when Star Size is cranked up
    float desiredMargin  = max(jitterMargin, (baseRadius * 2.4) / max(cellHeight, 1e-5));
    float adaptiveMargin = clamp(desiredMargin, jitterMargin, 0.36);

    float jEl = lerp(adaptiveMargin, 1.0 - adaptiveMargin, h.y);
    float jAz = lerp(adaptiveMargin, 1.0 - adaptiveMargin, h.z);

    float starEl     = lerp(elMin, elMax, jEl);
    float starAzNorm = isPoleCap ? h.z : ((colId + jAz) / cols);
    float starAz     = (starAzNorm - 0.5) * PWSKY_TWO_PI;

    float cosStarEl  = cos(starEl);
    float3 starDir   = float3(cos(starAz) * cosStarEl, sin(starEl), sin(starAz) * cosStarEl);

    // Strict circular window inside cell guarantees zero box clipping at any star size
    float minCos        = min(cos(abs(elMin)), cos(abs(elMax)));
    float minCellWidth  = isPoleCap ? cellHeight : ((PWSKY_TWO_PI / cols) * max(minCos, 0.05));
    float maxSafeRadius = min(cellHeight, minCellWidth) * (adaptiveMargin * 0.94);

    float angDist = length(layerDir - starDir);
    if (angDist >= maxSafeRadius)
        return half3(0.0h, 0.0h, 0.0h);

    float radialT      = saturate(1.0 - angDist / max(maxSafeRadius, 1e-5));
    float circularMask = radialT * radialT * (3.0 - 2.0 * radialT);

    // Astronomical power-law brightness magnitude
    float magRand = frac(h.x * 73.17 + h.w * 11.73);
    float mag     = pow(magRand, magPower);

    // Controllable per-star size variation (0 = uniform, 1 = dramatic pinpoints to big jewels)
    float sizeNoise        = frac(h.w * 17.31 + h.y * 53.7);
    float sizeCurve        = pow(sizeNoise, 1.85);
    float combinedSizeRand = lerp(sizeCurve, sqrt(mag), 0.35);
    float variedScale      = lerp(0.22, 2.35, combinedSizeRand);
    float sizeMult         = lerp(1.0, variedScale, (float)sizeVariation);

    float radius         = min(baseRadius * sizeMult, maxSafeRadius * 0.54);
    float renderedRadius = max(radius, pixelAngle * 0.72);
    float areaScale      = lerp(saturate(radius / renderedRadius), 1.0, 0.25);

    float coreRatio = angDist / max(renderedRadius, 1e-5);
    float core      = exp(-coreRatio * coreRatio * 2.35) * areaScale * circularMask;

    // Soft halo scales smoothly with star size & magnitude
    float halo = 0.0;
    if (haloStrength > 0.001h && mag > 0.14)
    {
        float haloRadius = max(renderedRadius * 2.2, pixelAngle * 1.4);
        halo = exp(-angDist / haloRadius) * (float)haloStrength * (mag * mag) * circularMask;
    }

    // Optional rotated 4-point cross flare (Hero stars)
    float flare = 0.0;
    if (flareIntensity > 0.001h && mag > 0.22)
    {
        float3 sRight, sUp;
        PWSky_BuildBasis(starDir, sRight, sUp);
        float3 delta = layerDir - starDir;

        float u = dot(delta, sRight) / max(maxSafeRadius, 1e-5);
        float v = dot(delta, sUp)    / max(maxSafeRadius, 1e-5);

        float rot = (h.y - 0.5) * 0.65;
        float sr, cr;
        sincos(rot, sr, cr);
        float dx = abs(u * cr - v * sr);
        float dy = abs(u * sr + v * cr);

        float spikeH = exp(-dx * 4.6) * exp(-dy * 54.0);
        float spikeV = exp(-dy * 4.6) * exp(-dx * 54.0);
        flare = (spikeH + spikeV) * (float)flareIntensity * mag * circularMask;
    }

    float shape = core + halo + flare;
    if (shape < 0.001)
        return half3(0.0h, 0.0h, 0.0h);

    // Independent organic twinkling
    float tPhase = h.w * PWSKY_TWO_PI;
    float tSpeed = twinkleSpeed * lerp(0.5, 1.85, frac(h.y * 37.13));
    float tWave  = 0.5 + 0.5 * sin(time * tSpeed + tPhase) * cos(time * tSpeed * 0.41 + tPhase * 1.37);
    half twinkle = lerp(1.0h, lerp(0.14h, 1.0h, (half)tWave), twinkleIntensity);

    half lumVar = lerp(0.14h, 2.35h, (half)mag);
    half3 tint  = PWSky_StarTintFromHash(frac(h.z * 43.91 + h.x * 7.13), colorVariation);

    half coreWhiten = saturate((half)(core * mag * 0.55));
    half3 finalTint = lerp(tint, half3(1.0h, 1.0h, 1.0h), coreWhiten);

    return finalTint * ((half)shape * brightness * lumVar * twinkle);
}

half3 PWSky_EvaluateStarfield(
    float3 viewDir,
    float pixelAngle,
    float density,
    float starSize,
    half starSizeVariation,
    half brightness,
    float twinkleSpeed,
    half twinkleIntensity,
    half colorVariation,
    half heroAmount,
    half heroFlareIntensity,
    half3 starTint,
    float time)
{
    half horizonFade = smoothstep(-0.02h, 0.16h, (half)viewDir.y);
    if (horizonFade <= 0.001h)
        return half3(0.0h, 0.0h, 0.0h);

    float cluster = PWSky_EvaluateStarClusterMask(viewDir);
    float sv      = (float)starSizeVariation;

    half3 stars = half3(0.0h, 0.0h, 0.0h);

    #if defined(_QUALITY_LOW)
        // Low Quality: Single crisp clustered starfield
        stars += PWSky_EvaluateSphericalStarLayer(
            viewDir, pixelAngle,
            76.0, density * 0.46 * cluster, starSize * 0.0020, starSizeVariation, brightness * 1.15h,
            twinkleSpeed, twinkleIntensity, colorVariation,
            0.22h, 0.0h, 0.14, 2.5, time, 1.0);
    #else
        // Layer 1: Fine background stars (higher variation keeps these delicate while big stars pop)
        float3 dirFine = PWSky_TiltDirection(viewDir, 0.309017, 0.951057, 0.207912, 0.978148);
        float fineRad  = starSize * lerp(0.0020, 0.00145, sv);
        stars += PWSky_EvaluateSphericalStarLayer(
            dirFine, pixelAngle,
            92.0, density * 0.45 * cluster, fineRad, starSizeVariation, brightness * 0.95h,
            twinkleSpeed * 1.15, twinkleIntensity * 0.85h, colorVariation * 0.85h,
            0.12h, 0.0h, 0.12, 2.7, time, 1.0);

        // Layer 2: Mid-field constellation stars
        float3 dirMid = PWSky_TiltDirection(viewDir, -0.258819, 0.965926, -0.422618, 0.906308);
        float midRad  = starSize * lerp(0.0023, 0.0029, sv);
        stars += PWSky_EvaluateSphericalStarLayer(
            dirMid, pixelAngle,
            42.0, density * 0.22 * lerp(0.35, 1.35, cluster), midRad, starSizeVariation, brightness * 1.35h,
            twinkleSpeed * 0.9, twinkleIntensity, colorVariation,
            0.35h, 0.0h, 0.16, 2.3, time, 9.0);

        // Layer 3: Rare bright Hero stars (scales up dramatically with Star Size & Variation)
        if (heroAmount > 0.001h)
        {
            #if defined(_QUALITY_HIGH)
                half flare = heroFlareIntensity;
            #else
                half flare = heroFlareIntensity * 0.6h;
            #endif

            float3 dirHero = PWSky_TiltDirection(viewDir, -0.529919, 0.848048, 0.453990, 0.891007);
            float heroRad  = starSize * lerp(0.0034, 0.0056, sv);
            stars += PWSky_EvaluateSphericalStarLayer(
                dirHero, pixelAngle,
                16.0, density * 0.14 * (float)heroAmount * lerp(0.45, 1.4, cluster), heroRad, starSizeVariation, brightness * 1.85h,
                twinkleSpeed * 0.75, twinkleIntensity * 0.8h, colorVariation,
                0.55h, flare, 0.22, 1.8, time, 17.0);
        }
    #endif

    #if defined(_QUALITY_HIGH)
        // Layer 4: Ultra-fine glittering Milky Way dust concentrated inside clusters
        float3 dirDust = PWSky_TiltDirection(viewDir, 0.615661, 0.788011, -0.374607, 0.927184);
        float dustDensity = density * 0.62 * saturate((cluster - 0.30) * 1.15);
        float dustRad     = starSize * lerp(0.0014, 0.00095, sv);
        stars += PWSky_EvaluateSphericalStarLayer(
            dirDust, pixelAngle,
            165.0, dustDensity, dustRad, starSizeVariation * 0.75h, brightness * 0.62h,
            twinkleSpeed * 1.35, twinkleIntensity * 0.65h, colorVariation * 0.65h,
            0.0h, 0.0h, 0.11, 2.2, time, 37.0);
    #endif

    return stars * starTint * horizonFade;
}

#endif // PWSKY_STARS_INCLUDED