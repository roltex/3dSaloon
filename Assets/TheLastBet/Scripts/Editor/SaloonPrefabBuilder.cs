#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TheLastBet.Saloon.Editor.SaloonBuildUtility;

namespace TheLastBet.Saloon.Editor
{
    internal static class SaloonPrefabBuilder
    {
        static SaloonMaterials Mats;
        static GameObject Stool;
        static GameObject ChairRed;
        static GameObject ChairGreen;
        static GameObject Lantern;
        static GameObject Barrel;
        static GameObject Crate;
        static GameObject Plant;
        static GameObject ChipStack;

        public static GameObject Shell { get; private set; }
        public static GameObject Bar { get; private set; }
        public static GameObject Entrance { get; private set; }
        public static GameObject PokerGreen { get; private set; }
        public static GameObject PokerRed { get; private set; }
        public static GameObject Roulette { get; private set; }
        public static GameObject SlotArea { get; private set; }
        public static GameObject CentralLounge { get; private set; }
        public static GameObject DuelArena { get; private set; }
        public static GameObject Gallows { get; private set; }

        public static void BuildAll(SaloonMaterials materials)
        {
            SaloonMeshFactory.EnsureBuiltins();
            Mats = materials;
            SaloonAssetPaths.EnsureFolder(SaloonAssetPaths.Prefabs);
            SaloonAssetPaths.EnsureFolder(SaloonAssetPaths.PropPrefabs);

            Stool = SavePrefab(BuildStool(), SaloonAssetPaths.PropPrefabs, "PF_Prop_Stool");
            ChairRed = SavePrefab(BuildChair(Mats.LeatherRed), SaloonAssetPaths.PropPrefabs, "PF_Prop_ChairRed");
            ChairGreen = SavePrefab(BuildChair(Mats.LeatherGreen), SaloonAssetPaths.PropPrefabs, "PF_Prop_ChairGreen");
            Lantern = SavePrefab(BuildLantern(), SaloonAssetPaths.PropPrefabs, "PF_Prop_Lantern");
            Barrel = SavePrefab(BuildBarrel(), SaloonAssetPaths.PropPrefabs, "PF_Prop_Barrel");
            Crate = SavePrefab(BuildCrate(), SaloonAssetPaths.PropPrefabs, "PF_Prop_Crate");
            Plant = SavePrefab(BuildPlant(), SaloonAssetPaths.PropPrefabs, "PF_Prop_Plant");
            ChipStack = SavePrefab(BuildChipStack(), SaloonAssetPaths.PropPrefabs, "PF_Prop_ChipStack");

            Shell = SavePrefab(BuildShell(), SaloonAssetPaths.Prefabs, "PF_SaloonShell");
            Bar = SavePrefab(BuildBar(), SaloonAssetPaths.Prefabs, "PF_Bar");
            Entrance = SavePrefab(BuildEntrance(), SaloonAssetPaths.Prefabs, "PF_Entrance");
            PokerGreen = SavePrefab(BuildPokerGreen(), SaloonAssetPaths.Prefabs, "PF_PokerGreen");
            PokerRed = SavePrefab(BuildPokerRed(), SaloonAssetPaths.Prefabs, "PF_PokerRed");
            Roulette = SavePrefab(BuildRoulette(), SaloonAssetPaths.Prefabs, "PF_Roulette");
            SlotArea = SavePrefab(BuildSlotArea(), SaloonAssetPaths.Prefabs, "PF_SlotArea");
            CentralLounge = SavePrefab(BuildLounge(), SaloonAssetPaths.Prefabs, "PF_CentralLounge");
            DuelArena = SavePrefab(BuildDuel(), SaloonAssetPaths.Prefabs, "PF_DuelArena");
            Gallows = SavePrefab(BuildGallows(), SaloonAssetPaths.Prefabs, "PF_Gallows");
        }

        [MenuItem("The Last Bet/Rebuild Classic Shell")]
        public static void RebuildClassicShellInScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[The Last Bet] Stop Play Mode before rebuilding the classic shell.");
                return;
            }

            EnsureShellContext();
            Shell = SavePrefab(BuildShell(), SaloonAssetPaths.Prefabs, "PF_SaloonShell");
            var env = GameObject.Find("Environment");
            Transform parent = env != null ? env.transform : null;
            var old = GameObject.Find("SaloonShell");
            Vector3 pos = old != null ? old.transform.localPosition : Vector3.zero;
            Vector3 euler = old != null ? old.transform.localEulerAngles : Vector3.zero;
            int index = old != null ? old.transform.GetSiblingIndex() : 0;
            if (old != null)
            {
                parent = old.transform.parent;
                Object.DestroyImmediate(old);
            }

            var instance = InstantiatePrefab(Shell, parent, pos, euler);
            instance.name = "SaloonShell";
            instance.transform.SetSiblingIndex(index);

            var nav = GameObject.Find("Navigation");
            if (nav != null)
            {
                Transform floor = nav.transform.Find("WalkableFloor");
                if (floor != null)
                    floor.localScale = new Vector3(44f, 0.02f, 42f);
                var surface = nav.GetComponent<NavMeshSurface>();
                if (surface != null)
                    surface.BuildNavMesh();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[The Last Bet] Classic saloon shell expanded and saved.");
        }

        static void EnsureShellContext()
        {
            Mats = SaloonMaterialFactory.BuildAll();
            Lantern = AssetDatabase.LoadAssetAtPath<GameObject>($"{SaloonAssetPaths.PropPrefabs}/PF_Prop_Lantern.prefab");
            Barrel = AssetDatabase.LoadAssetAtPath<GameObject>($"{SaloonAssetPaths.PropPrefabs}/PF_Prop_Barrel.prefab");
            Plant = AssetDatabase.LoadAssetAtPath<GameObject>($"{SaloonAssetPaths.PropPrefabs}/PF_Prop_Plant.prefab");
            ChairRed = AssetDatabase.LoadAssetAtPath<GameObject>($"{SaloonAssetPaths.PropPrefabs}/PF_Prop_ChairRed.prefab");
            Crate = AssetDatabase.LoadAssetAtPath<GameObject>($"{SaloonAssetPaths.PropPrefabs}/PF_Prop_Crate.prefab");
        }

