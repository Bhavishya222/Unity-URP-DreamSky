// ============================================================================
// Pinwheel DreamSky — Zero-GC Runtime Skybox, Moon Light & Void Plane Controller
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

using UnityEngine;

namespace PinwheelStudios.DreamSky
{
    [ExecuteAlways]
    [AddComponentMenu("Pinwheel Studios/Pinwheel DreamSky/Sky Controller")]
    public class PWSky_Controller : MonoBehaviour
    {
        // Cached Shader Property IDs (zero string hashing or GC at runtime)
        private static readonly int PropSkyRotation       = Shader.PropertyToID("_SkyRotation");
        private static readonly int PropExposure          = Shader.PropertyToID("_Exposure");
        private static readonly int PropZenithColor       = Shader.PropertyToID("_ZenithColor");
        private static readonly int PropMidSkyColor       = Shader.PropertyToID("_MidSkyColor");
        private static readonly int PropHorizonColor      = Shader.PropertyToID("_HorizonColor");
        private static readonly int PropGroundColor       = Shader.PropertyToID("_GroundColor");
        private static readonly int PropNebulaPrimary     = Shader.PropertyToID("_NebulaColorPrimary");
        private static readonly int PropNebulaSecondary   = Shader.PropertyToID("_NebulaColorSecondary");
        private static readonly int PropNebulaAccent      = Shader.PropertyToID("_NebulaColorAccent");
        private static readonly int PropNebulaIntensity   = Shader.PropertyToID("_NebulaIntensity");
        private static readonly int PropCloudBaseColor    = Shader.PropertyToID("_CloudBaseColor");
        private static readonly int PropCloudLitColor     = Shader.PropertyToID("_CloudLitColor");
        private static readonly int PropCloudRimColor     = Shader.PropertyToID("_CloudRimColor");
        private static readonly int PropCloudCoverage     = Shader.PropertyToID("_CloudCoverage");
        private static readonly int PropMoonAzimuth       = Shader.PropertyToID("_MoonAzimuth");
        private static readonly int PropMoonElevation     = Shader.PropertyToID("_MoonElevation");
        private static readonly int PropMoonPhase         = Shader.PropertyToID("_MoonPhase");
        private static readonly int PropMoonGlowColor     = Shader.PropertyToID("_MoonGlowColor");
        private static readonly int PropAtmoGlowColor     = Shader.PropertyToID("_AtmosphereGlowColor");
        private static readonly int PropLowerMode         = Shader.PropertyToID("_LowerMode");
        private static readonly int PropRetroEnabled      = Shader.PropertyToID("_RetroEnabled");
        private static readonly int PropQuality           = Shader.PropertyToID("_Quality");
        private static readonly int PropVoidMistColor     = Shader.PropertyToID("_VoidMistColor");
        private static readonly int PropWaterDeepColor    = Shader.PropertyToID("_WaterDeepColor");
        private static readonly int PropWaterReflectColor = Shader.PropertyToID("_WaterReflectColor");
        private static readonly int PropPlaneMode         = Shader.PropertyToID("_PlaneMode");

        [Header("References")]
        [Tooltip("Target DreamSky skybox material. Leave empty to use RenderSettings.skybox.")]
        [SerializeField] private Material skyboxMaterial;

        [Tooltip("Optional Directional Light to synchronize with the Skybox Moon position.")]
        [SerializeField] private Light moonDirectionalLight;

        [Tooltip("Optional Void & Water Plane material to keep moon shimmer, horizon, and retro settings synced.")]
        [SerializeField] private Material voidWaterPlaneMaterial;

        [Header("Synchronization")]
        [Tooltip("Automatically updates Skybox Moon position whenever the Directional Light rotates (zero work when stationary).")]
        [SerializeField] private bool syncMoonFromDirectionalLight = true;

        [Tooltip("Matches Unity Scene Fog colour to the Skybox Horizon Colour whenever synced or crossfading.")]
        [SerializeField] private bool syncSceneFogColor = false;

