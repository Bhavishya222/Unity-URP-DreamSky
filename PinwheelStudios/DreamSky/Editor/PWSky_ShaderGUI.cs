// ============================================================================
// Pinwheel DreamSky — 9-Section Custom Material Inspector & Style Presets
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

using UnityEditor;
using UnityEngine;

namespace PinwheelStudios.DreamSky.Editor
{
    public class PWSky_ShaderGUI : ShaderGUI
    {
        private const string FoldPrefix = "PWSky_Fold_";
        private PWSky_StylePreset _selectedStylePreset = PWSky_StylePreset.DreamyFantasy;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            Material mat = materialEditor.target as Material;
            if (mat == null)
            {
                base.OnGUI(materialEditor, props);
                return;
            }

            // 1. Studio Banner
            PWSky_EditorStyle.DrawBanner("PINWHEEL DREAMSKY", "Procedural URP Night Skybox");
            EditorGUILayout.Space(4f);

            // 2. Quick Quality & Style Preset Bar
            DrawPresetToolbar(materialEditor, mat);
            EditorGUILayout.Space(4f);

            // 3. Missing Baked Textures Helper
            DrawMissingTexturesHelper(mat, props);

            // 4. All 9 Organized Collapsible Sections
            DrawSection1General(materialEditor, props);
            DrawSection2SkyColours(materialEditor, props);
            DrawSection3Stars(materialEditor, props);
            DrawSection4Nebula(materialEditor, props);
            DrawSection5Clouds(materialEditor, props);
            DrawSection6Moon(materialEditor, props);
            DrawSection7AtmosphereAndLower(materialEditor, props);
            DrawSection8AnimationAndRetro(materialEditor, props);
            DrawSection9PerformanceQuality(materialEditor, mat, props);

