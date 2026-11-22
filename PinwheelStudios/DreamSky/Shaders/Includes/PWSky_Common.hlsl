// ============================================================================
// Pinwheel DreamSky — Common Sky Math, Hashing, Gradients & Dithering
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_COMMON_INCLUDED
#define PWSKY_COMMON_INCLUDED

#define PWSKY_TWO_PI  6.28318530718
#define PWSKY_DEG2RAD 0.01745329252

// ----------------------------------------------------------------------------
// Deterministic High-Quality Hashes (No Sin-Artifacts Across GPUs)
// ----------------------------------------------------------------------------
float PWSky_Hash13(float3 p3)
{
    p3 = frac(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return frac((p3.x + p3.y) * p3.z);
}

float4 PWSky_Hash43(float3 p3)
{
    float4 p4 = frac(float4(p3.xyzx) * float4(0.1031, 0.1030, 0.0973, 0.1099));
    p4 += dot(p4, p4.wzxy + 33.33);
    return frac((p4.xxyz + p4.yzzw) * p4.zywx);
}

// ----------------------------------------------------------------------------
// Direction & Tangent-Basis Helpers
// ----------------------------------------------------------------------------
float3 PWSky_DirFromAzimuthElevation(float azimuthDeg, float elevationDeg)
{
    float az = azimuthDeg * PWSKY_DEG2RAD;
    float el = elevationDeg * PWSKY_DEG2RAD;
    float cosEl = cos(el);
    return normalize(float3(sin(az) * cosEl, sin(el), cos(az) * cosEl));
}

void PWSky_BuildBasis(float3 forwardDir, out float3 rightDir, out float3 upDir)
{
    float3 refUp = (abs(forwardDir.y) > 0.985) ? float3(1.0, 0.0, 0.0) : float3(0.0, 1.0, 0.0);
    rightDir = normalize(cross(refUp, forwardDir));
    upDir    = cross(forwardDir, rightDir);
}

// ----------------------------------------------------------------------------
// Virtual Cubemap Projection (Zero Pole Pinching for Procedural Stars)
// ----------------------------------------------------------------------------
void PWSky_DirToCubeFace(float3 dir, out float faceId, out float2 faceUV)
{
    float3 a = abs(dir);

    if (a.x >= a.y && a.x >= a.z)
    {
        float inv = rcp(max(a.x, 1e-5));
        if (dir.x >= 0.0)
        {
            faceId = 0.0;
            faceUV = float2(-dir.z, dir.y) * inv;
        }
        else
        {
            faceId = 1.0;
            faceUV = float2(dir.z, dir.y) * inv;
        }
    }
    else if (a.y >= a.x && a.y >= a.z)
    {
        float inv = rcp(max(a.y, 1e-5));
        if (dir.y >= 0.0)
        {
            faceId = 2.0;
            faceUV = float2(dir.x, -dir.z) * inv;
        }
        else
        {
            faceId = 3.0;
            faceUV = float2(dir.x, dir.z) * inv;
        }
    }
    else
    {
        float inv = rcp(max(a.z, 1e-5));
        if (dir.z >= 0.0)
        {
            faceId = 4.0;
            faceUV = float2(dir.x, dir.y) * inv;
        }
        else
        {
            faceId = 5.0;
            faceUV = float2(-dir.x, dir.y) * inv;
        }
    }
}

float3 PWSky_CubeFaceToDir(float faceId, float2 faceUV)
{
    if (faceId < 0.5) return normalize(float3( 1.0,       faceUV.y, -faceUV.x));
    if (faceId < 1.5) return normalize(float3(-1.0,       faceUV.y,  faceUV.x));
    if (faceId < 2.5) return normalize(float3( faceUV.x,  1.0,      -faceUV.y));
    if (faceId < 3.5) return normalize(float3( faceUV.x, -1.0,       faceUV.y));
    if (faceId < 4.5) return normalize(float3( faceUV.x,  faceUV.y,  1.0));
    return                   normalize(float3(-faceUV.x,  faceUV.y, -1.0));
}

// ----------------------------------------------------------------------------
// Base Night Sky Gradient & Subtle Environmental Colour Variation
// ----------------------------------------------------------------------------
half3 PWSky_EvaluateBaseGradient(
    float3 viewDir,
    half3 zenithColor,
    half3 midSkyColor,
    half3 horizonColor,
    half3 groundColor,
    half horizonHeight,
    half horizonFalloff,
    half atmosphereGlowIntensity,
    half3 atmosphereGlowColor,
    half envColorVariation,
    float time)
{
    half y = (half)viewDir.y;

    // Upper hemisphere: Horizon -> Mid Sky -> Zenith
    half upT = saturate((y - horizonHeight) / max(1.0h - horizonHeight, 0.05h));
    half midBlend = pow(upT, max(horizonFalloff, 0.1h));

    half3 lowerBand = lerp(horizonColor, midSkyColor, saturate(midBlend * 2.0h));
    half3 upperSky  = lerp(lowerBand, zenithColor, saturate((midBlend - 0.35h) / 0.65h));

    // Subtle azimuthal + slow temporal colour shift across the sky
    if (envColorVariation > 0.001h)
    {
        float wave = sin(viewDir.x * 2.3 + time * 0.06) * cos(viewDir.z * 1.9 - time * 0.045);
        half shift = (half)(wave * 0.5 + 0.5);
        half3 tint = lerp(midSkyColor, atmosphereGlowColor, shift);
        upperSky = lerp(upperSky, upperSky * 0.78h + tint * 0.38h, envColorVariation * saturate(1.0h - upT * 0.6h));
    }

    // Horizon atmospheric glow band
    half hBand = exp(-abs(y - horizonHeight) * (6.5h + horizonFalloff * 3.0h));
    upperSky += atmosphereGlowColor * (hBand * atmosphereGlowIntensity);

    // Lower hemisphere blend (Phase 3 expands this with Void Mist & Fake Water)
    half belowT = saturate((-y + horizonHeight) * 3.5h);
    half3 finalBg = lerp(upperSky, groundColor, smoothstep(0.0h, 1.0h, belowT));

    return finalBg;
}

// ----------------------------------------------------------------------------
// Ordered 4x4 Bayer Dithering (Eliminates 8-Bit Dark Sky Colour Banding)
// ----------------------------------------------------------------------------
half PWSky_Bayer4x4(uint2 pixelPos)
{
    static const half kBayer[16] =
    {
         0.0h / 16.0h,  8.0h / 16.0h,  2.0h / 16.0h, 10.0h / 16.0h,
        12.0h / 16.0h,  4.0h / 16.0h, 14.0h / 16.0h,  6.0h / 16.0h,
         3.0h / 16.0h, 11.0h / 16.0h,  1.0h / 16.0h,  9.0h / 16.0h,
        15.0h / 16.0h,  7.0h / 16.0h, 13.0h / 16.0h,  5.0h / 16.0h
    };
    uint idx = (pixelPos.x & 3u) + ((pixelPos.y & 3u) << 2u);
    return kBayer[idx] - 0.5h;
}

#endif // PWSKY_COMMON_INCLUDED