        [Tooltip("If enabled in Play Mode, creates a runtime material instance so scene changes never dirty your project asset.")]
        [SerializeField] private bool instantiateMaterialInPlayMode = true;

        [Header("Optional Slow Drift (0 = Static, Zero Update Work)")]
        [Tooltip("Degrees per second to slowly rotate the entire night sky dome.")]
        [SerializeField] private float slowSkyRotationSpeed = 0f;

        private Material _activeMaterial;
        private bool _ownsInstance;

        // Zero-GC smooth crossfade state
        private bool _isTransitioning;
        private float _transitionElapsed;
        private float _transitionDuration;
        private Material _transitionTarget;

        private Color _startZenith, _startMidSky, _startHorizon, _startGround;
        private Color _startNebPrimary, _startNebSecondary, _startNebAccent;
        private Color _startCloudBase, _startCloudLit, _startCloudRim;
        private Color _startMoonGlow, _startAtmoGlow;
        private float _startExposure, _startNebIntensity, _startCloudCoverage;

        public Material ActiveSkyboxMaterial => _activeMaterial != null ? _activeMaterial : ResolveSkyboxMaterial();

        private void OnEnable()
        {
            InitializeMaterial();
            SyncAllNow();
        }

        private void OnDisable()
        {
            CleanupRuntimeInstance();
        }

        private void OnDestroy()
        {
            CleanupRuntimeInstance();
        }

        private void Update()
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat == null) return;

            // 1. Optional Directional Light -> Moon sync (runs ONLY on frames where the light actually rotates)
            if (syncMoonFromDirectionalLight && moonDirectionalLight != null && moonDirectionalLight.transform.hasChanged)
            {
                SyncMoonFromLight(mat);
                moonDirectionalLight.transform.hasChanged = false;
            }

            // 2. Optional slow sky rotation (skipped completely when speed is 0)
            if (Application.isPlaying && Mathf.Abs(slowSkyRotationSpeed) > 0.0001f)
            {
                float rot = Mathf.Repeat(mat.GetFloat(PropSkyRotation) + slowSkyRotationSpeed * Time.deltaTime, 360f);
                mat.SetFloat(PropSkyRotation, rot);
            }

