// ============================================================================
// Pinwheel DreamSky — Procedural Time-Sliced Shooting Star System
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

#ifndef PWSKY_SHOOTING_STARS_INCLUDED
#define PWSKY_SHOOTING_STARS_INCLUDED

#include "PWSky_Common.hlsl"

half3 PWSky_EvaluateSingleMeteor(
    float3 viewDir,
    float pixelAngle,
    float frequency,
    float speed,
    float lengthScale,
    half brightness,
    half3 color,
    float time,
    float slotSeed)
{
    float slotTime  = time * max(frequency, 0.02) * 0.18 + slotSeed * 19.37;
    float cycleId   = floor(slotTime);
    float cycleFrac = frac(slotTime);

    float4 h = PWSky_Hash43(float3(cycleId, slotSeed, 41.7));

    // Meteor only flashes briefly inside each cycle window -> rare, organic sightings
    float activeDur = lerp(0.08, 0.16, h.x);
    float startFrac = lerp(0.06, 0.78, h.y);
    float life      = (cycleFrac - startFrac) / activeDur;

    if (life <= 0.0 || life >= 1.0)
        return half3(0.0h, 0.0h, 0.0h);

    // Random anchor in upper sky dome (elevation ~18 to 72 deg)
    float az = h.z * PWSKY_TWO_PI;
    float el = lerp(0.32, 1.25, h.w);
    float cosEl = cos(el);
    float3 anchorDir = float3(cos(az) * cosEl, sin(el), sin(az) * cosEl);

    // Fast hemisphere cone rejection (skips ~85% of pixels immediately)
    if (dot(viewDir, anchorDir) < 0.72)
        return half3(0.0h, 0.0h, 0.0h);

    float3 mRight, mUp;
    PWSky_BuildBasis(anchorDir, mRight, mUp);

    float4 h2 = PWSky_Hash43(float3(cycleId, slotSeed + 7.1, 89.3));
    float angle = lerp(-2.65, -0.48, h2.x); // Natural downward-angled streaks
    float2 travelDir = float2(cos(angle), sin(angle));
    float2 perpDir   = float2(-travelDir.y, travelDir.x);

    float3 delta  = viewDir - anchorDir;
    float2 localP = float2(dot(delta, mRight), dot(delta, mUp));

    float along  = dot(localP, travelDir);
    float across = abs(dot(localP, perpDir));

    float travelSpan = 0.24 * max(speed, 0.2);
    float headPos    = (life - 0.5) * travelSpan;
    float behind     = headPos - along;
    float tailLen    = max(lengthScale * 0.14, 0.025);

    if (behind < -0.015 || behind > tailLen)
        return half3(0.0h, 0.0h, 0.0h);

    float tailT = saturate(1.0 - behind / tailLen);

    // Tapered glowing streak + sparkling white-hot head
    float width    = max(pixelAngle * 1.1, lerp(0.0004, 0.0022, tailT * tailT));
    float lineMask = exp(-pow(across / width, 2.0));
    float tailFade = smoothstep(-0.006, 0.003, behind) * pow(tailT, 2.2);

    float headDist = length(float2(behind, across));
    float headGlow = exp(-headDist / max(pixelAngle * 2.6, 0.0045)) * 1.35;

    // Smooth fade in & fade out across meteor lifespan
    half env = (half)sin(life * 3.14159265);
    half3 coreTint = lerp(color, half3(1.0h, 1.0h, 1.0h), saturate((half)headGlow * 0.65h));

    return coreTint * ((half)(lineMask * tailFade + headGlow) * env * brightness);
}

half3 PWSky_EvaluateShootingStars(
    float3 viewDir,
    float pixelAngle,
    float frequency,
    float speed,
    float lengthScale,
    half brightness,
    half3 color,
    float time)
{
    #if defined(_QUALITY_LOW)
        return half3(0.0h, 0.0h, 0.0h);
    #else
        half horizonMask = smoothstep(0.02h, 0.20h, (half)viewDir.y);
        if (horizonMask <= 0.001h || brightness <= 0.001h || frequency <= 0.001)
            return half3(0.0h, 0.0h, 0.0h);

        half3 meteors = PWSky_EvaluateSingleMeteor(
            viewDir, pixelAngle, frequency, speed, lengthScale, brightness, color, time, 1.0);

        #if defined(_QUALITY_HIGH)
            meteors += PWSky_EvaluateSingleMeteor(
                viewDir, pixelAngle, frequency * 0.78, speed * 1.15, lengthScale * 0.85,
                brightness * 0.85h, color, time, 5.0);
        #endif

        return meteors * horizonMask;
    #endif
}

#endif // PWSKY_SHOOTING_STARS_INCLUDED