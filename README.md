# Pinwheel DreamSky — Procedural Night Sky Suite for Unity URP

![Unity 6.3+](https://img.shields.io/badge/Unity-6.3%2B-black?logo=unity)
![URP 17](https://img.shields.io/badge/Pipeline-URP%2017-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

A lightweight, reusable procedural night-sky shader and workflow tool for **Unity 6.3+ (URP 17)**. Built for **stylized / dark-fantasy** worlds and **realistic night** scenes, featuring a toggleable **PS1/PS2 Retro mode**, a built-in **Editor Texture Baker**, and an ultra-light **World-Edge Void Mist / Fake Water Plane** for hiding terrain borders and cliff bases.

Zero external dependencies. Zero HDRP requirements. Zero volumetric raymarching.

---

## Features

* **Spherical Dome Procedural Starfield** — Zero-loop, zero-texture-read star layers with organic galactic clustering, dark voids, power-law magnitude variation, adjustable size & size variation, independent per-star twinkling, and rotated hero star flares.
* **Domain-Warped Organic Nebula** — Pinch-free upper-hemisphere projection driven by a single channel-packed RGBA noise atlas, with 3-stop colour palettes, filament accents, and slow drift animation.
* **Multi-Layer Animated Clouds** — Curved-plane perspective projection with dual-layer velocities and moon-driven backlit rim glow.
* **Customizable Moon & Distant Celestials** — Local tangent-plane projection (scaling the moon never distorts the sky), procedural spherical moon phases, earthshine, halo glow, custom texture support, and an optional ringed planet.
* **Procedural Shooting Stars** — Time-sliced meteor streaks driven purely on `_Time.y` with zero C# `Update()` loops.
* **Lower Hemisphere & World-Edge Blending** — Switch below the horizon between **Solid Gradient**, **Swirling Void Mist**, or **Fake Night Water** (with automatic moon shimmer reflection), paired with a 1-draw-call `Void & Water Plane` shader and 1-click URP Horizon Fog sync.
* **Toggleable PS1/PS2 Retro Mode** — World-locked spherical texel pixelation, perceptual 4×4 Bayer dithering, and colour quantization with zero GPU divergence when off.
* **Built-In Editor Texture Baker** — One-click generation of a seamless 4-channel RGBA noise atlas (`Perlin / Worley / Ridged / Warp`) and procedural lunar crater texture directly inside Unity.
* **Studio Workflow Hub & Custom Inspector** — 9-section collapsible Material Inspector, 4 one-click style presets (*Dreamy Fantasy*, *Dark Gothic Retro*, *Realistic Night*, *Twilight Mist*), and an optional zero-GC `PWSky_Controller` for Directional Light moon syncing and runtime preset crossfading.

---

## Quality Tiers

Scales cleanly to low-end and integrated GPUs without modifying your URP Pipeline Asset (strictly **3 shader variants** total):

| Tier | Star Layers | Nebula | Clouds | Moon & Extras |
| :--- | :--- | :--- | :--- | :--- |
| **Low** | 1 clustered layer | 1 tap (no warp) | 1 scrolling layer | Analytic disc + glow, no meteors |
| **Medium** | 3 layers + hero stars | 2 taps (domain warped) | 2 layers | Textured moon + phases, 1 meteor slot |
| **High** | 4 layers + Milky Way dust + flares | 3 taps (warped + filaments) | 2 layers + fine erosion + rim glow | Dual meteor slots + distant ringed planet |

---

## Installation

### Option 1: Unity Package (Easiest)
1. Download [`PinwheelDreamSky-v1.0.0.unitypackage`](Packages/PinwheelDreamSky-v1.0.0.unitypackage) from the `Packages/` folder (or the [Releases](https://github.com/Bhavishya222/Unity-URP-DreamSky/releases) page).
2. In Unity 6.3+ (URP), go to **Assets > Import Package > Custom Package...** and select the downloaded file.

### Option 2: Manual Copy
Copy `Assets/PinwheelStudios/DreamSky/` into your Unity project's `Assets/PinwheelStudios/` folder.

---

## Quick Start

1. Open **`Tools > Pinwheel Studios > Pinwheel DreamSky`**.
2. Pick a **Style Preset** (`Dreamy Fantasy`, `Dark Gothic Retro`, `Realistic Night`, or `Twilight Mist`) and a **Quality Tier**.
3. Click **Create & Apply Skybox to Scene** — default noise and moon textures bake automatically into `Assets/PinwheelStudios/Generated/DreamSky/` on first run.
4. *(Optional)* Click **Sync Scene Fog to Skybox Horizon Colour** to hide distant terrain edges.
5. *(Optional)* Click **Spawn Synced Void / Water Plane** to wrap mist or fake night water around cliffs and islands. Enable **Depth Texture** on your URP Asset for soft cliff intersections, or uncheck *Soft Cliff Intersection* on the plane material for zero depth-pass cost.
6. *(Optional)* Click **Create / Select Sky Controller** to sync your scene Directional Light with the Skybox Moon.

---

## Folder Structure

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
```

Baked textures and generated materials save to `Assets/PinwheelStudios/Generated/DreamSky/` to keep the tool folder clean and export-ready.

---

## License & Credits

Created by **Bhavishya Awasthi** — **Pinwheel Studios**  
Released under the **MIT License**.

* Contact: `pinwheels.studio.help@gmail.com`
* Discord: [discord.gg/wGyS56Tn2h](https://discord.gg/wGyS56Tn2h)
* X: [@pinwheelgames](https://x.com/pinwheelgames)
* Instagram: [@pinwheel_games](https://instagram.com/pinwheel_games)