        [MenuItem("The Last Bet/Rebuild Bar")]
        public static void RebuildBarInScene() => RebuildAuthoredStations();

        [MenuItem("The Last Bet/Rebuild Authored Stations")]
        public static void RebuildAuthoredStations()
        {
            var leftover = GameObject.Find("_GlbMeasure");
            if (leftover != null)
                Object.DestroyImmediate(leftover);
            leftover = GameObject.Find("_BarGlbPreview");
            if (leftover != null)
                Object.DestroyImmediate(leftover);

            Bar = SavePrefab(BuildBar(), SaloonAssetPaths.Prefabs, "PF_Bar");
            Entrance = SavePrefab(BuildEntrance(), SaloonAssetPaths.Prefabs, "PF_Entrance");
            PokerGreen = SavePrefab(BuildPokerGreen(), SaloonAssetPaths.Prefabs, "PF_PokerGreen");
            PokerRed = SavePrefab(BuildPokerRed(), SaloonAssetPaths.Prefabs, "PF_PokerRed");
            Roulette = SavePrefab(BuildRoulette(), SaloonAssetPaths.Prefabs, "PF_Roulette");
            SlotArea = SavePrefab(BuildSlotArea(), SaloonAssetPaths.Prefabs, "PF_SlotArea");
            CentralLounge = SavePrefab(BuildLounge(), SaloonAssetPaths.Prefabs, "PF_CentralLounge");
            DuelArena = SavePrefab(BuildDuel(), SaloonAssetPaths.Prefabs, "PF_DuelArena");
            Gallows = SavePrefab(BuildGallows(), SaloonAssetPaths.Prefabs, "PF_Gallows");

            ReplaceStation("BarArea", Bar);
            ReplaceStation("Entrance", Entrance);
            ReplaceStation("GreenPoker", PokerGreen);
            ReplaceStation("RedPoker", PokerRed);
            ReplaceStation("Roulette", Roulette);
            ReplaceStation("SlotArea", SlotArea);
            ReplaceStation("CentralLounge", CentralLounge);
            ReplaceStation("DuelArena", DuelArena);
            ReplaceStation("Gallows", Gallows);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[The Last Bet] Authored station models rebuilt.");
        }

        static void ReplaceStation(string sceneName, GameObject prefab)
        {
            var old = GameObject.Find(sceneName);
            if (old == null || prefab == null)
                return;

            Transform parent = old.transform.parent;
            Vector3 pos = old.transform.localPosition;
            Vector3 euler = old.transform.localEulerAngles;
            int index = old.transform.GetSiblingIndex();
            Object.DestroyImmediate(old);
            var instance = InstantiatePrefab(prefab, parent, pos, euler);
            instance.name = sceneName;
            instance.transform.SetSiblingIndex(index);
        }

