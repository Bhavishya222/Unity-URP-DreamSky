// ============================================================================
// Pinwheel DreamSky — Moon (Texture, Phases, Glow) & Distant Celestial Objects
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_CELESTIAL_INCLUDED
#define PWSKY_CELESTIAL_INCLUDED

#include "PWSky_Common.hlsl"

struct PWSky_CelestialResult
{
    half3 color;     // Disc surface + atmospheric glow
    half  discMask;  // 1 inside solid disc (used to occlude stars behind moon/planet)
};

// ----------------------------------------------------------------------------
// Main Customizable Moon (Local Tangent-Plane Projection: Zero Sky Distortion)
// ----------------------------------------------------------------------------
PWSky_CelestialResult PWSky_EvaluateMoon(
    float3 viewDir,
    float pixelAngle,
    float3 moonDir,
    float moonSize,
    half3 moonColor,
    half moonBrightness,
    TEXTURE2D_PARAM(moonTex, sampler_moonTex),
    half surfaceDetail,
    float moonPhase,
    half phaseSoftness,
    half earthshine,
    half glowIntensity,
    float glowRadius,
    half3 glowColor)
{
    PWSky_CelestialResult res;
    res.color    = half3(0.0h, 0.0h, 0.0h);
    res.discMask = 0.0h;

    half horizonMask = smoothstep(-0.08h, 0.06h, (half)viewDir.y);
    if (horizonMask <= 0.001h)
        return res;

    float moonRad   = max(moonSize, 0.005);
    float chordDist = length(viewDir - moonDir);

    // 1. Analytic Soft Atmospheric Halo Glow (runs cleanly outside the disc too)
    if (glowIntensity > 0.001h)
    {
        float gRad      = max(glowRadius, 0.015);
        float edgeDist  = max(0.0, chordDist - moonRad * 0.92);
        float innerHalo = exp(-edgeDist / (gRad * 0.30));
        float outerHalo = exp(-chordDist / (gRad * 1.40)) * 0.42;

        // Slightly dim halo on a pure New Moon while keeping dreamy silhouette glow
        half phaseGlowScale = lerp(0.45h, 1.0h, (half)(0.5 + 0.5 * cos(moonPhase * PWSKY_TWO_PI)));
        res.color += glowColor * ((half)(innerHalo + outerHalo) * glowIntensity * phaseGlowScale * horizonMask);
    }

    // 2. Moon Disc (Strict Local Basis -> Scaling Moon Never Affects Rest of Sky)
    float cosAngle = dot(viewDir, moonDir);
    if (cosAngle > 0.0 && chordDist < moonRad * 1.35)
    {
        float3 moonRight, moonUp;
        PWSky_BuildBasis(moonDir, moonRight, moonUp);

        float2 localPos = float2(dot(viewDir, moonRight), dot(viewDir, moonUp)) / moonRad;
        float rSq = dot(localPos, localPos);
        float r   = sqrt(rSq);

        float aa = max((pixelAngle / moonRad) * 1.25, 0.012);
        half disc = (half)(1.0 - smoothstep(1.0 - aa, 1.0 + aa, r));

        if (disc > 0.001h)
        {
            // Spherical normal for realistic limb darkening and procedural moon phases
            float z = sqrt(max(0.0, 1.0 - min(rSq, 1.0)));
            float3 sphereN = float3(localPos.x, localPos.y, z);

            // Phase = 0.0 Full | 0.25 Last Quarter | 0.5 New Moon | 0.75 First Quarter
            float phaseAngle = moonPhase * PWSKY_TWO_PI;
            float3 sunLocal  = normalize(float3(sin(phaseAngle), 0.14 * sin(phaseAngle), cos(phaseAngle)));

            float ndl = dot(sphereN, sunLocal);
            float soft = max((float)phaseSoftness, 0.02);
            half phaseLit = (half)smoothstep(-soft, soft, ndl);
            half lighting = lerp(earthshine, 1.0h, phaseLit);

            // Subtle limb darkening so the disc reads as a glowing sphere
            half limb = lerp(0.80h, 1.0h, saturate((half)z * 1.35h));

            // Surface texture (Low preset skips texture read for maximum speed)
            half3 surface = half3(1.0h, 1.0h, 1.0h);
            #if !defined(_QUALITY_LOW)
                float2 moonUV = localPos * 0.5 + 0.5;
                half3 texCol = SAMPLE_TEXTURE2D(moonTex, sampler_moonTex, moonUV).rgb;
                surface = lerp(half3(1.0h, 1.0h, 1.0h), texCol, surfaceDetail);
            #endif

            // Subtle illuminated crescent rim boost (matches dreamy fantasy look)
            half rimBoost = pow(1.0h - (half)z, 3.0h) * phaseLit * 0.35h;

            half3 discColor = surface * moonColor * (moonBrightness * lighting * limb + rimBoost);

            res.discMask = disc * horizonMask;
            res.color    = lerp(res.color, res.color * 0.35h + discColor, res.discMask);
        }
    }

    return res;
}

