// ============================================================================
// Pinwheel DreamSky — Zero-Overhead Retro Stylization (Pixelation, Dither & Quantization)
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_RETRO_INCLUDED
#define PWSKY_RETRO_INCLUDED

#include "PWSky_Common.hlsl"

#ifndef PWSKY_PI
#define PWSKY_PI 3.14159265359
#endif

// ----------------------------------------------------------------------------
// World-Locked Equal-Area Spherical Pixelation (No Zenith Pinch, No Camera Smear)
// Snaps the sky view direction onto a uniform retro texel dome.
// ----------------------------------------------------------------------------
float3 PWSky_QuantizeViewDirection(float3 viewDir, float pixelResolution)
{
    if (pixelResolution <= 0.5)
        return viewDir;

    float rings  = max(pixelResolution, 16.0);
    float el     = asin(clamp(viewDir.y, -1.0, 1.0));
    float ringF  = (el / PWSKY_PI + 0.5) * rings;
    float ringId = clamp(floor(ringF), 0.0, rings - 1.0);

    float qEl    = ((ringId + 0.5) / rings - 0.5) * PWSKY_PI;
    float cosEl  = cos(qEl);
    float cols   = max(6.0, floor(2.0 * rings * max(cosEl, 0.02) + 0.5));

    float azNorm = atan2(viewDir.z, viewDir.x) / PWSKY_TWO_PI + 0.5;
    float colId  = clamp(floor(azNorm * cols), 0.0, cols - 1.0);
    float qAz    = ((colId + 0.5) / cols - 0.5) * PWSKY_TWO_PI;

    return float3(cos(qAz) * cosEl, sin(qEl), sin(qAz) * cosEl);
}

// ----------------------------------------------------------------------------
// Perceptual PS1/PS2 Ordered Bayer Dither + Color Posterization
// Operates in approximate gamma space (sqrt/square) so dark fantasy gradients
// band evenly while preserving HDR bloom peaks on the moon and hero stars.
// ----------------------------------------------------------------------------
half3 PWSky_ApplyRetroColorPass(
    half3 linearColor,
    float2 screenPosPx,
    half colorSteps,
    half ditherAmount,
    half ditherPixelScale,
    half contrast)
{
    // Optional retro contrast & subtle deep-shadow crush
    half3 col = max(half3(0.0h, 0.0h, 0.0h), linearColor);
    if (abs(contrast - 1.0h) > 0.001h)
    {
        col = pow(max(col, half3(1e-4h, 1e-4h, 1e-4h)), contrast);
    }

    // Perceptual space keeps dark indigo/violet bands looking authentic instead of crushed black
    half3 perceptual = sqrt(col);

    float pxScale  = max((float)ditherPixelScale, 1.0);
    uint2 ditherPx = (uint2)floor(screenPosPx / pxScale);
    half  bayer    = PWSky_Bayer4x4(ditherPx);

    half steps = max(colorSteps, 2.0h);

    // Classic ordered dithering right before quantization step
    perceptual += bayer * (ditherAmount / steps);
    perceptual  = floor(max(half3(0.0h, 0.0h, 0.0h), perceptual) * steps + 0.5h) / steps;

    return perceptual * perceptual;
}

#endif // PWSKY_RETRO_INCLUDED