            // 3. Optional zero-GC preset crossfade
            if (_isTransitioning)
            {
                StepTransition(mat);
            }
        }

        // --------------------------------------------------------------------
        // Public Runtime API
        // --------------------------------------------------------------------

        /// <summary>
        /// Immediately syncs Directional Light, Void/Water Plane, and Scene Fog with the active skybox.
        /// </summary>
        public void SyncAllNow()
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat == null) return;

            if (syncMoonFromDirectionalLight && moonDirectionalLight != null)
            {
                SyncMoonFromLight(mat);
                moonDirectionalLight.transform.hasChanged = false;
            }

            SyncVoidWaterPlane(mat);

            if (syncSceneFogColor)
            {
                RenderSettings.fogColor = mat.GetColor(PropHorizonColor);
            }
        }

        /// <summary>
        /// Rotates the assigned Directional Light to match the current Skybox Moon Azimuth & Elevation.
        /// </summary>
        public void SyncLightFromMoon()
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat == null || moonDirectionalLight == null) return;

            float az = mat.GetFloat(PropMoonAzimuth) * Mathf.Deg2Rad;
            float el = mat.GetFloat(PropMoonElevation) * Mathf.Deg2Rad;
            float cosEl = Mathf.Cos(el);

            Vector3 shaderMoonDir = new Vector3(Mathf.Sin(az) * cosEl, Mathf.Sin(el), Mathf.Cos(az) * cosEl);
            float skyRot = mat.GetFloat(PropSkyRotation);
            Vector3 worldMoonDir = RotateAroundY(shaderMoonDir, -skyRot);

            moonDirectionalLight.transform.rotation = Quaternion.LookRotation(-worldMoonDir, Vector3.up);
            moonDirectionalLight.transform.hasChanged = false;
        }

        public void SetQualityTier(PWSky_QualityTier tier)
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat != null)
            {
                ApplyQualityKeywords(mat, tier);
            }

            if (voidWaterPlaneMaterial != null)
            {
                ApplyQualityKeywords(voidWaterPlaneMaterial, tier);
            }
        }

        public void SetLowerHemisphereMode(PWSky_LowerHemisphereMode mode)
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat == null) return;

            mat.SetFloat(PropLowerMode, (float)mode);
            SyncVoidWaterPlane(mat);
        }

        public void SetRetroMode(bool enabled)
        {
            float val = enabled ? 1f : 0f;

            Material mat = ActiveSkyboxMaterial;
            if (mat != null) mat.SetFloat(PropRetroEnabled, val);
            if (voidWaterPlaneMaterial != null) voidWaterPlaneMaterial.SetFloat(PropRetroEnabled, val);
        }

        public void SetMoonPosition(float azimuthDeg, float elevationDeg)
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat == null) return;

            float wrappedAz = Mathf.Repeat(azimuthDeg, 360f);
            float clampedEl = Mathf.Clamp(elevationDeg, -15f, 88f);

            mat.SetFloat(PropMoonAzimuth, wrappedAz);
            mat.SetFloat(PropMoonElevation, clampedEl);

            if (voidWaterPlaneMaterial != null)
            {
                voidWaterPlaneMaterial.SetFloat(PropMoonAzimuth, wrappedAz);
                voidWaterPlaneMaterial.SetFloat(PropMoonElevation, clampedEl);
            }
        }

        public void SetMoonPhase(float phase01)
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat != null)
            {
                mat.SetFloat(PropMoonPhase, Mathf.Repeat(phase01, 1f));
            }
        }

        /// <summary>
        /// Smoothly crossfades key atmospheric colours, nebula, clouds, and exposure toward another
        /// DreamSky preset material over the given duration with zero GC allocations.
        /// </summary>
        public void CrossfadeToPreset(Material targetPresetMaterial, float durationSeconds)
        {
            Material mat = ActiveSkyboxMaterial;
            if (mat == null || targetPresetMaterial == null) return;

            if (durationSeconds <= 0.001f)
            {
                mat.CopyMatchingPropertiesFromMaterial(targetPresetMaterial);
                SyncAllNow();
                _isTransitioning = false;
                return;
            }

            _transitionTarget   = targetPresetMaterial;
            _transitionDuration = durationSeconds;
            _transitionElapsed  = 0f;
            _isTransitioning    = true;

            _startZenith        = mat.GetColor(PropZenithColor);
            _startMidSky        = mat.GetColor(PropMidSkyColor);
            _startHorizon       = mat.GetColor(PropHorizonColor);
            _startGround        = mat.GetColor(PropGroundColor);
            _startNebPrimary    = mat.GetColor(PropNebulaPrimary);
            _startNebSecondary  = mat.GetColor(PropNebulaSecondary);
            _startNebAccent     = mat.GetColor(PropNebulaAccent);
            _startCloudBase     = mat.GetColor(PropCloudBaseColor);
            _startCloudLit      = mat.GetColor(PropCloudLitColor);
            _startCloudRim      = mat.GetColor(PropCloudRimColor);
            _startMoonGlow      = mat.GetColor(PropMoonGlowColor);
            _startAtmoGlow      = mat.GetColor(PropAtmoGlowColor);
            _startExposure      = mat.GetFloat(PropExposure);
            _startNebIntensity  = mat.GetFloat(PropNebulaIntensity);
            _startCloudCoverage = mat.GetFloat(PropCloudCoverage);
        }

        public void AssignReferences(Material skyMat, Light moonLight, Material planeMat)
        {
            skyboxMaterial = skyMat;
            moonDirectionalLight = moonLight;
            voidWaterPlaneMaterial = planeMat;
            InitializeMaterial();
            SyncAllNow();
        }

        // --------------------------------------------------------------------
        // Internal Helpers
        // --------------------------------------------------------------------

        private void InitializeMaterial()
        {
            CleanupRuntimeInstance();
            Material source = ResolveSkyboxMaterial();
            if (source == null) return;

            if (Application.isPlaying && instantiateMaterialInPlayMode)
            {
                _activeMaterial = new Material(source)
                {
                    name = source.name + " (Runtime Instance)"
                };
                _ownsInstance = true;
                RenderSettings.skybox = _activeMaterial;
            }
            else
            {
                _activeMaterial = source;
                _ownsInstance = false;
            }
        }

        private void CleanupRuntimeInstance()
        {
            if (_ownsInstance && _activeMaterial != null)
            {
                if (RenderSettings.skybox == _activeMaterial && skyboxMaterial != null)
                {
                    RenderSettings.skybox = skyboxMaterial;
                }

                if (Application.isPlaying) Destroy(_activeMaterial);
                else DestroyImmediate(_activeMaterial);
            }

            _activeMaterial = null;
            _ownsInstance = false;
        }

        private Material ResolveSkyboxMaterial()
        {
            return skyboxMaterial != null ? skyboxMaterial : RenderSettings.skybox;
        }

        private void SyncMoonFromLight(Material mat)
        {
            Vector3 worldToMoon = -moonDirectionalLight.transform.forward;
            float skyRot = mat.GetFloat(PropSkyRotation);
            Vector3 shaderDir = RotateAroundY(worldToMoon, skyRot).normalized;

            float elDeg = Mathf.Asin(Mathf.Clamp(shaderDir.y, -1f, 1f)) * Mathf.Rad2Deg;
            float azDeg = Mathf.Repeat(Mathf.Atan2(shaderDir.x, shaderDir.z) * Mathf.Rad2Deg, 360f);

            mat.SetFloat(PropMoonAzimuth, azDeg);
            mat.SetFloat(PropMoonElevation, Mathf.Clamp(elDeg, -15f, 88f));

            if (voidWaterPlaneMaterial != null)
            {
                voidWaterPlaneMaterial.SetFloat(PropMoonAzimuth, azDeg);
                voidWaterPlaneMaterial.SetFloat(PropMoonElevation, Mathf.Clamp(elDeg, -15f, 88f));
            }
        }

        private void SyncVoidWaterPlane(Material skyMat)
        {
            if (voidWaterPlaneMaterial == null || skyMat == null) return;

            voidWaterPlaneMaterial.SetColor(PropHorizonColor, skyMat.GetColor(PropHorizonColor));
            voidWaterPlaneMaterial.SetFloat(PropMoonAzimuth, skyMat.GetFloat(PropMoonAzimuth));
            voidWaterPlaneMaterial.SetFloat(PropMoonElevation, skyMat.GetFloat(PropMoonElevation));
            voidWaterPlaneMaterial.SetColor(PropMoonGlowColor, skyMat.GetColor(PropMoonGlowColor));
            voidWaterPlaneMaterial.SetColor(PropVoidMistColor, skyMat.GetColor(PropVoidMistColor));
            voidWaterPlaneMaterial.SetColor(PropWaterDeepColor, skyMat.GetColor(PropWaterDeepColor));
            voidWaterPlaneMaterial.SetColor(PropWaterReflectColor, skyMat.GetColor(PropWaterReflectColor));
            voidWaterPlaneMaterial.SetFloat(PropRetroEnabled, skyMat.GetFloat(PropRetroEnabled));

            int lowerMode = Mathf.RoundToInt(skyMat.GetFloat(PropLowerMode));
            if (lowerMode == (int)PWSky_LowerHemisphereMode.FakeWater)
                voidWaterPlaneMaterial.SetFloat(PropPlaneMode, 1f);
            else if (lowerMode == (int)PWSky_LowerHemisphereMode.VoidMist)
                voidWaterPlaneMaterial.SetFloat(PropPlaneMode, 0f);

            PWSky_QualityTier tier = (PWSky_QualityTier)Mathf.Clamp(Mathf.RoundToInt(skyMat.GetFloat(PropQuality)), 0, 2);
            ApplyQualityKeywords(voidWaterPlaneMaterial, tier);
        }

        private void StepTransition(Material mat)
        {
            if (_transitionTarget == null)
            {
                _isTransitioning = false;
                return;
            }

            _transitionElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_transitionElapsed / Mathf.Max(_transitionDuration, 0.001f));
            float s = t * t * (3f - 2f * t);

            Color horizon = Color.Lerp(_startHorizon, _transitionTarget.GetColor(PropHorizonColor), s);

            mat.SetColor(PropZenithColor, Color.Lerp(_startZenith, _transitionTarget.GetColor(PropZenithColor), s));
            mat.SetColor(PropMidSkyColor, Color.Lerp(_startMidSky, _transitionTarget.GetColor(PropMidSkyColor), s));
            mat.SetColor(PropHorizonColor, horizon);
            mat.SetColor(PropGroundColor, Color.Lerp(_startGround, _transitionTarget.GetColor(PropGroundColor), s));
            mat.SetColor(PropNebulaPrimary, Color.Lerp(_startNebPrimary, _transitionTarget.GetColor(PropNebulaPrimary), s));
            mat.SetColor(PropNebulaSecondary, Color.Lerp(_startNebSecondary, _transitionTarget.GetColor(PropNebulaSecondary), s));
            mat.SetColor(PropNebulaAccent, Color.Lerp(_startNebAccent, _transitionTarget.GetColor(PropNebulaAccent), s));
            mat.SetColor(PropCloudBaseColor, Color.Lerp(_startCloudBase, _transitionTarget.GetColor(PropCloudBaseColor), s));
            mat.SetColor(PropCloudLitColor, Color.Lerp(_startCloudLit, _transitionTarget.GetColor(PropCloudLitColor), s));
            mat.SetColor(PropCloudRimColor, Color.Lerp(_startCloudRim, _transitionTarget.GetColor(PropCloudRimColor), s));
            mat.SetColor(PropMoonGlowColor, Color.Lerp(_startMoonGlow, _transitionTarget.GetColor(PropMoonGlowColor), s));
            mat.SetColor(PropAtmoGlowColor, Color.Lerp(_startAtmoGlow, _transitionTarget.GetColor(PropAtmoGlowColor), s));
            mat.SetFloat(PropExposure, Mathf.Lerp(_startExposure, _transitionTarget.GetFloat(PropExposure), s));
            mat.SetFloat(PropNebulaIntensity, Mathf.Lerp(_startNebIntensity, _transitionTarget.GetFloat(PropNebulaIntensity), s));
            mat.SetFloat(PropCloudCoverage, Mathf.Lerp(_startCloudCoverage, _transitionTarget.GetFloat(PropCloudCoverage), s));

            if (syncSceneFogColor)
            {
                RenderSettings.fogColor = horizon;
            }

            if (voidWaterPlaneMaterial != null)
            {
                voidWaterPlaneMaterial.SetColor(PropHorizonColor, horizon);
                voidWaterPlaneMaterial.SetColor(PropMoonGlowColor, mat.GetColor(PropMoonGlowColor));
            }

            if (t >= 1f)
            {
                _isTransitioning = false;
            }
        }

        public static void ApplyQualityKeywords(Material mat, PWSky_QualityTier tier)
        {
            if (mat == null) return;

            mat.SetFloat(PropQuality, (float)tier);
            mat.DisableKeyword("_QUALITY_LOW");
            mat.DisableKeyword("_QUALITY_MEDIUM");
            mat.DisableKeyword("_QUALITY_HIGH");

            switch (tier)
            {
                case PWSky_QualityTier.Low:    mat.EnableKeyword("_QUALITY_LOW");    break;
                case PWSky_QualityTier.High:   mat.EnableKeyword("_QUALITY_HIGH");   break;
                default:                       mat.EnableKeyword("_QUALITY_MEDIUM"); break;
            }
        }

        private static Vector3 RotateAroundY(Vector3 dir, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad);
            float c = Mathf.Cos(rad);
            return new Vector3(dir.x * c - dir.z * s, dir.y, dir.x * s + dir.z * c);
        }
    }
}