// ----------------------------------------------------------------------------
// Optional Distant Celestial Object (Twin Moon or Faint Ringed Fantasy Planet)
// Zero Texture Samples — 100% Analytic
// ----------------------------------------------------------------------------
PWSky_CelestialResult PWSky_EvaluateDistantCelestial(
    float3 viewDir,
    float pixelAngle,
    float3 bodyDir,
    float bodySize,
    half3 bodyColor,
    half bodyOpacity,
    half ringIntensity,
    float ringTiltDeg)
{
    PWSky_CelestialResult res;
    res.color    = half3(0.0h, 0.0h, 0.0h);
    res.discMask = 0.0h;

    half horizonMask = smoothstep(-0.02h, 0.14h, (half)viewDir.y);
    if (horizonMask <= 0.001h || bodyOpacity <= 0.001h)
        return res;

    float rad = max(bodySize, 0.004);
    float cosAngle = dot(viewDir, bodyDir);
    if (cosAngle <= 0.0)
        return res;

    float chordDist = length(viewDir - bodyDir);
    if (chordDist > rad * 3.0)
        return res;

    float3 bRight, bUp;
    PWSky_BuildBasis(bodyDir, bRight, bUp);

    float2 localPos = float2(dot(viewDir, bRight), dot(viewDir, bUp)) / rad;
    float rSq = dot(localPos, localPos);
    float r   = sqrt(rSq);

    float aa = max((pixelAngle / rad) * 1.25, 0.02);
    half disc = (half)(1.0 - smoothstep(1.0 - aa, 1.0 + aa, r));

    // Planet body (softly crescent-lit with subtle atmospheric bands)
    half3 planetCol = half3(0.0h, 0.0h, 0.0h);
    if (disc > 0.001h)
    {
        float z = sqrt(max(0.0, 1.0 - min(rSq, 1.0)));
        float3 n = float3(localPos.x, localPos.y, z);
        half lit = saturate((half)dot(n, normalize(float3(0.65, 0.35, 0.65))));
        half bands = 0.88h + 0.12h * (half)sin(localPos.y * 14.0);
        planetCol = bodyColor * (lerp(0.18h, 1.0h, lit) * bands);
    }

    // Optional tilted planetary rings (front arc occludes planet, back arc passes behind)
    half ringMask = 0.0h;
    if (ringIntensity > 0.001h)
    {
        float tilt = ringTiltDeg * PWSKY_DEG2RAD;
        float s = sin(tilt);
        float c = cos(tilt);
        float2 rotP = float2(localPos.x * c - localPos.y * s, localPos.x * s + localPos.y * c);

        // Squashed ellipse for perspective ring view
        float2 ringP = float2(rotP.x, rotP.y * 3.6);
        float ringR  = length(ringP);

        half outerBand = smoothstep(1.30h, 1.48h, (half)ringR) * (1.0h - smoothstep(2.15h, 2.38h, (half)ringR));
        half gap       = 1.0h - 0.55h * exp(-pow(((half)ringR - 1.82h) * 16.0h, 2.0h));
        ringMask = outerBand * gap * ringIntensity;

        // Back half of the ring (rotP.y > 0) sits behind the planet sphere
        if (rotP.y > 0.0)
        {
            ringMask *= (1.0h - disc);
        }
    }

    half combinedMask = saturate(max(disc, ringMask)) * bodyOpacity * horizonMask;
    half3 ringCol = bodyColor * 1.15h;
    half3 finalCol = lerp(planetCol, ringCol, saturate(ringMask / max(disc + ringMask, 0.001h)));

    res.discMask = disc * bodyOpacity * horizonMask;
    res.color    = finalCol * combinedMask;
    return res;
}

#endif // PWSKY_CELESTIAL_INCLUDED