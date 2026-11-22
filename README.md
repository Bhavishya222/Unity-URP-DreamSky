# Pinwheel DreamSky — Procedural Night Sky Suite for Unity URP

![Unity 6.3+](https://img.shields.io/badge/Unity-6.3%2B-black?logo=unity)
![URP 17](https://img.shields.io/badge/Pipeline-URP%2017-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

A lightweight, procedural night-sky shader and workflow tool for **Unity 6.3+ (URP 17)**. Built for both **stylized / dark-fantasy** worlds and **realistic night** scenes, featuring a toggleable **PS1/PS2 Retro mode**, a built-in **Editor Texture Baker**, and a 1-draw-call **Void Mist / Fake Water Plane** for hiding world edges and cliff bases.

Zero external dependencies. Zero HDRP requirements. Strictly **3 shader variants** (Low / Medium / High).

---

## Features

* **Spherical Dome Starfield** — Zero-loop, zero-texture-tap star layers with organic galactic clustering, dark voids, adjustable star size & size variation, independent twinkling, and hero star cross-flares.
* **Domain-Warped Nebula** — Pinch-free dome projection powered by a single packed RGBA noise atlas with 3-colour palettes, ridged filaments, and slow drift animation.
* **Animated Multi-Layer Clouds** — Perspective dome clouds with independent layer speeds and moon-driven backlit rim glow.
* **Customizable Moon & Distant Celestials** — Local tangent-plane projection (scaling never distorts the sky), procedural moon phases, earthshine, halo glow, custom texture support, and an optional ringed planet.
* **Procedural Shooting Stars** — Time-sliced meteors driven purely on `_Time.y` with zero C# `Update()` loops.
* **Lower Hemisphere & World-Edge Blending** — Switch below the horizon between **Solid Gradient**, **Swirling Void Mist**, or **Fake Night Water** (with synced moon shimmer), paired with an optional 1-draw-call `Void & Water Plane` shader and 1-click URP Horizon Fog sync.
* **Toggleable PS1/PS2 Retro Mode** — World-locked spherical texel pixelation, perceptual 4×4 ordered Bayer dithering, and colour quantization.
* **Built-In Editor Texture Baker** — Generates a seamless 4-channel RGBA noise atlas (`Perlin / Worley / Ridged / Warp`) and procedural lunar crater texture in one click.
* **Studio Setup Hub & Custom Inspector** — 9-section collapsible Material Inspector, 4 style presets (*Dreamy Fantasy*, *Dark Gothic Retro*, *Realistic Night*, *Twilight Mist*), and an optional zero-GC `PWSky_Controller` for Directional Light moon syncing and runtime preset crossfading.

---

## Quality Tiers

Switch tiers per material without changing your URP Pipeline Asset:

| Tier | Star Layers | Nebula | Clouds | Moon & Extras |
| :--- | :--- | :--- | :--- | :--- |
| **Low** | 1 clustered layer | 1 tap (no warp) | 1 layer | Analytic disc + glow |
| **Medium** | 3 layers + hero stars | 2 taps (warped) | 2 layers | Textured moon + phases, 1 meteor slot |
| **High** | 4 layers + dust + flares | 3 taps (warped + filaments) | 2 layers + fine erosion | Dual meteors + distant ringed planet |

---

## Installation

### Option 1: Unity Package (Recommended)
1. Download `PinwheelDreamSky-v1.0.0.unitypackage` from the [**Releases**](https://github.com/Bhavishya222/Unity-URP-DreamSky/releases) page.
2. In Unity 6.3+ (URP), go to **Assets → Import Package → Custom Package...** and import it.

### Option 2: Manual
Copy `Assets/PinwheelStudios/DreamSky/` into your project's `Assets/PinwheelStudios/` folder.

---

## Quick Start

1. Open **`Tools → Pinwheel Studios → Pinwheel DreamSky`**.
2. Pick a **Style Preset** and **Quality Tier**, then click **Create & Apply Skybox to Scene** (default textures bake automatically on first use into `Assets/PinwheelStudios/Generated/DreamSky/`).
3. *(Optional)* Click **Sync Scene Fog to Skybox Horizon Colour** to hide distant terrain borders.
4. *(Optional)* Click **Spawn Synced Void / Water Plane** to wrap mist or night water around cliff bases and floating islands.
5. *(Optional)* Click **Create / Select Sky Controller** to sync the Skybox Moon to your scene's Directional Light.

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

---

## License & Credits

Created by **Bhavishya Awasthi** at **Pinwheel Studios**.  
Released under the **MIT License** — free for personal and commercial Unity projects.

* Email: `pinwheels.studio.help@gmail.com`
* Discord: [discord.gg/wGyS56Tn2h](https://discord.gg/wGyS56Tn2h)
* X: [@pinwheelgames](https://x.com/pinwheelgames)
* Instagram: [@pinwheel_games](https://instagram.com/pinwheel_games)