        static GameObject BuildStool()
        {
            var root = new GameObject("PF_Prop_Stool");
            var v = root.transform;
            Cyl("Seat", v, Mats.LeatherRed, new Vector3(0f, 0.62f, 0f), 0.17f, 0.045f);
            Cyl("Post", v, Mats.WoodDark, new Vector3(0f, 0.31f, 0f), 0.035f, 0.58f);
            Cyl("Ring", v, Mats.Brass, new Vector3(0f, 0.22f, 0f), 0.15f, 0.02f);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad;
                Cyl($"Foot_{i}", v, Mats.WoodDark, new Vector3(Mathf.Cos(a) * 0.13f, 0.04f, Mathf.Sin(a) * 0.13f), 0.025f, 0.08f);
            }

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.32f, 0f);
            col.radius = 0.18f;
            col.height = 0.64f;
            return root;
        }

        static GameObject BuildChair(Material leather)
        {
            var root = new GameObject(leather == Mats.LeatherGreen ? "PF_Prop_ChairGreen" : "PF_Prop_ChairRed");
            var v = root.transform;
            Box("Seat", v, leather, new Vector3(0f, 0.46f, 0f), new Vector3(0.48f, 0.06f, 0.48f));
            Box("Back", v, leather, new Vector3(0f, 0.74f, -0.21f), new Vector3(0.48f, 0.50f, 0.07f));
            Box("SeatFrame", v, Mats.WoodDark, new Vector3(0f, 0.42f, 0f), new Vector3(0.50f, 0.04f, 0.50f));
            foreach (var p in new[] { new Vector3(-0.2f, 0.21f, -0.2f), new Vector3(0.2f, 0.21f, -0.2f), new Vector3(-0.2f, 0.21f, 0.2f), new Vector3(0.2f, 0.21f, 0.2f) })
                Cyl(p.x > 0 ? "LegR" : "LegL", v, Mats.WoodDark, p, 0.03f, 0.42f);
            Box("ArmL", v, leather, new Vector3(-0.26f, 0.62f, 0f), new Vector3(0.06f, 0.08f, 0.42f));
            Box("ArmR", v, leather, new Vector3(0.26f, 0.62f, 0f), new Vector3(0.06f, 0.08f, 0.42f));
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.48f, 0f);
            col.size = new Vector3(0.56f, 0.96f, 0.56f);
            return root;
        }

        static GameObject BuildLantern()
        {
            var root = new GameObject("PF_Prop_Lantern");
            var v = root.transform;
            Box("Backplate", v, Mats.Iron, new Vector3(0f, 0f, 0.02f), new Vector3(0.12f, 0.18f, 0.03f));
            Cyl("Arm", v, Mats.Iron, new Vector3(0f, 0.02f, 0.12f), 0.015f, 0.18f, new Vector3(90f, 0f, 0f));
            Cyl("Housing", v, Mats.Iron, new Vector3(0f, -0.04f, 0.22f), 0.05f, 0.10f);
            Sphere("Bulb", v, Mats.LanternGlow, new Vector3(0f, -0.04f, 0.22f), 0.04f);
            Cyl("Cap", v, Mats.Iron, new Vector3(0f, 0.04f, 0.22f), 0.055f, 0.03f);
            return root;
        }

        static GameObject BuildBarrel()
        {
            var root = new GameObject("PF_Prop_Barrel");
            var v = root.transform;
            Cyl("Body", v, Mats.WoodDark, new Vector3(0f, 0.36f, 0f), 0.28f, 0.72f);
            Cyl("RingT", v, Mats.Iron, new Vector3(0f, 0.64f, 0f), 0.29f, 0.04f);
            Cyl("RingB", v, Mats.Iron, new Vector3(0f, 0.08f, 0f), 0.29f, 0.04f);
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.36f, 0f);
            col.radius = 0.29f;
            col.height = 0.74f;
            return root;
        }

        static GameObject BuildCrate()
        {
            var root = new GameObject("PF_Prop_Crate");
            Box("Box", root.transform, Mats.WoodTrim, new Vector3(0f, 0.22f, 0f), new Vector3(0.48f, 0.44f, 0.48f));
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.22f, 0f);
            col.size = new Vector3(0.48f, 0.44f, 0.48f);
            return root;
        }

        static GameObject BuildPlant()
        {
            var root = new GameObject("PF_Prop_Plant");
            var v = root.transform;
            Cyl("Pot", v, Mats.Pot, new Vector3(0f, 0.12f, 0f), 0.13f, 0.24f);
            Sphere("FoliageA", v, Mats.Plant, new Vector3(0f, 0.42f, 0f), 0.20f);
            Sphere("FoliageB", v, Mats.Plant, new Vector3(0.08f, 0.50f, 0.04f), 0.13f);
            Sphere("FoliageC", v, Mats.Plant, new Vector3(-0.07f, 0.48f, -0.05f), 0.12f);
            return root;
        }

        static GameObject BuildChipStack()
        {
            var root = new GameObject("PF_Prop_ChipStack");
            var mats = new[] { Mats.ChipRed, Mats.ChipWhite, Mats.ChipBlue };
            for (int i = 0; i < 5; i++)
                Cyl($"Chip_{i}", root.transform, mats[i % 3], new Vector3(0f, 0.008f + i * 0.012f, 0f), 0.028f, 0.01f);
            return root;
        }

        static GameObject BuildShell()
        {
            var root = PrefabRoot("PF_SaloonShell");
            var vis = Visual(root);
            var col = Collision(root);
            const float hx = 18f;
            const float hz = 14f;
            const float wallH = 3.4f;
            const float thick = 0.18f;
            const float partH = 1.25f;
            const float barGap = 7f;
            const float doorGap = 5f;
            const float floor2 = 3.4f;
            const float upperH = 2.8f;
            const float balcony = 2.5f;
            const float porchD = 6f;
            const float parlorD = 7f;

            Box("Floor", vis, Mats.WoodFloor, new Vector3(0f, -0.04f, 0f), new Vector3(hx * 2f, 0.08f, hz * 2f));
            CollisionBox(col, "FloorCol", new Vector3(0f, -0.04f, 0f), new Vector3(hx * 2f, 0.08f, hz * 2f));

            WallSegment(vis, col, "Wall_N_W", new Vector3((-hx - barGap * 0.5f) * 0.5f, wallH * 0.5f, hz), new Vector3(hx - barGap * 0.5f, wallH, thick));
            WallSegment(vis, col, "Wall_N_E", new Vector3((hx + barGap * 0.5f) * 0.5f, wallH * 0.5f, hz), new Vector3(hx - barGap * 0.5f, wallH, thick));
            WallSegment(vis, col, "Wall_S_W", new Vector3((-hx - doorGap * 0.5f) * 0.5f, wallH * 0.5f, -hz), new Vector3(hx - doorGap * 0.5f, wallH, thick));
            WallSegment(vis, col, "Wall_S_E", new Vector3((hx + doorGap * 0.5f) * 0.5f, wallH * 0.5f, -hz), new Vector3(hx - doorGap * 0.5f, wallH, thick));
            WallSegment(vis, col, "Wall_E", new Vector3(hx, wallH * 0.5f, 0f), new Vector3(thick, wallH, hz * 2f));

            const float stairGap = 2.6f;
            WallSegment(vis, col, "Wall_W_N", new Vector3(-hx, wallH * 0.5f, (hz + stairGap * 0.5f) * 0.5f), new Vector3(thick, wallH, hz - stairGap * 0.5f));
            WallSegment(vis, col, "Wall_W_S", new Vector3(-hx, wallH * 0.5f, (-hz - stairGap * 0.5f) * 0.5f), new Vector3(thick, wallH, hz - stairGap * 0.5f));

            WallSegment(vis, col, "Part_NW", new Vector3(-10f, partH * 0.5f, 5.2f), new Vector3(12f, partH, thick));
            WallSegment(vis, col, "Part_NE", new Vector3(10f, partH * 0.5f, 5.2f), new Vector3(12f, partH, thick));
            WallSegment(vis, col, "Part_SW_H", new Vector3(-9f, partH * 0.5f, -5.2f), new Vector3(12f, partH, thick));
            WallSegment(vis, col, "Part_SW_V", new Vector3(-3.6f, partH * 0.5f, -9.6f), new Vector3(thick, partH, 8.8f));
            WallSegment(vis, col, "Part_SE_H", new Vector3(9f, partH * 0.5f, -5.2f), new Vector3(12f, partH, thick));
            WallSegment(vis, col, "Part_SE_V", new Vector3(3.6f, partH * 0.5f, -9.6f), new Vector3(thick, partH, 8.8f));

            foreach (var p in new[]
                     {
                         new Vector3(-hx + 0.2f, 0f, hz - 0.2f), new Vector3(hx - 0.2f, 0f, hz - 0.2f),
                         new Vector3(-hx + 0.2f, 0f, -hz + 0.2f), new Vector3(hx - 0.2f, 0f, -hz + 0.2f),
                         new Vector3(-4.2f, 0f, -5.2f), new Vector3(4.2f, 0f, -5.2f),
                         new Vector3(-5.2f, 0f, 5.2f), new Vector3(5.2f, 0f, 5.2f),
                         new Vector3(-hx + 0.2f, 0f, 5.2f), new Vector3(hx - 0.2f, 0f, 5.2f),
                         new Vector3(0f, 0f, 0f), new Vector3(-8f, 0f, 0f), new Vector3(8f, 0f, 0f)
                     })
            {
                Cyl("Post", vis, Mats.WoodTrim, p + Vector3.up * (wallH * 0.5f), 0.1f, wallH);
            }

            Box("Beam_N", vis, Mats.WoodDark, new Vector3(0f, wallH - 0.12f, 5f), new Vector3(hx * 1.7f, 0.14f, 0.18f));
            Box("Beam_S", vis, Mats.WoodDark, new Vector3(0f, wallH - 0.12f, -5f), new Vector3(hx * 1.7f, 0.14f, 0.18f));
            Box("Beam_C", vis, Mats.WoodDark, new Vector3(0f, wallH - 0.12f, 0f), new Vector3(0.18f, 0.14f, hz * 1.6f));

            AddPorch(vis, col, hx, hz, porchD, doorGap, thick);
            AddParlor(vis, col, hx, hz, parlorD, barGap, thick, wallH);
            AddSecondFloor(vis, col, hx, hz, floor2, balcony, upperH, thick, stairGap);
            AddHotelWing(vis, col, hx, floor2, upperH, thick);
            AddStairRun(vis, col, -hx + 1.15f, 0.2f, floor2);

            Window(vis, new Vector3(-10.2f, 1.8f, hz - 0.02f));
            Window(vis, new Vector3(10.2f, 1.8f, hz - 0.02f));
            Window(vis, new Vector3(-hx + 0.02f, 1.8f, 8.4f), 90f);
            Window(vis, new Vector3(hx - 0.02f, 1.8f, 8.4f), -90f);
            Curtain(vis, new Vector3(9.4f, 1.65f, hz - 0.12f));
            Skull(vis, new Vector3(12.6f, 2.25f, hz - 0.14f));
            Box("Picture", vis, Mats.WoodTrim, new Vector3(11.2f, 2.15f, hz - 0.08f), new Vector3(0.55f, 0.4f, 0.04f));
            Box("PictureArt", vis, Mats.Paper, new Vector3(11.2f, 2.15f, hz - 0.06f), new Vector3(0.44f, 0.3f, 0.02f));

            PlaceLanterns(vis, new[]
            {
                new Vector3(-16.4f, 2.1f, 13.4f), new Vector3(16.4f, 2.1f, 13.4f),
                new Vector3(-16.4f, 2.1f, -13.4f), new Vector3(16.4f, 2.1f, -13.4f),
                new Vector3(-17.4f, 2.1f, 2.2f), new Vector3(17.4f, 2.1f, 2.2f),
                new Vector3(-8.2f, 1.7f, 5.05f), new Vector3(8.2f, 1.7f, 5.05f),
                new Vector3(-8.4f, 1.7f, -5.05f), new Vector3(8.4f, 1.7f, -5.05f),
                new Vector3(0f, 2.0f, -hz - porchD * 0.45f), new Vector3(0f, 2.0f, hz + parlorD * 0.4f)
            });

            PlacePlants(vis, new[]
            {
                new Vector3(-16.8f, 0f, 13.1f), new Vector3(16.8f, 0f, 13.1f),
                new Vector3(-16.8f, 0f, -13.1f), new Vector3(16.8f, 0f, -13.1f),
                new Vector3(-4.2f, 0f, 5.5f), new Vector3(4.2f, 0f, 5.5f),
                new Vector3(-4.2f, 0f, -4.8f), new Vector3(4.2f, 0f, -4.8f)
            });

            if (Barrel != null)
            {
                InstantiatePrefab(Barrel, vis, new Vector3(-16.6f, 0f, 10.4f));
                InstantiatePrefab(Barrel, vis, new Vector3(16.6f, 0f, 10.4f));
                InstantiatePrefab(Barrel, vis, new Vector3(-16.6f, 0f, -10.2f), new Vector3(0f, 20f, 0f));
                InstantiatePrefab(Barrel, vis, new Vector3(16.6f, 0f, -10.2f));
                InstantiatePrefab(Barrel, vis, new Vector3(-2.2f, 0f, -hz - porchD * 0.35f));
            }

            PointLight(vis, "HeroLantern_W", new Vector3(-14.4f, 2.2f, 0.4f), 9.5f, 3.4f, 2600f);
            PointLight(vis, "HeroLantern_E", new Vector3(14.4f, 2.2f, 0.4f), 9.5f, 3.4f, 2600f);
            PointLight(vis, "UpperLamp", new Vector3(0f, floor2 + 2.2f, 0f), 12f, 2.6f, 2550f);
            return root;
        }

        static void AddPorch(Transform vis, Transform col, float hx, float hz, float depth, float doorGap, float thick)
        {
            float z = -hz - depth * 0.5f;
            Box("PorchFloor", vis, Mats.WoodFloor, new Vector3(0f, -0.04f, z), new Vector3(doorGap + 8f, 0.08f, depth));
            CollisionBox(col, "PorchFloorCol", new Vector3(0f, -0.04f, z), new Vector3(doorGap + 8f, 0.08f, depth));
            WallSegment(vis, col, "Porch_W", new Vector3(-(doorGap * 0.5f + 4f), 1.1f, z), new Vector3(thick, 2.2f, depth));
            WallSegment(vis, col, "Porch_E", new Vector3(doorGap * 0.5f + 4f, 1.1f, z), new Vector3(thick, 2.2f, depth));
            for (int i = -2; i <= 2; i++)
                Cyl("PorchPost", vis, Mats.WoodTrim, new Vector3(i * 2.1f, 1.35f, -hz - depth + 0.18f), 0.09f, 2.7f);
            Box("PorchRoof", vis, Mats.WoodDark, new Vector3(0f, 2.75f, z), new Vector3(doorGap + 8.2f, 0.1f, depth + 0.2f));
        }

        static void AddParlor(Transform vis, Transform col, float hx, float hz, float depth, float barGap, float thick, float wallH)
        {
            float z = hz + depth * 0.5f;
            Box("ParlorFloor", vis, Mats.WoodFloor, new Vector3(0f, -0.04f, z), new Vector3(barGap + 6f, 0.08f, depth));
            CollisionBox(col, "ParlorFloorCol", new Vector3(0f, -0.04f, z), new Vector3(barGap + 6f, 0.08f, depth));
            WallSegment(vis, col, "Parlor_N", new Vector3(0f, wallH * 0.5f, hz + depth), new Vector3(barGap + 6f, wallH, thick));
            WallSegment(vis, col, "Parlor_W", new Vector3(-(barGap * 0.5f + 3f), wallH * 0.5f, z), new Vector3(thick, wallH, depth));
            WallSegment(vis, col, "Parlor_E", new Vector3(barGap * 0.5f + 3f, wallH * 0.5f, z), new Vector3(thick, wallH, depth));
            Box("ParlorRug", vis, Mats.Rug, new Vector3(0f, 0.01f, z), new Vector3(5.2f, 0.02f, 4.4f));
            if (ChairRed != null)
            {
                InstantiatePrefab(ChairRed, vis, new Vector3(-1.6f, 0f, z + 0.6f), new Vector3(0f, 180f, 0f));
                InstantiatePrefab(ChairRed, vis, new Vector3(1.6f, 0f, z + 0.6f), new Vector3(0f, 180f, 0f));
            }
        }

        static void AddSecondFloor(Transform vis, Transform col, float hx, float hz, float floor2, float balcony, float upperH, float thick, float stairGap)
        {
            float y = floor2;
            Box("Balcony_N", vis, Mats.WoodFloor, new Vector3(0f, y, hz - balcony * 0.5f), new Vector3(hx * 2f, 0.14f, balcony));
            CollisionBox(col, "Balcony_NCol", new Vector3(0f, y, hz - balcony * 0.5f), new Vector3(hx * 2f, 0.14f, balcony));
            Box("Balcony_S", vis, Mats.WoodFloor, new Vector3(0f, y, -hz + balcony * 0.5f), new Vector3(hx * 2f, 0.14f, balcony));
            CollisionBox(col, "Balcony_SCol", new Vector3(0f, y, -hz + balcony * 0.5f), new Vector3(hx * 2f, 0.14f, balcony));
            Box("Balcony_E", vis, Mats.WoodFloor, new Vector3(hx - balcony * 0.5f, y, 0f), new Vector3(balcony, 0.14f, hz * 2f - balcony * 2f));
            CollisionBox(col, "Balcony_ECol", new Vector3(hx - balcony * 0.5f, y, 0f), new Vector3(balcony, 0.14f, hz * 2f - balcony * 2f));
            float westLen = hz - balcony - stairGap * 0.5f;
            Box("Balcony_WN", vis, Mats.WoodFloor, new Vector3(-hx + balcony * 0.5f, y, (hz - balcony - stairGap * 0.5f) * 0.5f), new Vector3(balcony, 0.14f, westLen));
            CollisionBox(col, "Balcony_WNCol", new Vector3(-hx + balcony * 0.5f, y, (hz - balcony - stairGap * 0.5f) * 0.5f), new Vector3(balcony, 0.14f, westLen));
            Box("Balcony_WS", vis, Mats.WoodFloor, new Vector3(-hx + balcony * 0.5f, y, (-hz + balcony + stairGap * 0.5f) * 0.5f), new Vector3(balcony, 0.14f, westLen));
            CollisionBox(col, "Balcony_WSCol", new Vector3(-hx + balcony * 0.5f, y, (-hz + balcony + stairGap * 0.5f) * 0.5f), new Vector3(balcony, 0.14f, westLen));

            float railY = y + 0.85f;
            CollisionBox(col, "Rail_N", new Vector3(0f, railY, hz - balcony), new Vector3(hx * 2f - 1f, 1.5f, 0.12f));
            CollisionBox(col, "Rail_S", new Vector3(0f, railY, -hz + balcony), new Vector3(hx * 2f - 1f, 1.5f, 0.12f));
            CollisionBox(col, "Rail_E", new Vector3(hx - balcony, railY, 0f), new Vector3(0.12f, 1.5f, hz * 2f - balcony * 2f));
            Box("RailVis_N", vis, Mats.WoodTrim, new Vector3(0f, railY, hz - balcony), new Vector3(hx * 2f - 1f, 0.08f, 0.08f));
            Box("RailVis_S", vis, Mats.WoodTrim, new Vector3(0f, railY, -hz + balcony), new Vector3(hx * 2f - 1f, 0.08f, 0.08f));

            WallSegment(vis, col, "Upper_N", new Vector3(0f, y + upperH * 0.5f, hz), new Vector3(hx * 2f, upperH, thick));
            WallSegment(vis, col, "Upper_S", new Vector3(0f, y + upperH * 0.5f, -hz), new Vector3(hx * 2f, upperH, thick));
            WallSegment(vis, col, "Upper_E", new Vector3(hx, y + upperH * 0.5f, 0f), new Vector3(thick, upperH, hz * 2f));
            WallSegment(vis, col, "Upper_W_N", new Vector3(-hx, y + upperH * 0.5f, 7f), new Vector3(thick, upperH, 10f));
            WallSegment(vis, col, "Upper_W_S", new Vector3(-hx, y + upperH * 0.5f, -7f), new Vector3(thick, upperH, 10f));
        }

        static void AddHotelWing(Transform vis, Transform col, float hx, float floor2, float upperH, float thick)
        {
            float x = -hx - 3.4f;
            Box("HotelYard", vis, Mats.DustFloor != null ? Mats.DustFloor : Mats.WoodFloor, new Vector3(x, -0.04f, 0f), new Vector3(6.8f, 0.08f, 16f));
            CollisionBox(col, "HotelYardCol", new Vector3(x, -0.04f, 0f), new Vector3(6.8f, 0.08f, 16f));
            Box("HotelFloor", vis, Mats.WoodFloor, new Vector3(x, floor2, 0f), new Vector3(6.8f, 0.14f, 16f));
            CollisionBox(col, "HotelFloorCol", new Vector3(x, floor2, 0f), new Vector3(6.8f, 0.14f, 16f));
            WallSegment(vis, col, "Hotel_W", new Vector3(x - 3.4f, floor2 + upperH * 0.5f, 0f), new Vector3(thick, upperH, 16f));
            WallSegment(vis, col, "Hotel_N", new Vector3(x, floor2 + upperH * 0.5f, 8f), new Vector3(6.8f, upperH, thick));
            WallSegment(vis, col, "Hotel_S", new Vector3(x, floor2 + upperH * 0.5f, -8f), new Vector3(6.8f, upperH, thick));
            WallSegment(vis, col, "Hotel_Split", new Vector3(x, floor2 + upperH * 0.5f, 0f), new Vector3(6.8f, upperH, thick));
            Box("BedA", vis, Mats.FabricRed, new Vector3(x - 1.4f, floor2 + 0.28f, 4.2f), new Vector3(1.8f, 0.32f, 2.2f));
            CollisionBox(col, "BedACol", new Vector3(x - 1.4f, floor2 + 0.28f, 4.2f), new Vector3(1.8f, 0.32f, 2.2f));
            Box("BedB", vis, Mats.FabricRed, new Vector3(x - 1.4f, floor2 + 0.28f, -4.2f), new Vector3(1.8f, 0.32f, 2.2f));
            CollisionBox(col, "BedBCol", new Vector3(x - 1.4f, floor2 + 0.28f, -4.2f), new Vector3(1.8f, 0.32f, 2.2f));
            if (Crate != null)
            {
                InstantiatePrefab(Crate, vis, new Vector3(x + 1.6f, floor2 + 0.02f, 5.4f));
                InstantiatePrefab(Crate, vis, new Vector3(x + 1.6f, floor2 + 0.02f, -5.4f));
            }
        }

        static void AddStairRun(Transform vis, Transform col, float x, float z, float floor2)
        {
            const int steps = 12;
            float rise = floor2 / steps;
            float run = 0.32f;
            for (int i = 0; i < steps; i++)
            {
                Vector3 center = new Vector3(x, rise * (i + 0.5f), z + run * i);
                Vector3 size = new Vector3(1.7f, rise + 0.02f, run + 0.04f);
                Box("Step_" + i, vis, Mats.WoodDark, center, size);
                CollisionBox(col, "StepCol_" + i, center, size);
            }
        }

        static void WallSegment(Transform vis, Transform col, string name, Vector3 center, Vector3 size)
        {
            Box(name, vis, Mats.WoodWall, center, size);
            Box(name + "_Base", vis, Mats.WoodTrim, center + new Vector3(0f, -size.y * 0.5f + 0.08f, 0f), new Vector3(size.x + 0.02f, 0.16f, size.z + 0.02f));
            CollisionBox(col, name + "Col", center, size);
        }

        static void Window(Transform vis, Vector3 pos, float yaw = 0f)
        {
            var go = Create("Window", vis);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box("Frame", go.transform, Mats.WoodTrim, Vector3.zero, new Vector3(1.15f, 1.25f, 0.08f));
            Box("Glass", go.transform, Mats.WindowGlow, new Vector3(0f, 0f, 0.01f), new Vector3(0.95f, 1.05f, 0.02f));
            Box("MullionV", go.transform, Mats.WoodTrim, Vector3.zero, new Vector3(0.05f, 1.05f, 0.04f));
            Box("MullionH", go.transform, Mats.WoodTrim, Vector3.zero, new Vector3(0.95f, 0.05f, 0.04f));
        }

        static void Curtain(Transform vis, Vector3 pos)
        {
            Box("CurtainL", vis, Mats.FabricRed, pos + new Vector3(-0.28f, 0f, 0f), new Vector3(0.28f, 2.1f, 0.06f));
            Box("CurtainR", vis, Mats.FabricRed, pos + new Vector3(0.28f, 0f, 0f), new Vector3(0.28f, 2.1f, 0.06f));
            Cyl("Tie", vis, Mats.Gold, pos + new Vector3(0.28f, -0.15f, 0.04f), 0.02f, 0.08f, new Vector3(0f, 0f, 90f));
        }

        static void Skull(Transform vis, Vector3 pos)
        {
            Sphere("Skull", vis, Mats.Bone, pos, 0.11f);
            Cyl("HornL", vis, Mats.Bone, pos + new Vector3(-0.16f, 0.08f, 0f), 0.025f, 0.22f, new Vector3(0f, 0f, 50f));
            Cyl("HornR", vis, Mats.Bone, pos + new Vector3(0.16f, 0.08f, 0f), 0.025f, 0.22f, new Vector3(0f, 0f, -50f));
        }

        static void PlaceLanterns(Transform vis, IEnumerable<Vector3> positions)
        {
            if (Lantern == null)
                return;
            int i = 0;
            foreach (Vector3 p in positions)
                InstantiatePrefab(Lantern, vis, p, new Vector3(0f, i++ % 2 == 0 ? 0f : 180f, 0f));
        }

        static void PlacePlants(Transform vis, IEnumerable<Vector3> positions)
        {
            if (Plant == null)
                return;
            foreach (Vector3 p in positions)
                InstantiatePrefab(Plant, vis, p);
        }

        static GameObject BuildBar()
        {
            var root = PrefabRoot("PF_Bar");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);

            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.BarCounterModel, "BarCounter", 6.6f, AuthoredFit.MaxZ, 1.52f, new Vector3(0f, 180f, 0f));
            BindFrontStation(root, vis, col, anc, model, "bar", "Bar", SaloonStationType.Bar, "bar", "BarLight", 8.5f, 5.4f, 2500f, 2.1f, 2.6f, true, 0, 0.45f, true);
            return root;
        }

        static GameObject BuildEntrance()
        {
            var root = PrefabRoot("PF_Entrance");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);

            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.EntranceModel, "EntranceGate", 4.8f, AuthoredFit.MinZ, -1.05f, new Vector3(0f, 180f, 0f));
            var bounds = EncapsulateRenderers(model);
            var trigger = TriggerSphere(col, "Trigger", new Vector3(0f, 0.9f, bounds.max.z + 0.35f), 2.0f);
            Transform player = Anchor(anc, "Anchor_Player", new Vector3(0f, 0.4f, bounds.max.z + 0.2f), Vector3.back);
            Transform camera = Anchor(anc, "Anchor_Camera", new Vector3(0.2f, 3.2f, bounds.max.z + 3.8f), new Vector3(0f, -2.8f, -5.2f));
            Transform audio = Anchor(anc, "Anchor_Audio", new Vector3(0f, 1.6f, bounds.center.z), Vector3.forward);
            MarkCamera(camera, "entrance", 44f);
            PointLight(vis, "EntranceLight", new Vector3(0f, 2.3f, 0f), 6.5f, 3.6f, 2650f);
            BindStation(root, "entrance", "Entrance", SaloonStationType.Entrance, player, camera, audio, trigger, null, null, 2.3f);
            return root;
        }

        static GameObject BuildPokerGreen()
        {
            var root = PrefabRoot("PF_PokerGreen");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);
            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.OvalPokerModel, "OvalPoker", 4.2f);
            BindFrontStation(root, vis, col, anc, model, "poker-green", "Green Poker", SaloonStationType.PokerGreen, "poker-green", "TableLight", 5.5f, 2.4f, 2800f, 2.6f, 2.8f, true, 2, 0.2f, false);
            return root;
        }

        static GameObject BuildPokerRed()
        {
            var root = PrefabRoot("PF_PokerRed");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);
            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.RoundPokerModel, "RoundPoker", 3.6f);
            BindFrontStation(root, vis, col, anc, model, "poker-red", "Red Card Table", SaloonStationType.PokerRed, "poker-red", "TableLight", 5.2f, 2.3f, 2800f, 2.5f, 2.7f, true, 0, 0.2f, false);
            return root;
        }

        static GameObject BuildRoulette()
        {
            var root = PrefabRoot("PF_Roulette");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);
            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.RouletteModel, "RouletteTable", 4.2f);
            var bounds = EncapsulateRenderers(model);
            var wheel = Create("Wheel", vis);
            wheel.transform.localPosition = new Vector3(bounds.min.x + bounds.size.x * 0.28f, bounds.size.y * 0.72f, 0f);
            BindFrontStation(root, vis, col, anc, model, "roulette", "Roulette", SaloonStationType.Roulette, "roulette", "RouletteLight", 5.0f, 2.1f, 2750f, 2.5f, 2.6f, false, 0, 0.2f, false);
            return root;
        }

        static GameObject BuildSlotArea()
        {
            var root = PrefabRoot("PF_SlotArea");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);

            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.SlotWallModel, "SlotWall", 4.1f, AuthoredFit.MaxZ, 0.62f, new Vector3(0f, 180f, 0f));
            var bounds = EncapsulateRenderers(model);
            float front = bounds.min.z;
            float[] xs = { -bounds.size.x * 0.28f, 0f, bounds.size.x * 0.28f };
            var specs = new List<Transform>();
            for (int i = 0; i < 3; i++)
            {
                var machine = Create($"Slot_{i}", vis);
                machine.transform.localPosition = new Vector3(xs[i], 0f, front + 0.35f);
                var mCol = machine.AddComponent<BoxCollider>();
                mCol.center = new Vector3(0f, 0.85f, 0.1f);
                mCol.size = new Vector3(0.72f, 1.7f, 0.55f);
                var mTrig = machine.AddComponent<SphereCollider>();
                mTrig.isTrigger = true;
                mTrig.center = new Vector3(0f, 0.8f, -0.55f);
                mTrig.radius = 0.85f;
                Transform mPlayer = Anchor(machine.transform, "Anchor_Player", new Vector3(0f, 0f, -0.85f), Vector3.forward);
                Transform mCam = Anchor(machine.transform, "Anchor_Camera", new Vector3(0f, 1.6f, -1.6f), new Vector3(0f, -0.6f, 1.7f));
                Transform mAud = Anchor(machine.transform, "Anchor_Audio", new Vector3(0f, 1.1f, 0.1f), Vector3.forward);
                MarkCamera(mCam, $"slots-{i}", 38f, 0.45f);
                BindStation(machine, $"slots-{i}", $"Slot Machine {i + 1}", SaloonStationType.Slots, mPlayer, mCam, mAud, mTrig, null, null, 1.4f);
            }

            var trigger = TriggerSphere(col, "Trigger", new Vector3(0f, 0.9f, front - 0.35f), 2.3f);
            Transform player = Anchor(anc, "Anchor_Player", new Vector3(0f, 0f, front - 0.45f), Vector3.forward);
            Transform camera = Anchor(anc, "Anchor_Camera", new Vector3(0.2f, 2.8f, front - 2.8f), new Vector3(0f, -2.2f, 3.5f));
            Transform audio = Anchor(anc, "Anchor_Audio", new Vector3(0f, 2.2f, bounds.center.z), Vector3.forward);
            specs.Add(Anchor(anc, "Anchor_Spectator_01", new Vector3(bounds.min.x - 0.2f, 0f, front), Vector3.right));
            MarkCamera(camera, "slots", 42f);
            PointLight(vis, "JackpotLight", new Vector3(0f, 2.2f, 0.1f), 6.0f, 3.8f, 2400f);
            BindStation(root, "slots", "Slot Area", SaloonStationType.Slots, player, camera, audio, trigger, null, specs, 2.5f);
            return root;
        }

        static GameObject BuildLounge()
        {
            var root = PrefabRoot("PF_CentralLounge");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);
            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.LoungeModel, "LoungeArea", 5.0f);
            BindFrontStation(root, vis, col, anc, model, "central-lounge", "Central Lounge", SaloonStationType.CentralLounge, "central-lounge", "LoungeLight", 6.8f, 4.2f, 2550f, 2.6f, 2.8f, true, 2, 0.2f, true);
            return root;
        }

        static GameObject BuildDuel()
        {
            var root = PrefabRoot("PF_DuelArena");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);

            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.DuelArenaModel, "DuelRing", 6.0f, AuthoredFit.Center, 0f, new Vector3(0f, 180f, 0f));
            var bounds = EncapsulateRenderers(model);
            var trigger = TriggerSphere(col, "Trigger", Vector3.zero, 3.0f);

            float hx = bounds.size.x * 0.22f;
            float hz = bounds.size.z * 0.22f;
            Transform playerA = Anchor(anc, "Duel_PlayerA", new Vector3(-hx, 0f, 0f), Vector3.right);
            Transform playerB = Anchor(anc, "Duel_PlayerB", new Vector3(hx, 0f, 0f), Vector3.left);
            Transform intro = Anchor(anc, "Duel_Camera_Intro", new Vector3(0f, 3.6f, bounds.min.z - 1.4f), new Vector3(0f, -3.2f, 5.2f));
            Transform gameplay = Anchor(anc, "Duel_Camera_Gameplay", new Vector3(0.15f, 2.4f, bounds.min.z - 0.6f), new Vector3(0f, -1.6f, 4.1f));
            Transform resultA = Anchor(anc, "Duel_Camera_ResultA", new Vector3(bounds.min.x - 0.15f, 1.7f, bounds.min.z + 0.4f), new Vector3(2.6f, -0.9f, 1.4f));
            Transform resultB = Anchor(anc, "Duel_Camera_ResultB", new Vector3(bounds.max.x + 0.15f, 1.7f, bounds.min.z + 0.4f), new Vector3(-2.6f, -0.9f, 1.4f));
            var specs = new[]
            {
                Anchor(anc, "Anchor_Spectator_01", new Vector3(bounds.min.x + 0.15f, 0f, bounds.min.z + 0.2f), new Vector3(1f, 0f, 1f)),
                Anchor(anc, "Anchor_Spectator_02", new Vector3(bounds.max.x - 0.15f, 0f, bounds.min.z + 0.2f), new Vector3(-1f, 0f, 1f)),
                Anchor(anc, "Anchor_Spectator_03", new Vector3(0f, 0f, bounds.max.z - 0.15f), Vector3.back)
            };
            MarkCamera(intro, "duel-intro", 40f, 0.8f);
            MarkCamera(gameplay, "duel-gameplay", 38f, 0.55f);
            MarkCamera(resultA, "duel-result-a", 36f, 0.5f);
            MarkCamera(resultB, "duel-result-b", 36f, 0.5f);
            BindDuelAnchors(root, playerA, playerB, intro, gameplay, resultA, resultB, specs);

            Transform player = Anchor(anc, "Anchor_Player", new Vector3(-hx, 0f, 0f), Vector3.right);
            Transform camera = Anchor(anc, "Anchor_Camera", gameplay.localPosition, gameplay.forward);
            camera.localRotation = gameplay.localRotation;
            Transform audio = Anchor(anc, "Anchor_Audio", new Vector3(0f, 1.4f, 0f), Vector3.forward);
            MarkCamera(camera, "duel-arena", 40f);
            PointLight(vis, "DuelSignLight", new Vector3(0f, 2.0f, bounds.max.z * 0.6f), 6.2f, 3.5f, 2450f);
            BindStation(root, "duel-arena", "Duel Arena", SaloonStationType.DuelArena, player, camera, audio, trigger, playerB, specs, 3.2f);
            _ = hz;
            return root;
        }

        static GameObject BuildGallows()
        {
            var root = PrefabRoot("PF_Gallows");
            var vis = Visual(root);
            var col = Collision(root);
            var anc = Anchors(root);

            var model = PlaceAuthoredModel(vis, SaloonAssetPaths.GallowsModel, "Gallows", 4.2f, AuthoredFit.Center, 0f, new Vector3(0f, 180f, 0f));
            var bounds = EncapsulateRenderers(model);
            var noose = Create("Noose", vis);
            noose.transform.localPosition = new Vector3(0f, bounds.size.y * 0.72f, bounds.center.z);

            var trigger = TriggerSphere(col, "Trigger", new Vector3(0f, 0.5f, bounds.min.z - 0.2f), 1.8f);
            Transform player = Anchor(anc, "Anchor_Player", new Vector3(0f, 0f, bounds.min.z - 0.35f), Vector3.forward);
            Transform camera = Anchor(anc, "Anchor_Camera", new Vector3(-0.2f, 3.4f, bounds.min.z - 2.2f), new Vector3(0.2f, -2.4f, 4.0f));
            Transform audio = Anchor(anc, "Anchor_Audio", new Vector3(0f, 2.2f, bounds.center.z), Vector3.forward);
            var specs = new List<Transform>
            {
                Anchor(anc, "Anchor_Spectator_01", new Vector3(bounds.min.x - 0.15f, 0f, bounds.min.z), new Vector3(1f, 0f, 1f)),
                Anchor(anc, "Anchor_Spectator_02", new Vector3(bounds.max.x + 0.15f, 0f, bounds.min.z), new Vector3(-1f, 0f, 1f))
            };
            MarkCamera(camera, "gallows", 38f, 0.7f);
            PointLight(vis, "GallowsLight", new Vector3(0f, 2.6f, 0.2f), 5.5f, 2.6f, 2550f);
            BindStation(root, "gallows", "Gallows", SaloonStationType.Gallows, player, camera, audio, trigger, null, specs, 2.4f);
            return root;
        }

        static void BindFrontStation(
            GameObject root,
            Transform vis,
            Transform col,
            Transform anc,
            GameObject model,
            string id,
            string display,
            SaloonStationType type,
            string cameraId,
            string lightName,
            float lightRange,
            float lightIntensity,
            float kelvin,
            float triggerRadius,
            float interactRadius,
            bool opponent,
            int spectators,
            float playerInset,
            bool softLight)
        {
            var bounds = EncapsulateRenderers(model);
            float front = bounds.min.z;
            var trigger = TriggerSphere(col, "Trigger", new Vector3(0f, 0.9f, front - 0.1f), triggerRadius);
            Transform player = Anchor(anc, "Anchor_Player", new Vector3(0f, 0f, front - playerInset), Vector3.forward);
            Transform camera = Anchor(anc, "Anchor_Camera", new Vector3(0.3f, 3.5f, front - 2.6f), new Vector3(0f, -3.1f, 3.6f));
            Transform audio = Anchor(anc, "Anchor_Audio", new Vector3(0f, 1.2f, bounds.center.z), Vector3.forward);
            Transform opponentT = opponent
                ? Anchor(anc, "Anchor_Opponent", new Vector3(0f, 0f, bounds.max.z + 0.1f), Vector3.back)
                : null;
            List<Transform> specs = null;
            if (spectators > 0)
            {
                specs = new List<Transform>();
                specs.Add(Anchor(anc, "Anchor_Spectator_01", new Vector3(bounds.min.x - 0.2f, 0f, 0f), Vector3.right));
                if (spectators > 1)
                    specs.Add(Anchor(anc, "Anchor_Spectator_02", new Vector3(bounds.max.x + 0.2f, 0f, 0f), Vector3.left));
            }

            MarkCamera(camera, cameraId, 40f);
            var light = PointLight(vis, lightName, new Vector3(0f, Mathf.Max(2.2f, bounds.max.y + 0.15f), 0f), lightRange, lightIntensity, kelvin);
            if (softLight)
                light.shadows = LightShadows.Soft;
            BindStation(root, id, display, type, player, camera, audio, trigger, opponentT, specs, interactRadius);
        }
    }
}
#endif
