# Pinwheel DreamSky — Procedural Night Sky Suite for Unity URP

![Unity 6.3+](https://img.shields.io/badge/Unity-6.3%2B-black?logo=unity)
![URP 17](https://img.shields.io/badge/Pipeline-URP%2017-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

A lightweight, reusable procedural night-sky shader and workflow tool for **Unity 6.3+ (URP 17)**. Built for both **stylized / dark-fantasy** worlds and **realistic night** scenes, with an optional zero-overhead **PS1/PS2 Retro mode**, a built-in **Editor Texture Baker**, and an ultra-light **World-Edge Void Mist / Fake Water Plane** for hiding terrain borders and cliff bases.

Zero external dependencies. Zero HDRP requirements. Zero volumetric raymarching.

<!-- Replace these with actual captures inside a /Docs or /Media folder -->
<!-- ![Dreamy Fantasy & Retro Preview](Media/preview_banner.png) -->

---

## Features

* **Spherical Dome Procedural Starfield** — Zero-loop, zero-texture-read star layers with organic galactic clustering, dark voids, power-law magnitude variation, adjustable size & size variation, independent per-star twinkling, and rotated hero star flares.
* **Domain-Warped Organic Nebula** — Pinch-free upper-hemisphere projection driven by a single channel-packed RGBA noise atlas, supporting 3-stop colour palettes, filament accents, and slow drift animation.
* **Multi-Layer Animated Clouds** — Curved-plane perspective projection with independent dual-layer drift velocities and moon-driven backlit rim glow.
* **Customizable Moon & Distant Celestials** — Local tangent-plane projection (scaling the moon never distorts the sky), procedural spherical moon phases, earthshine, halo glow, custom texture support, plus an optional ringed distant planet.
* **Procedural Shooting Stars** — Time-sliced downward meteor streaks driven purely on `_Time.y` with zero C# `Update()` overhead.
* **Lower Hemisphere & World-Edge Blending** — Switch below the horizon between **Solid Gradient**, **Swirling Void Mist**, or **Fake Night Water** (with automatic moon shimmer reflection), paired with an optional 1-draw-call `Void & Water Plane` shader and 1-click URP Horizon Fog sync.
* **Toggleable PS1/PS2 Retro Mode** — World-locked equal-area sky texel pixelation, perceptual ordered 4×4 Bayer dithering, and colour quantization with zero GPU divergence when disabled.
* **Built-In Editor Texture Baker** — Generates seamless tileable 4-channel RGBA noise (`Perlin / Worley / Ridged / Warp`) and a procedural lunar crater texture directly inside Unity.
* **Studio Workflow Hub & Custom Inspector** — 9-section collapsible Material Inspector, 4 one-click style presets (*Dreamy Fantasy*, *Dark Gothic Retro*, *Realistic Night*, *Twilight Mist*), and an optional zero-GC `PWSky_Controller` for Directional Light moon syncing and smooth runtime preset crossfading.

---

## Quality Tiers

Designed to scale down cleanly to integrated and low-end GPUs without modifying your URP Pipeline Asset (strictly **3 shader variants** total):

| Tier | Star Layers | Nebula Sampling | Cloud Layers | Moon & Extras |
| :--- | :--- | :--- | :--- | :--- |
| **Low** | 1 clustered pinpoint layer | 1 tap (no warp) | 1 scrolling layer | Analytic disc + glow, no shooting stars |
| **Medium** | 3 layers + hero stars | 2 taps (domain warped) | 2 independent layers | Textured moon + phases, 1 meteor slot |
| **High** | 4 layers + Milky Way dust + flares | 3 taps (warped + fine filaments) | 2 layers + fine erosion + rim glow | Dual meteor slots + distant ringed planet |

---

## Installation

### Option 1: Unity Package (Recommended)
1. Download the latest `PinwheelDreamSky-v1.0.0.unitypackage` from the [**Releases**](https://github.com/Bhavishya222/Unity-URP-DreamSky/releases) page.
2. In Unity 6.3+ (URP), go to **Assets > Import Package > Custom Package...** and import the downloaded file.

### Option 2: Manual / Git Clone
Copy the `Assets/PinwheelStudios/DreamSky/` folder into your Unity project's `Assets/PinwheelStudios/` directory.

---

## Quick Start

1. Open the setup window via **`Tools > Pinwheel Studios > Pinwheel DreamSky`**.
2. Choose a **Style Preset** (`Dreamy Fantasy`, `Dark Gothic Retro`, `Realistic Night`, or `Twilight Mist`) and a **Quality Tier**.
3. Click **Create & Apply Skybox to Scene** — the tool automatically bakes the default seamless noise atlas and lunar texture on first use, creates your skybox material in `Assets/PinwheelStudios/Generated/DreamSky/`, and assigns it to Scene Lighting.
4. *(Optional)* Click **Sync Scene Fog to Skybox Horizon Colour** to blend distant terrain edges seamlessly into the horizon.
5. *(Optional)* Click **Spawn Synced Void / Water Plane** to place a single 2-triangle quad around cliff bases or floating islands. Enable **Depth Texture** on your URP Asset for soft cliff-intersection mist, or uncheck *Soft Cliff Intersection* on the plane material for zero depth-pass cost.
6. *(Optional)* Click **Create / Select Sky Controller** to link a scene Directional Light to the Skybox Moon.

---

## Package Structure

```text
Assets/PinwheelStudios/DreamSky/
├── Runtime/
│   ├── PinwheelStudios.DreamSky.Runtime.asmdef
│   ├── PWSky_Controller.cs
│   └── PWSky_PresetEnums.cs
├── Editor/
│   ├── PinwheelStudios.DreamSky.Editor.asmdef
│   ├── PWSky_EditorStyle.cs
│   ├── PWSky_TextureBaker.cs
│   ├── PWSky_ShaderGUI.cs
│   └── PWSky_ToolWindow.cs
└── Shaders/
    ├── PWSky_DreamSkybox.shader
    ├── PWSky_VoidWaterPlane.shader
    └── Includes/
        ├── PWSky_Common.hlsl
        ├── PWSky_Stars.hlsl
        ├── PWSky_Nebula.hlsl
        ├── PWSky_Clouds.hlsl
        ├── PWSky_Celestial.hlsl
        ├── PWSky_ShootingStars.hlsl
        ├── PWSky_LowerHemisphere.hlsl
        └── PWSky_Retro.hlsl
