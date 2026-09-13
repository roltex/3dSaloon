#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastBet.Saloon.Editor
{
    internal sealed class SaloonMaterials
    {
        public Material WoodFloor;
        public Material WoodWall;
        public Material WoodDark;
        public Material WoodTrim;
        public Material LeatherRed;
        public Material LeatherGreen;
        public Material FeltGreen;
        public Material FeltRed;
        public Material Brass;
        public Material Iron;
        public Material FabricRed;
        public Material Rug;
        public Material GlassDark;
        public Material GlassAmber;
        public Material EmissionBar;
        public Material EmissionJackpot;
        public Material LanternGlow;
        public Material DustFloor;
        public Material Card;
        public Material Plant;
        public Material Pot;
        public Material Rope;
        public Material Paper;
        public Material Mat;
        public Material Bone;
        public Material WindowGlow;
        public Material ChipRed;
        public Material ChipBlue;
        public Material ChipWhite;
        public Material SlotPurple;
        public Material Gold;
    }

    internal static class SaloonMaterialFactory
    {
        public static SaloonMaterials BuildAll()
        {
            SaloonAssetPaths.EnsureFolder(SaloonAssetPaths.Textures);
            SaloonAssetPaths.EnsureFolder(SaloonAssetPaths.Materials);

            Shader lit = FindLit();
            var catalog = new SaloonMaterials
            {
                WoodFloor = Lit(lit, "M_WoodFloor", new Color(0.38f, 0.24f, 0.13f), 0f, 0.32f, Wood(512, new Color(0.42f, 0.26f, 0.14f), new Color(0.22f, 0.12f, 0.06f), 36f), 8f, 4f),
                WoodWall = Lit(lit, "M_WoodWall", new Color(0.40f, 0.26f, 0.15f), 0f, 0.38f, Wood(512, new Color(0.48f, 0.30f, 0.16f), new Color(0.26f, 0.15f, 0.08f), 48f), 2.2f, 1.4f),
                WoodDark = Lit(lit, "M_WoodDark", new Color(0.28f, 0.16f, 0.09f), 0f, 0.28f, Wood(512, new Color(0.30f, 0.17f, 0.09f), new Color(0.14f, 0.07f, 0.04f), 28f), 1.6f, 1.2f),
                WoodTrim = Lit(lit, "M_WoodTrim", new Color(0.34f, 0.20f, 0.11f), 0f, 0.34f, Wood(256, new Color(0.36f, 0.21f, 0.11f), new Color(0.20f, 0.10f, 0.05f), 20f), 1.2f, 1.2f),
                LeatherRed = Lit(lit, "M_LeatherRed", new Color(0.45f, 0.10f, 0.10f), 0.05f, 0.42f, Leather(256, new Color(0.50f, 0.12f, 0.12f), new Color(0.28f, 0.05f, 0.05f))),
                LeatherGreen = Lit(lit, "M_LeatherGreen", new Color(0.12f, 0.22f, 0.14f), 0.05f, 0.40f, Leather(256, new Color(0.14f, 0.26f, 0.16f), new Color(0.06f, 0.12f, 0.08f))),
                FeltGreen = Lit(lit, "M_FeltGreen", new Color(0.10f, 0.32f, 0.16f), 0f, 0.18f, Felt(256, new Color(0.12f, 0.36f, 0.18f), new Color(0.07f, 0.22f, 0.11f))),
                FeltRed = Lit(lit, "M_FeltRed", new Color(0.42f, 0.08f, 0.10f), 0f, 0.18f, Felt(256, new Color(0.48f, 0.10f, 0.12f), new Color(0.28f, 0.04f, 0.06f))),
                Brass = Lit(lit, "M_Brass", new Color(0.72f, 0.55f, 0.22f), 0.85f, 0.62f, Metal(256, new Color(0.78f, 0.60f, 0.26f), new Color(0.40f, 0.28f, 0.10f))),
                Iron = Lit(lit, "M_Iron", new Color(0.18f, 0.17f, 0.16f), 0.75f, 0.35f, Metal(256, new Color(0.22f, 0.21f, 0.20f), new Color(0.08f, 0.08f, 0.07f))),
                FabricRed = Lit(lit, "M_FabricRed", new Color(0.48f, 0.07f, 0.10f), 0f, 0.22f, Felt(256, new Color(0.52f, 0.08f, 0.12f), new Color(0.30f, 0.03f, 0.05f))),
                Rug = Lit(lit, "M_Rug", new Color(0.46f, 0.10f, 0.12f), 0f, 0.20f, Rug(512)),
                GlassDark = Lit(lit, "M_GlassDark", new Color(0.06f, 0.12f, 0.08f, 0.85f), 0.1f, 0.85f, null, 1f, 1f, true),
                GlassAmber = Lit(lit, "M_GlassAmber", new Color(0.45f, 0.22f, 0.06f, 0.8f), 0.05f, 0.75f, null, 1f, 1f, true),
                EmissionBar = Emissive(lit, "M_EmissionBar", new Color(1f, 0.82f, 0.45f), 3.2f),
                EmissionJackpot = Emissive(lit, "M_EmissionJackpot", new Color(1f, 0.18f, 0.12f), 4.5f),
                LanternGlow = Emissive(lit, "M_LanternGlow", new Color(1f, 0.72f, 0.35f), 2.4f),
                DustFloor = Lit(lit, "M_DustFloor", new Color(0.55f, 0.42f, 0.28f), 0f, 0.22f, Dust(512)),
                Card = Lit(lit, "M_Card", new Color(0.92f, 0.88f, 0.80f), 0f, 0.45f, Paper(128)),
                Plant = Lit(lit, "M_Plant", new Color(0.18f, 0.38f, 0.16f), 0f, 0.28f, Felt(128, new Color(0.22f, 0.44f, 0.18f), new Color(0.10f, 0.22f, 0.08f))),
                Pot = Lit(lit, "M_Pot", new Color(0.28f, 0.16f, 0.10f), 0.05f, 0.30f, Leather(128, new Color(0.30f, 0.17f, 0.10f), new Color(0.16f, 0.08f, 0.05f))),
                Rope = Lit(lit, "M_Rope", new Color(0.62f, 0.50f, 0.32f), 0f, 0.25f, Felt(128, new Color(0.66f, 0.52f, 0.34f), new Color(0.40f, 0.30f, 0.16f))),
                Paper = Lit(lit, "M_Paper", new Color(0.86f, 0.78f, 0.62f), 0f, 0.35f, Paper(128)),
                Mat = Lit(lit, "M_Mat", new Color(0.16f, 0.12f, 0.08f), 0f, 0.16f, Felt(256, new Color(0.18f, 0.13f, 0.09f), new Color(0.08f, 0.06f, 0.04f))),
                Bone = Lit(lit, "M_Bone", new Color(0.86f, 0.80f, 0.68f), 0f, 0.38f, Paper(128)),
                WindowGlow = Emissive(lit, "M_WindowGlow", new Color(1f, 0.86f, 0.62f), 0.55f),
                ChipRed = Lit(lit, "M_ChipRed", new Color(0.70f, 0.10f, 0.10f), 0.05f, 0.40f),
                ChipBlue = Lit(lit, "M_ChipBlue", new Color(0.12f, 0.22f, 0.55f), 0.05f, 0.40f),
                ChipWhite = Lit(lit, "M_ChipWhite", new Color(0.90f, 0.88f, 0.82f), 0.05f, 0.42f),
                SlotPurple = Lit(lit, "M_SlotPurple", new Color(0.28f, 0.12f, 0.38f), 0.08f, 0.36f, Felt(128, new Color(0.32f, 0.14f, 0.42f), new Color(0.14f, 0.05f, 0.20f))),
                Gold = Lit(lit, "M_Gold", new Color(0.82f, 0.66f, 0.22f), 0.7f, 0.58f, Metal(128, new Color(0.88f, 0.70f, 0.24f), new Color(0.45f, 0.32f, 0.10f)))
            };

            AssetDatabase.SaveAssets();
            return catalog;
        }

        static Shader FindLit()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new System.InvalidOperationException("No URP Lit shader found. Is URP imported?");
            return shader;
        }

        static Material Lit(
            Shader shader,
            string name,
            Color color,
            float metallic,
            float smoothness,
            Texture2D albedo = null,
            float tileX = 1f,
            float tileY = 1f,
            bool transparent = false)
        {
            string path = $"{SaloonAssetPaths.Materials}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
                mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            }

            mat.shader = shader;
            mat.enableInstancing = true;
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            if (albedo != null)
            {
                mat.SetTexture("_BaseMap", albedo);
                mat.SetTexture("_MainTex", albedo);
            }

            mat.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_BumpScale", 1f);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_ZWrite", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)RenderQueue.Transparent;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                mat.SetFloat("_Surface", 0f);
                mat.SetFloat("_ZWrite", 1f);
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = (int)RenderQueue.Geometry;
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material Emissive(Shader shader, string name, Color color, float intensity)
        {
            var mat = Lit(shader, name, color, 0f, 0.2f);
            var emission = color * intensity;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Texture2D Wood(int size, Color a, Color b, float plank)
        {
            return SaveTex($"T_Wood_{ColorId(a)}_{size}", size, (x, y) =>
            {
                float groove = x % plank;
                float n = Mathf.PerlinNoise(x * 0.07f, y * 0.018f + Mathf.Floor(x / plank) * 13.7f);
                float grain = Mathf.PerlinNoise(x * 0.45f, y * 0.012f);
                Color c = Color.Lerp(a, b, n * 0.65f + grain * 0.35f);
                if (groove < 1.4f)
                    c *= 0.62f;
                float wear = Mathf.PerlinNoise(x * 0.03f + 20f, y * 0.03f);
                if (wear > 0.72f)
                    c = Color.Lerp(c, c * 0.7f, (wear - 0.72f) * 2f);
                return c;
            });
        }

        static Texture2D Leather(int size, Color a, Color b)
        {
            return SaveTex($"T_Leather_{ColorId(a)}_{size}", size, (x, y) =>
            {
                float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                float c2 = Mathf.PerlinNoise(x * 0.22f + 8f, y * 0.22f);
                Color c = Color.Lerp(a, b, n * 0.7f + c2 * 0.3f);
                if (c2 > 0.78f)
                    c *= 0.85f;
                return c;
            });
        }

        static Texture2D Felt(int size, Color a, Color b)
        {
            return SaveTex($"T_Felt_{ColorId(a)}_{size}", size, (x, y) =>
            {
                float n = Mathf.PerlinNoise(x * 0.35f, y * 0.35f);
                return Color.Lerp(a, b, n * 0.35f);
            });
        }

        static Texture2D Metal(int size, Color a, Color b)
        {
            return SaveTex($"T_Metal_{ColorId(a)}_{size}", size, (x, y) =>
            {
                float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                float scratch = Mathf.PerlinNoise(x * 0.8f, y * 0.05f);
                Color c = Color.Lerp(a, b, n);
                if (scratch > 0.82f)
                    c = Color.Lerp(c, Color.white, 0.12f);
                return c;
            });
        }

        static Texture2D Dust(int size)
        {
            return SaveTex("T_DustFloor", size, (x, y) =>
            {
                float n = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float dirt = Mathf.PerlinNoise(x * 0.11f + 4f, y * 0.11f);
                var a = new Color(0.60f, 0.46f, 0.30f);
                var b = new Color(0.38f, 0.26f, 0.16f);
                Color c = Color.Lerp(a, b, n);
                if (dirt > 0.62f)
                    c = Color.Lerp(c, new Color(0.28f, 0.18f, 0.10f), (dirt - 0.62f));
                return c;
            });
        }

        static Texture2D Paper(int size)
        {
            return SaveTex($"T_Paper_{size}", size, (x, y) =>
            {
                float n = Mathf.PerlinNoise(x * 0.2f, y * 0.2f);
                return Color.Lerp(new Color(0.93f, 0.89f, 0.80f), new Color(0.78f, 0.72f, 0.60f), n * 0.4f);
            });
        }

        static Texture2D Rug(int size)
        {
            return SaveTex("T_Rug", size, (x, y) =>
            {
                float u = x / (float)(size - 1);
                float v = y / (float)(size - 1);
                float dx = u - 0.5f;
                float dy = v - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);
                var field = new Color(0.42f, 0.08f, 0.10f);
                var gold = new Color(0.72f, 0.56f, 0.22f);
                float ring = Mathf.Abs(Mathf.Sin(r * 18f));
                float star = Mathf.Abs(Mathf.Cos(ang * 4f)) * (1f - Mathf.Clamp01(r * 2.2f));
                Color c = field;
                if (r > 0.46f)
                    c = gold;
                else if (ring > 0.82f)
                    c = Color.Lerp(field, gold, 0.7f);
                else
                    c = Color.Lerp(field, gold, star * 0.45f);
                float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                return Color.Lerp(c, c * 0.8f, n * 0.25f);
            });
        }

        static Texture2D SaveTex(string name, int size, System.Func<int, int, Color> fn)
        {
            string path = $"{SaloonAssetPaths.Textures}/{name}.png";
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = fn(x, y);
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 4;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static string ColorId(Color c) => $"{Mathf.RoundToInt(c.r * 99)}{Mathf.RoundToInt(c.g * 99)}{Mathf.RoundToInt(c.b * 99)}";
    }
}
#endif