            EditorGUILayout.Space(6f);
            PWSky_EditorStyle.DrawGoldDivider();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Pinwheel DreamSky  •  Pinwheel Studios", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open Tool Hub", EditorStyles.miniButton, GUILayout.Width(100f)))
                {
                    PWSky_ToolWindow.OpenWindow();
                }
            }
        }

        // --------------------------------------------------------------------
        // Quick Preset Toolbar (Quality Buttons + One-Click Style Presets)
        // --------------------------------------------------------------------
        private void DrawPresetToolbar(MaterialEditor editor, Material mat)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // Quality Tier segmented bar
                int currentQ = Mathf.Clamp(Mathf.RoundToInt(mat.GetFloat("_Quality")), 0, 2);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Quality Preset", EditorStyles.boldLabel, GUILayout.Width(96f));
                    int newQ = GUILayout.Toolbar(currentQ, new[] { "Low", "Medium", "High" });
                    if (newQ != currentQ)
                    {
                        Undo.RecordObjects(editor.targets, "Change DreamSky Quality Tier");
                        foreach (Object obj in editor.targets)
                        {
                            if (obj is Material targetMat)
                            {
                                PWSky_Controller.ApplyQualityKeywords(targetMat, (PWSky_QualityTier)newQ);
                                EditorUtility.SetDirty(targetMat);
                            }
                        }
                    }
                }

                EditorGUILayout.Space(2f);

                // Style Preset selector + Apply
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Style Preset", EditorStyles.boldLabel, GUILayout.Width(96f));
                    _selectedStylePreset = (PWSky_StylePreset)EditorGUILayout.EnumPopup(_selectedStylePreset);
                    if (GUILayout.Button("Apply", GUILayout.Width(64f)))
                    {
                        Undo.RecordObjects(editor.targets, "Apply DreamSky Style Preset");
                        foreach (Object obj in editor.targets)
                        {
                            if (obj is Material targetMat)
                            {
                                ApplyStylePreset(targetMat, _selectedStylePreset);
                                EditorUtility.SetDirty(targetMat);
                            }
                        }
                    }
                }
            }
        }

        private void DrawMissingTexturesHelper(Material mat, MaterialProperty[] props)
        {
            MaterialProperty noiseProp = FindProp("_NoiseTex", props);
            MaterialProperty moonProp  = FindProp("_MoonTex", props);

            bool missingNoise = noiseProp != null && noiseProp.textureValue == null;
            bool missingMoon  = moonProp != null && moonProp.textureValue == null;

            if (!missingNoise && !missingMoon) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Default procedural textures are not assigned yet.", EditorStyles.miniBoldLabel);
                if (GUILayout.Button("Bake & Auto-Assign Noise Atlas + Moon Texture"))
                {
                    Undo.RecordObject(mat, "Assign DreamSky Baked Textures");
                    if (missingNoise) mat.SetTexture("_NoiseTex", PWSky_TextureBaker.GetOrBakeNoiseAtlas());
                    if (missingMoon)  mat.SetTexture("_MoonTex", PWSky_TextureBaker.GetOrBakeMoonTexture());
                    EditorUtility.SetDirty(mat);
                }
            }
            EditorGUILayout.Space(4f);
        }

        // --------------------------------------------------------------------
        // 9 Organized Collapsible Sections
        // --------------------------------------------------------------------
        private void DrawSection1General(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("1. General Settings", "General", true)) return;
            DrawProp(editor, props, "_NoiseTex", "Packed Noise Atlas (RGBA)");
            DrawProp(editor, props, "_Exposure", "Sky Exposure");
            DrawProp(editor, props, "_SkyRotation", "Sky Yaw Rotation");
            DrawProp(editor, props, "_DitherStrength", "Anti-Banding Dither");
            EndSection();
        }

        private void DrawSection2SkyColours(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("2. Sky Colours", "Colours", true)) return;
            DrawProp(editor, props, "_ZenithColor", "Zenith Colour");
            DrawProp(editor, props, "_MidSkyColor", "Mid-Sky Colour");
            DrawProp(editor, props, "_HorizonColor", "Horizon Colour");
            DrawProp(editor, props, "_GroundColor", "Lower / Void Base Colour");
            DrawProp(editor, props, "_HorizonHeight", "Horizon Height Offset");
            DrawProp(editor, props, "_HorizonFalloff", "Horizon Gradient Falloff");
            EndSection();
        }

        private void DrawSection3Stars(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("3. Stars", "Stars", false)) return;
            if (DrawToggleProp(editor, props, "_EnableStars", "Enable Procedural Stars"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_StarTint", "Star Overall Tint");
                DrawProp(editor, props, "_StarDensity", "Star Density");
                DrawProp(editor, props, "_StarSize", "Star Size");
                DrawProp(editor, props, "_StarSizeVariation", "Star Size Variation");
                DrawProp(editor, props, "_StarBrightness", "Star Brightness");
                DrawProp(editor, props, "_StarColorVariation", "Colour Temperature Variation");
                DrawProp(editor, props, "_StarTwinkleSpeed", "Twinkle Speed");
                DrawProp(editor, props, "_StarTwinkleIntensity", "Twinkle Intensity");
                DrawProp(editor, props, "_HeroStarAmount", "Bright Hero Stars");
                DrawProp(editor, props, "_HeroFlareIntensity", "Hero Star Cross Flares");
                EditorGUI.indentLevel--;
            }
            EndSection();
        }

        private void DrawSection4Nebula(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("4. Nebula", "Nebula", false)) return;
            if (DrawToggleProp(editor, props, "_EnableNebula", "Enable Nebula System"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_NebulaColorPrimary", "Primary Colour");
                DrawProp(editor, props, "_NebulaColorSecondary", "Secondary Colour");
                DrawProp(editor, props, "_NebulaColorAccent", "Filament Accent Colour");
                DrawProp(editor, props, "_NebulaIntensity", "Nebula Intensity");
                DrawProp(editor, props, "_NebulaOpacity", "Nebula Opacity");
                DrawProp(editor, props, "_NebulaScale", "Nebula Scale");
                DrawProp(editor, props, "_NebulaDistortion", "Swirl Distortion");
                DrawProp(editor, props, "_NebulaDriftSpeed", "Drift Speed");
                DrawProp(editor, props, "_NebulaColorBlend", "Colour Blending");
                EditorGUI.indentLevel--;
            }
            EndSection();
        }

        private void DrawSection5Clouds(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("5. Clouds", "Clouds", false)) return;
            if (DrawToggleProp(editor, props, "_EnableClouds", "Enable Animated Clouds"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_CloudBaseColor", "Shadow Colour");
                DrawProp(editor, props, "_CloudLitColor", "Lit Colour");
                DrawProp(editor, props, "_CloudRimColor", "Moon-Rim Backlight");
                DrawProp(editor, props, "_CloudRimIntensity", "Moon-Rim Intensity");
                DrawProp(editor, props, "_CloudCoverage", "Cloud Coverage");
                DrawProp(editor, props, "_CloudDensity", "Cloud Density");
                DrawProp(editor, props, "_CloudSoftness", "Edge Softness");
                DrawProp(editor, props, "_CloudOpacity", "Cloud Opacity");
                EditorGUILayout.Space(3f);
                DrawProp(editor, props, "_CloudLayer1Scale", "Layer 1 Scale");
                DrawVector2Prop(props, "_CloudLayer1Speed", "Layer 1 Velocity (X, Z)");
                DrawProp(editor, props, "_CloudLayer2Scale", "Layer 2 Scale");
                DrawVector2Prop(props, "_CloudLayer2Speed", "Layer 2 Velocity (X, Z)");
                EditorGUI.indentLevel--;
            }
            EndSection();
        }

        private void DrawSection6Moon(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("6. Moon", "Moon", false)) return;
            if (DrawToggleProp(editor, props, "_EnableMoon", "Enable Customizable Moon"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_MoonTex", "Moon Surface Texture");
                DrawProp(editor, props, "_MoonAzimuth", "Azimuth (Degrees)");
                DrawProp(editor, props, "_MoonElevation", "Elevation (Degrees)");
                DrawProp(editor, props, "_MoonSize", "Moon Size");
                DrawProp(editor, props, "_MoonColor", "Moon Colour");
                DrawProp(editor, props, "_MoonBrightness", "Moon Brightness");
                DrawProp(editor, props, "_MoonSurfaceDetail", "Surface Detail Strength");
                DrawProp(editor, props, "_MoonPhase", "Moon Phase (0 Full - 0.5 New)");
                DrawProp(editor, props, "_MoonPhaseSoftness", "Phase Edge Softness");
                DrawProp(editor, props, "_MoonEarthshine", "Dark-Side Earthshine");
                DrawProp(editor, props, "_MoonGlowColor", "Halo Glow Colour");
                DrawProp(editor, props, "_MoonGlowIntensity", "Halo Glow Intensity");
                DrawProp(editor, props, "_MoonGlowRadius", "Halo Glow Radius");
                EditorGUI.indentLevel--;
            }
            EndSection();
        }

        private void DrawSection7AtmosphereAndLower(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("7. Atmospheric Effects & Lower Hemisphere", "Atmosphere", false)) return;

            EditorGUILayout.LabelField("Horizon & Environmental Atmosphere", EditorStyles.miniBoldLabel);
            DrawProp(editor, props, "_AtmosphereGlowColor", "Horizon Glow Colour");
            DrawProp(editor, props, "_AtmosphereGlowIntensity", "Horizon Glow Intensity");
            DrawProp(editor, props, "_EnvColorVariation", "Subtle Sky Colour Variation");

            EditorGUILayout.Space(5f);
            if (DrawToggleProp(editor, props, "_EnableShootingStars", "Enable Shooting Stars"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_ShootingStarColor", "Trail Colour");
                DrawProp(editor, props, "_ShootingStarFrequency", "Frequency");
                DrawProp(editor, props, "_ShootingStarSpeed", "Speed");
                DrawProp(editor, props, "_ShootingStarLength", "Trail Length");
                DrawProp(editor, props, "_ShootingStarBrightness", "Brightness");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5f);
            if (DrawToggleProp(editor, props, "_EnableCelestial", "Enable Distant Celestial Object"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_CelestialAzimuth", "Azimuth (Degrees)");
                DrawProp(editor, props, "_CelestialElevation", "Elevation (Degrees)");
                DrawProp(editor, props, "_CelestialSize", "Size");
                DrawProp(editor, props, "_CelestialColor", "Colour");
                DrawProp(editor, props, "_CelestialOpacity", "Opacity");
                DrawProp(editor, props, "_CelestialRingIntensity", "Planetary Ring Intensity");
                DrawProp(editor, props, "_CelestialRingTilt", "Ring Tilt (Degrees)");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Lower Hemisphere (Below Horizon)", EditorStyles.miniBoldLabel);
            MaterialProperty lowerProp = FindProp("_LowerMode", props);
            if (lowerProp != null)
            {
                editor.ShaderProperty(lowerProp, "Lower Hemisphere Mode");
                int mode = Mathf.RoundToInt(lowerProp.floatValue);

                if (mode == (int)PWSky_LowerHemisphereMode.VoidMist)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(editor, props, "_VoidMistColor", "Void Mist Colour");
                    DrawProp(editor, props, "_VoidMistScale", "Void Mist Scale");
                    DrawProp(editor, props, "_VoidMistSpeed", "Swirl Speed");
                    DrawProp(editor, props, "_VoidMistIntensity", "Mist Intensity");
                    EditorGUI.indentLevel--;
                }
                else if (mode == (int)PWSky_LowerHemisphereMode.FakeWater)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(editor, props, "_WaterDeepColor", "Deep Water Colour");
                    DrawProp(editor, props, "_WaterReflectColor", "Sky Reflection Tint");
                    DrawProp(editor, props, "_WaterScale", "Ripple Scale");
                    DrawProp(editor, props, "_WaterSpeed", "Ripple Speed");
                    DrawProp(editor, props, "_WaterMoonShimmer", "Moon Shimmer Intensity");
                    EditorGUI.indentLevel--;
                }
            }

            EndSection();
        }

        private void DrawSection8AnimationAndRetro(MaterialEditor editor, MaterialProperty[] props)
        {
            if (!BeginSection("8. Animation & Retro Stylization", "Animation", false)) return;
            DrawProp(editor, props, "_AnimationSpeed", "Global Animation Speed");

            EditorGUILayout.Space(4f);
            if (DrawToggleProp(editor, props, "_RetroEnabled", "Enable Retro Stylization (PS1 / PS2)"))
            {
                EditorGUI.indentLevel++;
                DrawProp(editor, props, "_RetroPixelate", "Sky Pixelation Blend");
                DrawProp(editor, props, "_RetroPixelResolution", "Sky Texel Resolution");
                DrawProp(editor, props, "_RetroColorSteps", "Colour Quantization Levels");
                DrawProp(editor, props, "_RetroDitherAmount", "Ordered Bayer Dither");
                DrawProp(editor, props, "_RetroDitherScale", "Dither Matrix Pixel Scale");
                DrawProp(editor, props, "_RetroContrast", "Retro Shadow Contrast");
                EditorGUI.indentLevel--;
            }
            EndSection();
        }

        private void DrawSection9PerformanceQuality(MaterialEditor editor, Material mat, MaterialProperty[] props)
        {
            if (!BeginSection("9. Performance & Quality", "Quality", false)) return;

            int q = Mathf.Clamp(Mathf.RoundToInt(mat.GetFloat("_Quality")), 0, 2);
            PWSky_QualityTier newTier = (PWSky_QualityTier)EditorGUILayout.EnumPopup("Active Quality Tier", (PWSky_QualityTier)q);
            if ((int)newTier != q)
            {
                Undo.RecordObjects(editor.targets, "Change DreamSky Quality Tier");
                foreach (Object obj in editor.targets)
                {
                    if (obj is Material targetMat)
                    {
                        PWSky_Controller.ApplyQualityKeywords(targetMat, newTier);
                        EditorUtility.SetDirty(targetMat);
                    }
                }
            }

            string summary = newTier switch
            {
                PWSky_QualityTier.Low =>
                    "Low: 1 star layer, 1-tap nebula (no warp), 1 cloud layer, analytic moon disc, no shooting stars (~2-3 texture taps/px).",
                PWSky_QualityTier.High =>
                    "High: 4 star layers + dust & hero flares, 3-tap warped nebula, 3-tap backlit clouds, dual shooting stars & distant planet (~7-8 texture taps/px).",
                _ =>
                    "Medium: 3 star layers + hero stars, 2-tap warped nebula, 2 cloud layers, textured moon + phases, 1 shooting star slot (~5-6 texture taps/px)."
            };

            EditorGUILayout.HelpBox(summary, MessageType.Info);
            EndSection();
        }

        // --------------------------------------------------------------------
        // Shared Static Preset Applier (Used by both ShaderGUI & ToolWindow)
        // --------------------------------------------------------------------
        public static void ApplyStylePreset(Material mat, PWSky_StylePreset preset)
        {
            if (mat == null) return;

            // Ensure baked textures exist if missing
            if (mat.GetTexture("_NoiseTex") == null) mat.SetTexture("_NoiseTex", PWSky_TextureBaker.GetOrBakeNoiseAtlas());
            if (mat.GetTexture("_MoonTex") == null)  mat.SetTexture("_MoonTex", PWSky_TextureBaker.GetOrBakeMoonTexture());

            switch (preset)
            {
                case PWSky_StylePreset.DreamyFantasy:
                    mat.SetFloat("_Exposure", 1.05f);
                    mat.SetColor("_ZenithColor", new Color(0.012f, 0.014f, 0.052f, 1f));
                    mat.SetColor("_MidSkyColor", new Color(0.062f, 0.040f, 0.165f, 1f));
                    mat.SetColor("_HorizonColor", new Color(0.175f, 0.095f, 0.310f, 1f));
                    mat.SetColor("_GroundColor", new Color(0.020f, 0.016f, 0.058f, 1f));
                    mat.SetFloat("_HorizonFalloff", 1.20f);

                    mat.SetFloat("_EnableStars", 1f);
                    mat.SetFloat("_StarDensity", 0.68f);
                    mat.SetFloat("_StarSize", 1.30f);
                    mat.SetFloat("_StarSizeVariation", 0.80f);
                    mat.SetFloat("_StarBrightness", 1.55f);
                    mat.SetFloat("_StarColorVariation", 0.85f);
                    mat.SetFloat("_HeroStarAmount", 0.72f);
                    mat.SetFloat("_HeroFlareIntensity", 0.65f);

                    mat.SetFloat("_EnableNebula", 1f);
                    mat.SetColor("_NebulaColorPrimary", new Color(0.22f, 0.11f, 0.58f, 1f));
                    mat.SetColor("_NebulaColorSecondary", new Color(0.62f, 0.18f, 0.72f, 1f));
                    mat.SetColor("_NebulaColorAccent", new Color(0.36f, 0.68f, 1.05f, 1f));
                    mat.SetFloat("_NebulaIntensity", 1.05f);
                    mat.SetFloat("_NebulaOpacity", 0.78f);
                    mat.SetFloat("_NebulaDistortion", 1.05f);

                    mat.SetFloat("_EnableClouds", 1f);
                    mat.SetColor("_CloudBaseColor", new Color(0.048f, 0.032f, 0.115f, 1f));
                    mat.SetColor("_CloudLitColor", new Color(0.26f, 0.16f, 0.46f, 1f));
                    mat.SetColor("_CloudRimColor", new Color(0.70f, 0.54f, 1.15f, 1f));
                    mat.SetFloat("_CloudRimIntensity", 1.30f);
                    mat.SetFloat("_CloudCoverage", 0.48f);
                    mat.SetFloat("_CloudOpacity", 0.86f);

                    mat.SetFloat("_EnableMoon", 1f);
                    mat.SetFloat("_MoonSize", 0.09f);
                    mat.SetColor("_MoonColor", new Color(0.90f, 0.94f, 1.0f, 1f));
                    mat.SetColor("_MoonGlowColor", new Color(0.46f, 0.56f, 1.0f, 1f));
                    mat.SetFloat("_MoonGlowIntensity", 0.95f);
                    mat.SetFloat("_MoonPhase", 0.0f);

                    mat.SetColor("_AtmosphereGlowColor", new Color(0.34f, 0.18f, 0.58f, 1f));
                    mat.SetFloat("_AtmosphereGlowIntensity", 0.48f);
                    mat.SetFloat("_EnableShootingStars", 1f);
                    mat.SetFloat("_LowerMode", (float)PWSky_LowerHemisphereMode.VoidMist);
                    mat.SetColor("_VoidMistColor", new Color(0.28f, 0.22f, 0.68f, 1f));
                    mat.SetFloat("_RetroEnabled", 0f);
                    break;

                case PWSky_StylePreset.DarkGothicRetro:
                    mat.SetFloat("_Exposure", 1.08f);
                    mat.SetColor("_ZenithColor", new Color(0.005f, 0.006f, 0.022f, 1f));
                    mat.SetColor("_MidSkyColor", new Color(0.028f, 0.024f, 0.120f, 1f));
                    mat.SetColor("_HorizonColor", new Color(0.085f, 0.070f, 0.280f, 1f));
                    mat.SetColor("_GroundColor", new Color(0.012f, 0.015f, 0.055f, 1f));
                    mat.SetFloat("_HorizonFalloff", 1.35f);

                    mat.SetFloat("_EnableStars", 1f);
                    mat.SetFloat("_StarDensity", 0.58f);
                    mat.SetFloat("_StarSize", 1.45f);
                    mat.SetFloat("_StarSizeVariation", 0.72f);
                    mat.SetFloat("_StarBrightness", 1.50f);
                    mat.SetFloat("_StarColorVariation", 0.55f);
                    mat.SetFloat("_HeroStarAmount", 0.55f);
                    mat.SetFloat("_HeroFlareIntensity", 0.50f);

                    mat.SetFloat("_EnableNebula", 1f);
                    mat.SetColor("_NebulaColorPrimary", new Color(0.10f, 0.14f, 0.65f, 1f));
                    mat.SetColor("_NebulaColorSecondary", new Color(0.32f, 0.12f, 0.68f, 1f));
                    mat.SetColor("_NebulaColorAccent", new Color(0.28f, 0.55f, 1.10f, 1f));
                    mat.SetFloat("_NebulaIntensity", 1.25f);
                    mat.SetFloat("_NebulaOpacity", 0.85f);
                    mat.SetFloat("_NebulaDistortion", 1.25f);

                    mat.SetFloat("_EnableClouds", 1f);
                    mat.SetColor("_CloudBaseColor", new Color(0.018f, 0.020f, 0.068f, 1f));
                    mat.SetColor("_CloudLitColor", new Color(0.14f, 0.16f, 0.45f, 1f));
                    mat.SetColor("_CloudRimColor", new Color(0.42f, 0.62f, 1.20f, 1f));
                    mat.SetFloat("_CloudRimIntensity", 1.40f);
                    mat.SetFloat("_CloudCoverage", 0.54f);
                    mat.SetFloat("_CloudOpacity", 0.92f);

                    mat.SetFloat("_EnableMoon", 1f);
                    mat.SetFloat("_MoonSize", 0.105f);
                    mat.SetColor("_MoonColor", new Color(0.76f, 0.90f, 1.05f, 1f));
                    mat.SetColor("_MoonGlowColor", new Color(0.28f, 0.52f, 1.05f, 1f));
                    mat.SetFloat("_MoonGlowIntensity", 1.10f);
                    mat.SetFloat("_MoonPhase", 0.12f);

                    mat.SetColor("_AtmosphereGlowColor", new Color(0.16f, 0.20f, 0.62f, 1f));
                    mat.SetFloat("_AtmosphereGlowIntensity", 0.55f);
                    mat.SetFloat("_EnableShootingStars", 0f);
                    mat.SetFloat("_LowerMode", (float)PWSky_LowerHemisphereMode.VoidMist);
                    mat.SetColor("_VoidMistColor", new Color(0.22f, 0.30f, 0.82f, 1f));

                    mat.SetFloat("_RetroEnabled", 1f);
                    mat.SetFloat("_RetroPixelate", 1.0f);
                    mat.SetFloat("_RetroPixelResolution", 180f);
                    mat.SetFloat("_RetroColorSteps", 14f);
                    mat.SetFloat("_RetroDitherAmount", 0.92f);
                    mat.SetFloat("_RetroDitherScale", 2f);
                    mat.SetFloat("_RetroContrast", 1.15f);
                    break;

                case PWSky_StylePreset.RealisticNight:
                    mat.SetFloat("_Exposure", 0.95f);
                    mat.SetColor("_ZenithColor", new Color(0.004f, 0.007f, 0.018f, 1f));
                    mat.SetColor("_MidSkyColor", new Color(0.014f, 0.024f, 0.052f, 1f));
                    mat.SetColor("_HorizonColor", new Color(0.042f, 0.062f, 0.105f, 1f));
                    mat.SetColor("_GroundColor", new Color(0.008f, 0.012f, 0.022f, 1f));
                    mat.SetFloat("_HorizonFalloff", 1.65f);

                    mat.SetFloat("_EnableStars", 1f);
                    mat.SetFloat("_StarDensity", 0.72f);
                    mat.SetFloat("_StarSize", 0.85f);
                    mat.SetFloat("_StarSizeVariation", 0.90f);
                    mat.SetFloat("_StarBrightness", 1.25f);
                    mat.SetFloat("_StarColorVariation", 0.55f);
                    mat.SetFloat("_HeroStarAmount", 0.35f);
                    mat.SetFloat("_HeroFlareIntensity", 0.0f);

                    mat.SetFloat("_EnableNebula", 1f);
                    mat.SetColor("_NebulaColorPrimary", new Color(0.06f, 0.08f, 0.14f, 1f));
                    mat.SetColor("_NebulaColorSecondary", new Color(0.11f, 0.13f, 0.19f, 1f));
                    mat.SetColor("_NebulaColorAccent", new Color(0.18f, 0.21f, 0.28f, 1f));
                    mat.SetFloat("_NebulaIntensity", 0.38f);
                    mat.SetFloat("_NebulaOpacity", 0.50f);
                    mat.SetFloat("_NebulaDistortion", 0.55f);

                    mat.SetFloat("_EnableClouds", 1f);
                    mat.SetColor("_CloudBaseColor", new Color(0.014f, 0.018f, 0.032f, 1f));
                    mat.SetColor("_CloudLitColor", new Color(0.055f, 0.068f, 0.095f, 1f));
                    mat.SetColor("_CloudRimColor", new Color(0.28f, 0.34f, 0.45f, 1f));
                    mat.SetFloat("_CloudRimIntensity", 0.65f);
                    mat.SetFloat("_CloudCoverage", 0.36f);
                    mat.SetFloat("_CloudOpacity", 0.68f);

                    mat.SetFloat("_EnableMoon", 1f);
                    mat.SetFloat("_MoonSize", 0.048f);
                    mat.SetColor("_MoonColor", new Color(0.95f, 0.95f, 0.92f, 1f));
                    mat.SetColor("_MoonGlowColor", new Color(0.32f, 0.40f, 0.56f, 1f));
                    mat.SetFloat("_MoonGlowIntensity", 0.45f);
                    mat.SetFloat("_MoonPhase", 0.18f);

                    mat.SetColor("_AtmosphereGlowColor", new Color(0.08f, 0.12f, 0.20f, 1f));
                    mat.SetFloat("_AtmosphereGlowIntensity", 0.22f);
                    mat.SetFloat("_EnableShootingStars", 1f);
                    mat.SetFloat("_ShootingStarFrequency", 0.28f);
                    mat.SetFloat("_LowerMode", (float)PWSky_LowerHemisphereMode.FakeWater);
                    mat.SetColor("_WaterDeepColor", new Color(0.008f, 0.014f, 0.030f, 1f));
                    mat.SetColor("_WaterReflectColor", new Color(0.055f, 0.082f, 0.135f, 1f));
                    mat.SetFloat("_WaterMoonShimmer", 1.15f);
                    mat.SetFloat("_RetroEnabled", 0f);
                    break;

                case PWSky_StylePreset.TwilightMist:
                    mat.SetFloat("_Exposure", 1.0f);
                    mat.SetColor("_ZenithColor", new Color(0.025f, 0.022f, 0.082f, 1f));
                    mat.SetColor("_MidSkyColor", new Color(0.145f, 0.075f, 0.220f, 1f));
                    mat.SetColor("_HorizonColor", new Color(0.380f, 0.165f, 0.260f, 1f));
                    mat.SetColor("_GroundColor", new Color(0.045f, 0.028f, 0.068f, 1f));
                    mat.SetFloat("_HorizonFalloff", 1.05f);

                    mat.SetFloat("_EnableStars", 1f);
                    mat.SetFloat("_StarDensity", 0.48f);
                    mat.SetFloat("_StarSize", 1.10f);
                    mat.SetFloat("_StarSizeVariation", 0.78f);
                    mat.SetFloat("_StarBrightness", 1.15f);
                    mat.SetFloat("_HeroStarAmount", 0.45f);
                    mat.SetFloat("_HeroFlareIntensity", 0.40f);

                    mat.SetFloat("_EnableNebula", 1f);
                    mat.SetColor("_NebulaColorPrimary", new Color(0.32f, 0.12f, 0.42f, 1f));
                    mat.SetColor("_NebulaColorSecondary", new Color(0.68f, 0.26f, 0.45f, 1f));
                    mat.SetColor("_NebulaColorAccent", new Color(0.95f, 0.62f, 0.48f, 1f));
                    mat.SetFloat("_NebulaIntensity", 0.68f);
                    mat.SetFloat("_NebulaOpacity", 0.62f);

                    mat.SetFloat("_EnableClouds", 1f);
                    mat.SetColor("_CloudBaseColor", new Color(0.085f, 0.048f, 0.125f, 1f));
                    mat.SetColor("_CloudLitColor", new Color(0.42f, 0.22f, 0.36f, 1f));
                    mat.SetColor("_CloudRimColor", new Color(1.05f, 0.68f, 0.56f, 1f));
                    mat.SetFloat("_CloudRimIntensity", 1.20f);
                    mat.SetFloat("_CloudCoverage", 0.52f);
                    mat.SetFloat("_CloudOpacity", 0.80f);

                    mat.SetFloat("_EnableMoon", 1f);
                    mat.SetFloat("_MoonSize", 0.08f);
                    mat.SetColor("_MoonColor", new Color(1.0f, 0.93f, 0.84f, 1f));
                    mat.SetColor("_MoonGlowColor", new Color(0.85f, 0.48f, 0.62f, 1f));
                    mat.SetFloat("_MoonGlowIntensity", 0.80f);
                    mat.SetFloat("_MoonPhase", 0.28f);

                    mat.SetColor("_AtmosphereGlowColor", new Color(0.65f, 0.28f, 0.34f, 1f));
                    mat.SetFloat("_AtmosphereGlowIntensity", 0.58f);
                    mat.SetFloat("_EnableShootingStars", 1f);
                    mat.SetFloat("_LowerMode", (float)PWSky_LowerHemisphereMode.VoidMist);
                    mat.SetColor("_VoidMistColor", new Color(0.36f, 0.20f, 0.44f, 1f));
                    mat.SetFloat("_RetroEnabled", 0f);
                    break;
            }
        }

        // --------------------------------------------------------------------
        // GUI Helpers
        // --------------------------------------------------------------------
        private bool BeginSection(string title, string key, bool defaultOpen)
        {
            EditorGUILayout.Space(2f);
            bool open = SessionState.GetBool(FoldPrefix + key, defaultOpen);
            bool next = PWSky_EditorStyle.DrawSectionHeader(title, open);
            if (next != open)
            {
                SessionState.SetBool(FoldPrefix + key, next);
            }

            if (next)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            }
            return next;
        }

        private static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        private static MaterialProperty FindProp(string name, MaterialProperty[] props)
        {
            return FindProperty(name, props, false);
        }

        private static void DrawProp(MaterialEditor editor, MaterialProperty[] props, string propName, string label)
        {
            MaterialProperty p = FindProp(propName, props);
            if (p != null)
            {
                editor.ShaderProperty(p, label);
            }
        }

        private static bool DrawToggleProp(MaterialEditor editor, MaterialProperty[] props, string propName, string label)
        {
            MaterialProperty p = FindProp(propName, props);
            if (p == null) return false;

            editor.ShaderProperty(p, label);
            return p.floatValue > 0.5f;
        }

        private static void DrawVector2Prop(MaterialProperty[] props, string propName, string label)
        {
            MaterialProperty p = FindProp(propName, props);
            if (p == null) return;

            EditorGUI.BeginChangeCheck();
            Vector4 v4 = p.vectorValue;
            Vector2 v2 = EditorGUILayout.Vector2Field(label, new Vector2(v4.x, v4.y));
            if (EditorGUI.EndChangeCheck())
            {
                p.vectorValue = new Vector4(v2.x, v2.y, v4.z, v4.w);
            }
        }
    }
}