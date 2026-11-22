// ============================================================================
// Pinwheel DreamSky — Studio Setup Hub, Fog Sync & Void/Water Plane Spawner
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

using UnityEditor;
using UnityEngine;

namespace PinwheelStudios.DreamSky.Editor
{
    public class PWSky_ToolWindow : EditorWindow
    {
        private const string SkyboxShaderName = "Pinwheel Studios/DreamSky/Night Skybox";
        private const string PlaneShaderName  = "Pinwheel Studios/DreamSky/Void & Water Plane";
        private const string PresetsFolder    = PWSky_TextureBaker.GeneratedFolder + "/Presets";

        private Vector2 _scrollPos;

        // Setup State
        private PWSky_StylePreset _stylePreset = PWSky_StylePreset.DreamyFantasy;
        private PWSky_QualityTier _qualityTier = PWSky_QualityTier.Medium;
        private string _newMaterialName = "PWSky_NightSkybox";

        // World-Edge Plane State
        private int _planeModeIndex = 0; // 0 = Void Mist, 1 = Fake Water
        private float _planeHeightY = -12f;
        private float _planeScale   = 600f;
        private float _fogDensity   = 0.008f;

        // Linking State
        private Light _moonLight;

        // Texture Baker State
        private int _noiseResIndex = 1; // 0=256, 1=512
        private int _moonResIndex  = 1;
        private int _bakeSeed      = 1337;

        private bool _foldSetup  = true;
        private bool _foldEdge   = true;
        private bool _foldLink   = true;
        private bool _foldBaker  = false;

        [MenuItem("Tools/Pinwheel Studios/Pinwheel DreamSky")]
        public static void OpenWindow()
        {
            PWSky_ToolWindow win = GetWindow<PWSky_ToolWindow>("Pinwheel DreamSky");
            win.minSize = new Vector2(360f, 480f);
            win.Show();
        }

        private void OnEnable()
        {
            if (_moonLight == null)
            {
                _moonLight = FindMainDirectionalLight();
            }
        }

        private void OnGUI()
        {
            PWSky_EditorStyle.DrawBanner("PINWHEEL DREAMSKY", "Procedural URP Night Sky Suite");

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.Space(6f);

            DrawQuickSetupSection();
            EditorGUILayout.Space(4f);

            DrawWorldEdgeSection();
            EditorGUILayout.Space(4f);

            DrawControllerLinkingSection();
            EditorGUILayout.Space(4f);

            DrawTextureBakerSection();
            EditorGUILayout.Space(10f);

            EditorGUILayout.EndScrollView();

            PWSky_EditorStyle.DrawFooterBar();
        }

