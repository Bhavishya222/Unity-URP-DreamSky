// ============================================================================
// Pinwheel DreamSky — Procedural Seamless RGBA Noise & Lunar Texture Baker
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

using System.IO;
using UnityEditor;
using UnityEngine;

namespace PinwheelStudios.DreamSky.Editor
{
    public static class PWSky_TextureBaker
    {
        public const string GeneratedFolder = "Assets/PinwheelStudios/Generated/DreamSky";
        public const string NoiseAssetPath  = GeneratedFolder + "/PWSky_NoiseRGBA.png";
        public const string MoonAssetPath   = GeneratedFolder + "/PWSky_Moon.png";

        [MenuItem("Tools/Pinwheel Studios/DreamSky Utilities/Bake Default Textures")]
        public static void BakeAllDefaultTextures()
        {
            BakeAllDefaultTextures(512, 512, 1337);
        }

        public static void BakeAllDefaultTextures(int noiseResolution, int moonResolution, int seed)
        {
            EnsureDirectory(GeneratedFolder);

            try
            {
                EditorUtility.DisplayProgressBar(
                    "Pinwheel DreamSky — Texture Baker",
                    "Baking seamless 4-channel RGBA noise atlas...",
                    0.15f);

                BakeNoiseAtlas(noiseResolution, seed);

                EditorUtility.DisplayProgressBar(
                    "Pinwheel DreamSky — Texture Baker",
                    "Baking procedural lunar surface texture...",
                    0.65f);

                BakeMoonTexture(moonResolution, seed + 42);

                EditorUtility.DisplayProgressBar(
                    "Pinwheel DreamSky — Texture Baker",
                    "Importing baked textures...",
                    0.92f);

                AssetDatabase.Refresh();
                ConfigureNoiseImporter(NoiseAssetPath);
                ConfigureMoonImporter(MoonAssetPath);

                Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(NoiseAssetPath);
                if (noiseTex != null)
                {
                    EditorGUIUtility.PingObject(noiseTex);
                }

                Debug.Log($"[Pinwheel DreamSky] Baked default textures to '{GeneratedFolder}'.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static Texture2D GetOrBakeNoiseAtlas()
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(NoiseAssetPath);
            if (tex == null)
            {
                BakeAllDefaultTextures();
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(NoiseAssetPath);
            }
            return tex;
        }

        public static Texture2D GetOrBakeMoonTexture()
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(MoonAssetPath);
            if (tex == null)
            {
                BakeAllDefaultTextures();
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(MoonAssetPath);
            }
            return tex;
        }

        // --------------------------------------------------------------------
        // 1. Seamless 4-Channel Noise Atlas
        //    R = Smooth Perlin fBm (base nebula & cloud body)
        //    G = Billowy Inverted Worley fBm (cloud puffs & nebula clumps)
        //    B = High-Frequency Wispy Ridged fBm (fine detail & Milky Way dust)
        //    A = Low-Frequency Smooth Warp Noise (domain distortion)
        // --------------------------------------------------------------------
        private static void BakeNoiseAtlas(int size, int seed)
        {
            int count = size * size;
            float[] chR = new float[count];
            float[] chG = new float[count];
            float[] chB = new float[count];
            float[] chA = new float[count];

            float invSize = 1f / size;

            for (int y = 0; y < size; y++)
            {
                float v = y * invSize;
                int rowOffset = y * size;

                for (int x = 0; x < size; x++)
                {
                    float u = x * invSize;
                    int idx = rowOffset + x;

                    // R: 4-octave seamless Perlin fBm
                    float r = TileableValueFbm(u, v, 4, 4, 0.5f, seed + 11);

                    // G: 2-octave billowy seamless Worley (inverted cellular)
                    float w1 = 1f - TileableWorley(u, v, 6, seed + 101);
                    float w2 = 1f - TileableWorley(u, v, 12, seed + 211);
                    float g = w1 * 0.68f + w2 * 0.32f;

                    // B: High-frequency wispy ridged noise
                    float b1 = 1f - Mathf.Abs(TileableValueNoise(u, v, 10, seed + 307) * 2f - 1f);
                    float b2 = 1f - Mathf.Abs(TileableValueNoise(u, v, 20, seed + 409) * 2f - 1f);
                    float b3 = TileableValueNoise(u, v, 40, seed + 503);
                    float b = b1 * b1 * 0.55f + b2 * b2 * 0.30f + b3 * 0.15f;

                    // A: Smooth low-frequency warp driver
                    float a = TileableValueFbm(u, v, 3, 2, 0.45f, seed + 613);

                    chR[idx] = r;
                    chG[idx] = g;
                    chB[idx] = b;
                    chA[idx] = a;
                }
            }

            NormalizeChannel(chR);
            NormalizeChannel(chG);
            NormalizeChannel(chB);
            NormalizeChannel(chA);

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[count];
            for (int i = 0; i < count; i++)
            {
                pixels[i] = new Color(chR[i], chG[i], chB[i], chA[i]);
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            File.WriteAllBytes(NoiseAssetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // --------------------------------------------------------------------
        // 2. Procedural Lunar Surface Texture (Sphere-Mapped with Edge Bleed)
        // --------------------------------------------------------------------
        private static void BakeMoonTexture(int size, int seed)
        {
            // Pre-generate deterministic 3D crater centers on the front hemisphere
            const int craterCount = 44;
            Vector3[] craterDirs = new Vector3[craterCount];
            float[] craterRadii  = new float[craterCount];
            float[] craterDepths = new float[craterCount];

            for (int i = 0; i < craterCount; i++)
            {
                float h1 = Hash1D(i * 3 + 1, seed);
                float h2 = Hash1D(i * 3 + 2, seed);
                float h3 = Hash1D(i * 3 + 3, seed);

                float theta = h1 * Mathf.PI * 2f;
                float z = Mathf.Lerp(0.15f, 1.0f, h2);
                float xy = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));

                craterDirs[i] = new Vector3(Mathf.Cos(theta) * xy, Mathf.Sin(theta) * xy, z).normalized;
                craterRadii[i] = Mathf.Lerp(0.055f, 0.24f, h3 * h3);
                craterDepths[i] = Mathf.Lerp(0.10f, 0.28f, Hash1D(i * 7 + 19, seed));
            }

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            Color[] pixels = new Color[size * size];
            float invSize = 1f / size;

            for (int y = 0; y < size; y++)
            {
                float ny = ((y + 0.5f) * invSize) * 2f - 1f;
                int rowOffset = y * size;

                for (int x = 0; x < size; x++)
                {
                    float nx = ((x + 0.5f) * invSize) * 2f - 1f;
                    float distSq = nx * nx + ny * ny;

                    // Clamp outside-disc coordinates to the rim so mipmaps never bleed black
                    float sx = nx;
                    float sy = ny;
                    if (distSq > 0.999f)
                    {
                        float invLen = 0.999f / Mathf.Sqrt(distSq);
                        sx *= invLen;
                        sy *= invLen;
                        distSq = sx * sx + sy * sy;
                    }

                    float sz = Mathf.Sqrt(Mathf.Max(0.0001f, 1f - distSq));
                    Vector3 spherePos = new Vector3(sx, sy, sz);

                    // Large dark lunar maria (seas)
                    float maria = ValueNoise3D(spherePos * 2.15f + new Vector3(1.7f, -0.8f, 0.4f), seed + 17);
                    maria = SmoothStep(0.32f, 0.68f, maria);

                    // Medium highland detail & micro regolith
                    float highland = ValueFbm3D(spherePos * 4.8f, 4, seed + 83);
                    float fineGrain = ValueNoise3D(spherePos * 26f, seed + 197);

                    float albedo = Mathf.Lerp(0.56f, 0.95f, maria);
                    albedo *= Mathf.Lerp(0.84f, 1.06f, highland);
                    albedo *= Mathf.Lerp(0.95f, 1.04f, fineGrain);

                    // Apply spherical impact craters (darkened bowl + subtle bright rim)
                    for (int c = 0; c < craterCount; c++)
                    {
                        float d = Vector3.Distance(spherePos, craterDirs[c]);
                        float r = craterRadii[c];
                        if (d < r * 1.35f)
                        {
                            float t = d / r;
                            if (t < 1f)
                            {
                                float bowl = 1f - SmoothStep(0.35f, 1.0f, t);
                                albedo -= bowl * craterDepths[c];
                            }
                            float rim = Mathf.Exp(-Mathf.Pow((t - 1.0f) * 5.2f, 2f));
                            albedo += rim * craterDepths[c] * 0.45f;
                        }
                    }

                    albedo = Mathf.Clamp01(albedo);

                    // Subtle cool-silver vs warm-dust tint variation
                    float rCol = Mathf.Clamp01(albedo * 0.99f);
                    float gCol = Mathf.Clamp01(albedo * 0.995f);
                    float bCol = Mathf.Clamp01(albedo * 1.02f);

                    pixels[rowOffset + x] = new Color(rCol, gCol, bCol, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            File.WriteAllBytes(MoonAssetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // --------------------------------------------------------------------
        // Texture Importer Setup
        // --------------------------------------------------------------------
        private static void ConfigureNoiseImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false; // Linear data for shader math
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void ConfigureMoonImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        // --------------------------------------------------------------------
        // Deterministic Tileable 2D & 3D Noise Helpers (Zero External Deps)
        // --------------------------------------------------------------------
        private static float TileableValueFbm(float u, float v, int basePeriod, int octaves, float persistence, int seed)
        {
            float sum = 0f;
            float amp = 1f;
            float ampTotal = 0f;
            int period = basePeriod;

            for (int i = 0; i < octaves; i++)
            {
                sum += TileableValueNoise(u, v, period, seed + i * 79) * amp;
                ampTotal += amp;
                amp *= persistence;
                period *= 2;
            }

            return sum / ampTotal;
        }

        private static float TileableValueNoise(float u, float v, int period, int seed)
        {
            float fx = u * period;
            float fy = v * period;

            int x0 = Mathf.FloorToInt(fx);
            int y0 = Mathf.FloorToInt(fy);
            float tx = fx - x0;
            float ty = fy - y0;

            float sx = tx * tx * (3f - 2f * tx);
            float sy = ty * ty * (3f - 2f * ty);

            int ix0 = Mod(x0, period);
            int iy0 = Mod(y0, period);
            int ix1 = Mod(x0 + 1, period);
            int iy1 = Mod(y0 + 1, period);

            float c00 = Hash2D(ix0, iy0, seed);
            float c10 = Hash2D(ix1, iy0, seed);
            float c01 = Hash2D(ix0, iy1, seed);
            float c11 = Hash2D(ix1, iy1, seed);

            float nx0 = Mathf.Lerp(c00, c10, sx);
            float nx1 = Mathf.Lerp(c01, c11, sx);
            return Mathf.Lerp(nx0, nx1, sy);
        }

        private static float TileableWorley(float u, float v, int period, int seed)
        {
            float fx = u * period;
            float fy = v * period;

            int cx = Mathf.FloorToInt(fx);
            int cy = Mathf.FloorToInt(fy);
            float lx = fx - cx;
            float ly = fy - cy;

            float minDistSq = 4f;

            for (int oy = -1; oy <= 1; oy++)
            {
                int ny = Mod(cy + oy, period);
                for (int ox = -1; ox <= 1; ox++)
                {
                    int nx = Mod(cx + ox, period);
                    float px = ox + Hash2D(nx, ny, seed);
                    float py = oy + Hash2D(nx, ny, seed + 1931);

                    float dx = px - lx;
                    float dy = py - ly;
                    float dSq = dx * dx + dy * dy;
                    if (dSq < minDistSq)
                    {
                        minDistSq = dSq;
                    }
                }
            }

            return Mathf.Clamp01(Mathf.Sqrt(minDistSq));
        }

        private static float ValueFbm3D(Vector3 p, int octaves, int seed)
        {
            float sum = 0f;
            float amp = 1f;
            float ampTotal = 0f;
            float freq = 1f;

            for (int i = 0; i < octaves; i++)
            {
                sum += ValueNoise3D(p * freq, seed + i * 131) * amp;
                ampTotal += amp;
                amp *= 0.5f;
                freq *= 2.05f;
            }

            return sum / ampTotal;
        }

        private static float ValueNoise3D(Vector3 p, int seed)
        {
            int x0 = Mathf.FloorToInt(p.x);
            int y0 = Mathf.FloorToInt(p.y);
            int z0 = Mathf.FloorToInt(p.z);

            float tx = p.x - x0;
            float ty = p.y - y0;
            float tz = p.z - z0;

            float sx = tx * tx * (3f - 2f * tx);
            float sy = ty * ty * (3f - 2f * ty);
            float sz = tz * tz * (3f - 2f * tz);

            float c000 = Hash3D(x0,     y0,     z0,     seed);
            float c100 = Hash3D(x0 + 1, y0,     z0,     seed);
            float c010 = Hash3D(x0,     y0 + 1, z0,     seed);
            float c110 = Hash3D(x0 + 1, y0 + 1, z0,     seed);
            float c001 = Hash3D(x0,     y0,     z0 + 1, seed);
            float c101 = Hash3D(x0 + 1, y0,     z0 + 1, seed);
            float c011 = Hash3D(x0,     y0 + 1, z0 + 1, seed);
            float c111 = Hash3D(x0 + 1, y0 + 1, z0 + 1, seed);

            float x00 = Mathf.Lerp(c000, c100, sx);
            float x10 = Mathf.Lerp(c010, c110, sx);
            float x01 = Mathf.Lerp(c001, c101, sx);
            float x11 = Mathf.Lerp(c011, c111, sx);

            float y0v = Mathf.Lerp(x00, x10, sy);
            float y1v = Mathf.Lerp(x01, x11, sy);

            return Mathf.Lerp(y0v, y1v, sz);
        }

        private static void NormalizeChannel(float[] data)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < min) min = data[i];
                if (data[i] > max) max = data[i];
            }

            float range = max - min;
            if (range < 0.0001f) return;

            float invRange = 1f / range;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (data[i] - min) * invRange;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private static float Hash1D(int x, int seed)
        {
            uint h = (uint)(x * 374761393 + seed * 668265263);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0x00FFFFFFu) / 16777215f;
        }

        private static float Hash2D(int x, int y, int seed)
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0x00FFFFFFu) / 16777215f;
        }

        private static float Hash3D(int x, int y, int z, int seed)
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + z * 2147483647 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0x00FFFFFFu) / 16777215f;
        }

        private static void EnsureDirectory(string assetFolderPath)
        {
            if (AssetDatabase.IsValidFolder(assetFolderPath)) return;

            string[] parts = assetFolderPath.Split('/');
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