        // --------------------------------------------------------------------
        // 1. Quick Scene Setup & Preset Material Generator
        // --------------------------------------------------------------------
        private void DrawQuickSetupSection()
        {
            _foldSetup = PWSky_EditorStyle.DrawSectionHeader("1. Quick Skybox Setup & Presets", _foldSetup);
            if (!_foldSetup) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _newMaterialName = EditorGUILayout.TextField("Material Name", _newMaterialName);
                _stylePreset     = (PWSky_StylePreset)EditorGUILayout.EnumPopup("Style Preset", _stylePreset);
                _qualityTier     = (PWSky_QualityTier)EditorGUILayout.EnumPopup("Quality Tier", _qualityTier);

                EditorGUILayout.Space(4f);

                if (GUILayout.Button("Create & Apply Skybox to Scene", GUILayout.Height(28f)))
                {
                    CreateAndApplySkybox();
                }

                EditorGUILayout.Space(2f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Generate Low / Med / High Presets"))
                    {
                        GenerateQualityPresetMaterials();
                    }

                    if (GUILayout.Button("Select Active Skybox", GUILayout.Width(135f)))
                    {
                        if (RenderSettings.skybox != null)
                        {
                            Selection.activeObject = RenderSettings.skybox;
                            EditorGUIUtility.PingObject(RenderSettings.skybox);
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------
        // 2. Horizon Fog Sync & World-Edge Void / Fake Water Plane
        // --------------------------------------------------------------------
        private void DrawWorldEdgeSection()
        {
            _foldEdge = PWSky_EditorStyle.DrawSectionHeader("2. Horizon Fog & World-Edge Blending", _foldEdge);
            if (!_foldEdge) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("URP Built-In Fog Sync (Zero Extra Passes)", EditorStyles.miniBoldLabel);
                _fogDensity = EditorGUILayout.Slider("Fog Density (Exp2)", _fogDensity, 0.0005f, 0.05f);

                if (GUILayout.Button("Sync Scene Fog to Skybox Horizon Colour"))
                {
                    SyncSceneFogToSkybox();
                }

                EditorGUILayout.Space(6f);
                PWSky_EditorStyle.DrawGoldDivider();
                EditorGUILayout.Space(4f);

                EditorGUILayout.LabelField("World-Edge Void Mist / Fake Water Plane (1 Draw Call)", EditorStyles.miniBoldLabel);
                _planeModeIndex = EditorGUILayout.Popup("Plane Surface Mode", _planeModeIndex, new[] { "Void Mist (Covers Cliff Base)", "Fake Night Water" });
                _planeHeightY   = EditorGUILayout.FloatField("World Y Height", _planeHeightY);
                _planeScale     = EditorGUILayout.Slider("Plane Diameter (Meters)", _planeScale, 80f, 3000f);

                EditorGUILayout.Space(3f);
                if (GUILayout.Button("Spawn Synced Void / Water Plane", GUILayout.Height(26f)))
                {
                    SpawnVoidWaterPlane();
                }
            }
        }

        // --------------------------------------------------------------------
        // 3. Moon Directional Light & Runtime Controller Linking
        // --------------------------------------------------------------------
        private void DrawControllerLinkingSection()
        {
            _foldLink = PWSky_EditorStyle.DrawSectionHeader("3. Moon Light & Runtime Controller", _foldLink);
            if (!_foldLink) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _moonLight = (Light)EditorGUILayout.ObjectField("Moon Directional Light", _moonLight, typeof(Light), true);

                EditorGUILayout.Space(3f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create / Select Sky Controller", GUILayout.Height(24f)))
                    {
                        CreateOrSelectController();
                    }

                    if (GUILayout.Button("Match Light to Moon", GUILayout.Height(24f), GUILayout.Width(135f)))
                    {
                        PWSky_Controller ctrl = Object.FindFirstObjectByType<PWSky_Controller>();
                        if (ctrl != null)
                        {
                            Undo.RecordObject(_moonLight != null ? (Object)_moonLight.transform : ctrl, "Match Light to Moon");
                            ctrl.SyncLightFromMoon();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Pinwheel DreamSky", "Create or select a PWSky_Controller first to link your Directional Light.", "OK");
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------
        // 4. Procedural Texture Baker Controls
        // --------------------------------------------------------------------
        private void DrawTextureBakerSection()
        {
            _foldBaker = PWSky_EditorStyle.DrawSectionHeader("4. Procedural Texture Baker", _foldBaker);
            if (!_foldBaker) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string[] resLabels = { "256 x 256 (Ultra-Light)", "512 x 512 (Recommended)" };
                _noiseResIndex = EditorGUILayout.Popup("Noise Atlas Resolution", _noiseResIndex, resLabels);
                _moonResIndex  = EditorGUILayout.Popup("Moon Surface Resolution", _moonResIndex, resLabels);
                _bakeSeed      = EditorGUILayout.IntField("Random Seed", _bakeSeed);

                EditorGUILayout.Space(4f);
                if (GUILayout.Button("Bake Noise Atlas & Moon Texture"))
                {
                    int noiseRes = _noiseResIndex == 0 ? 256 : 512;
                    int moonRes  = _moonResIndex == 0 ? 256 : 512;
                    PWSky_TextureBaker.BakeAllDefaultTextures(noiseRes, moonRes, _bakeSeed);
                }
            }
        }

        // --------------------------------------------------------------------
        // Actions
        // --------------------------------------------------------------------
        private void CreateAndApplySkybox()
        {
            Shader skyShader = Shader.Find(SkyboxShaderName);
            if (skyShader == null)
            {
                EditorUtility.DisplayDialog("Pinwheel DreamSky", $"Could not find shader '{SkyboxShaderName}'. Check for shader compile errors in the Console.", "OK");
                return;
            }

            EnsureFolder(PWSky_TextureBaker.GeneratedFolder);
            string safeName = string.IsNullOrWhiteSpace(_newMaterialName) ? "PWSky_NightSkybox" : _newMaterialName.Trim();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{PWSky_TextureBaker.GeneratedFolder}/{safeName}.mat");

            Material mat = new Material(skyShader);
            PWSky_ShaderGUI.ApplyStylePreset(mat, _stylePreset);
            PWSky_Controller.ApplyQualityKeywords(mat, _qualityTier);

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();

            RenderSettings.skybox = mat;
            DynamicGI.UpdateEnvironment();

            Selection.activeObject = mat;
            EditorGUIUtility.PingObject(mat);
            Debug.Log($"[Pinwheel DreamSky] Created and applied skybox material at '{path}'.");
        }

        private void GenerateQualityPresetMaterials()
        {
            Shader skyShader = Shader.Find(SkyboxShaderName);
            if (skyShader == null) return;

            EnsureFolder(PresetsFolder);

            PWSky_QualityTier[] tiers = { PWSky_QualityTier.Low, PWSky_QualityTier.Medium, PWSky_QualityTier.High };
            Material lastCreated = null;

            foreach (PWSky_QualityTier tier in tiers)
            {
                string path = $"{PresetsFolder}/PWSky_{_stylePreset}_{tier}.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(skyShader);
                    AssetDatabase.CreateAsset(mat, path);
                }

                PWSky_ShaderGUI.ApplyStylePreset(mat, _stylePreset);
                PWSky_Controller.ApplyQualityKeywords(mat, tier);
                EditorUtility.SetDirty(mat);
                lastCreated = mat;
            }

            AssetDatabase.SaveAssets();
            if (lastCreated != null) EditorGUIUtility.PingObject(lastCreated);
            Debug.Log($"[Pinwheel DreamSky] Generated Low / Medium / High preset materials in '{PresetsFolder}'.");
        }

        private void SyncSceneFogToSkybox()
        {
            Material sky = RenderSettings.skybox;
            if (sky == null || !sky.HasProperty("_HorizonColor"))
            {
                EditorUtility.DisplayDialog("Pinwheel DreamSky", "Active Scene Skybox is not a Pinwheel DreamSky material.", "OK");
                return;
            }

            RenderSettings.fog        = true;
            RenderSettings.fogMode    = FogMode.ExponentialSquared;
            RenderSettings.fogColor   = sky.GetColor("_HorizonColor");
            RenderSettings.fogDensity = _fogDensity;

            Debug.Log("[Pinwheel DreamSky] Synced URP Scene Fog colour to active Skybox Horizon Colour.");
        }

        private void SpawnVoidWaterPlane()
        {
            Shader planeShader = Shader.Find(PlaneShaderName);
            if (planeShader == null)
            {
                EditorUtility.DisplayDialog("Pinwheel DreamSky", $"Could not find shader '{PlaneShaderName}'.", "OK");
                return;
            }

            EnsureFolder(PWSky_TextureBaker.GeneratedFolder);
            string suffix = _planeModeIndex == 0 ? "VoidMist" : "FakeWater";
            string matPath = AssetDatabase.GenerateUniqueAssetPath($"{PWSky_TextureBaker.GeneratedFolder}/PWSky_{suffix}Plane.mat");

            Material planeMat = new Material(planeShader);
            planeMat.SetFloat("_PlaneMode", _planeModeIndex);
            planeMat.SetTexture("_NoiseTex", PWSky_TextureBaker.GetOrBakeNoiseAtlas());

            // Sync with active DreamSky skybox if present
            Material sky = RenderSettings.skybox;
            if (sky != null && sky.HasProperty("_HorizonColor"))
            {
                planeMat.SetColor("_HorizonColor", sky.GetColor("_HorizonColor"));
                planeMat.SetFloat("_MoonAzimuth", sky.GetFloat("_MoonAzimuth"));
                planeMat.SetFloat("_MoonElevation", sky.GetFloat("_MoonElevation"));
                planeMat.SetColor("_MoonGlowColor", sky.GetColor("_MoonGlowColor"));
                planeMat.SetColor("_VoidMistColor", sky.GetColor("_VoidMistColor"));
                planeMat.SetColor("_WaterDeepColor", sky.GetColor("_WaterDeepColor"));
                planeMat.SetColor("_WaterReflectColor", sky.GetColor("_WaterReflectColor"));
                planeMat.SetFloat("_RetroEnabled", sky.GetFloat("_RetroEnabled"));

                PWSky_QualityTier tier = (PWSky_QualityTier)Mathf.Clamp(Mathf.RoundToInt(sky.GetFloat("_Quality")), 0, 2);
                PWSky_Controller.ApplyQualityKeywords(planeMat, tier);
            }
            else
            {
                PWSky_Controller.ApplyQualityKeywords(planeMat, _qualityTier);
            }

            AssetDatabase.CreateAsset(planeMat, matPath);
            AssetDatabase.SaveAssets();

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = $"PinwheelDreamSky_{suffix}Plane";
            quad.transform.position   = new Vector3(0f, _planeHeightY, 0f);
            quad.transform.rotation   = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = new Vector3(_planeScale, _planeScale, 1f);

            // Remove collider so the visual edge plane never blocks raycasts or physics
            Collider col = quad.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            MeshRenderer mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = planeMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            Undo.RegisterCreatedObjectUndo(quad, "Spawn DreamSky Void/Water Plane");
            Selection.activeGameObject = quad;

            // Auto-link to controller if one exists in the scene
            PWSky_Controller ctrl = Object.FindFirstObjectByType<PWSky_Controller>();
            if (ctrl != null)
            {
                Undo.RecordObject(ctrl, "Link Void/Water Plane to Controller");
                ctrl.AssignReferences(RenderSettings.skybox, _moonLight, planeMat);
            }

            Debug.Log($"[Pinwheel DreamSky] Spawned '{quad.name}' (1 draw call) with material '{matPath}'.");
        }

        private void CreateOrSelectController()
        {
            PWSky_Controller ctrl = Object.FindFirstObjectByType<PWSky_Controller>();
            if (ctrl == null)
            {
                GameObject go = new GameObject("Pinwheel DreamSky Controller");
                ctrl = go.AddComponent<PWSky_Controller>();
                Undo.RegisterCreatedObjectUndo(go, "Create Pinwheel DreamSky Controller");
            }

            Material planeMat = FindSceneVoidWaterPlaneMaterial();
            Undo.RecordObject(ctrl, "Configure DreamSky Controller");
            ctrl.AssignReferences(RenderSettings.skybox, _moonLight, planeMat);

            Selection.activeGameObject = ctrl.gameObject;
        }

        private static Light FindMainDirectionalLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional && lights[i].isActiveAndEnabled)
                    return lights[i];
            }
            return null;
        }

        private static Material FindSceneVoidWaterPlaneMaterial()
        {
            MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material m = renderers[i].sharedMaterial;
                if (m != null && m.shader != null && m.shader.name == PlaneShaderName)
                    return m;
            }
            return null;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}