#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TheLastBet.Saloon.Editor.SaloonBuildUtility;

namespace TheLastBet.Saloon.Editor
{
    internal static class SaloonSceneAssembler
    {
        [MenuItem("The Last Bet/Build Production Saloon")]
        public static void BuildProductionSaloon()
        {
            SaloonAssetPaths.EnsureParentFolders();
            SaloonMeshFactory.BuildCore();
            SaloonMaterials materials = SaloonMaterialFactory.BuildAll();
            AssetDatabase.SaveAssets();

            SaloonPrefabBuilder.BuildAll(materials);
            CharacterPrefabBuilder.BuildAssets();
            AssembleScene();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[The Last Bet] Production saloon built and saved.");
        }

        public static void AssembleScene()
        {
            EditorSceneManager.SaveOpenScenes();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, SaloonAssetPaths.Scene);
            scene = EditorSceneManager.OpenScene(SaloonAssetPaths.Scene, OpenSceneMode.Single);

            ApplyEnvironmentSettings();

            var environment = Create("Environment", (Transform)null);
            var stations = Create("GameplayStations", (Transform)null);
            var navigation = Create("Navigation", (Transform)null);
            var spawns = Create("CharacterSpawns", (Transform)null);
            var cameraRig = Create("CameraRig", (Transform)null);
            var gameplay = Create("Gameplay", (Transform)null);
            var ui = Create("UI", (Transform)null);

            var lighting = Create("Lighting", environment);
            var probes = Create("ReflectionProbes", environment);
            var audio = Create("Audio", environment);

            var shell = InstantiatePrefab(SaloonPrefabBuilder.Shell, environment.transform, Vector3.zero);
            shell.name = "SaloonShell";

            PlaceStation(stations, SaloonPrefabBuilder.Bar, "BarArea", new Vector3(0f, 0f, 9.45f));
            PlaceStation(stations, SaloonPrefabBuilder.PokerGreen, "GreenPoker", new Vector3(-7.2f, 0f, 7.35f));
            PlaceStation(stations, SaloonPrefabBuilder.PokerRed, "RedPoker", new Vector3(7.2f, 0f, 7.35f));
            PlaceStation(stations, SaloonPrefabBuilder.Roulette, "Roulette", new Vector3(-7.0f, 0f, 0.85f));
            PlaceStation(stations, SaloonPrefabBuilder.CentralLounge, "CentralLounge", new Vector3(0f, 0f, 0.35f));
            PlaceStation(stations, SaloonPrefabBuilder.SlotArea, "SlotArea", new Vector3(10.85f, 0f, 0.9f), new Vector3(0f, 90f, 0f));
            PlaceStation(stations, SaloonPrefabBuilder.DuelArena, "DuelArena", new Vector3(-6.55f, 0f, -7.15f));
            PlaceStation(stations, SaloonPrefabBuilder.Gallows, "Gallows", new Vector3(6.55f, 0f, -7.15f), new Vector3(0f, 180f, 0f));
            PlaceStation(stations, SaloonPrefabBuilder.Entrance, "Entrance", new Vector3(0f, 0f, -10.0f));

            BuildLighting(lighting.transform);
            BuildProbe(probes.transform);
            BuildAudio(audio.transform);
            BuildCameraRig(cameraRig, spawns.transform);
            BuildDuelUi(ui.transform);
            BuildNavMesh(navigation.transform);
            SinglePlayerSceneWire.AddToOpenScene();
            AddToBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            _ = gameplay;
        }

        static GameObject PlaceStation(GameObject parent, GameObject prefab, string name, Vector3 position, Vector3 euler = default)
        {
            var instance = InstantiatePrefab(prefab, parent.transform, position, euler);
            instance.name = name;
            return instance;
        }

        static void ApplyEnvironmentSettings()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.30f, 0.18f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.18f, 0.10f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.08f, 0.05f);
            RenderSettings.subtractiveShadowColor = new Color(0.22f, 0.12f, 0.07f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.fogColor = new Color(0.38f, 0.24f, 0.12f);
            RenderSettings.skybox = null;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }

        static void BuildLighting(Transform parent)
        {
            var sunGo = Create("Fill_Directional", parent);
            sunGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.62f;
            sun.color = Kelvin(3200f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.55f;

            VolumeProfile profile = CreateVolumeProfile();
            var volumeGo = Create("SaloonVolume", parent);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
        }

        static VolumeProfile CreateVolumeProfile()
        {
            if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(SaloonAssetPaths.VolumeProfile) != null)
                AssetDatabase.DeleteAsset(SaloonAssetPaths.VolumeProfile);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, SaloonAssetPaths.VolumeProfile);

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.08f);
            bloom.intensity.Override(0.18f);
            bloom.scatter.Override(0.45f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.48f);
            vignette.color.Override(new Color(0.12f, 0.06f, 0.03f));

            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Medium2);
            grain.intensity.Override(0.22f);
            grain.response.Override(0.8f);

            var chroma = profile.Add<ChromaticAberration>(true);
            chroma.intensity.Override(0.14f);

            var motion = profile.Add<MotionBlur>(true);
            motion.intensity.Override(0.18f);
            motion.clamp.Override(0.04f);

            var color = profile.Add<ColorAdjustments>(true);
            color.contrast.Override(8f);
            color.saturation.Override(6f);
            color.postExposure.Override(-0.12f);
            color.colorFilter.Override(new Color(1f, 0.93f, 0.82f));

            var white = profile.Add<WhiteBalance>(true);
            white.temperature.Override(12f);
            white.tint.Override(4f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        static void BuildProbe(Transform parent)
        {
            var go = Create("InteriorProbe", parent);
            go.transform.position = new Vector3(0f, 1.8f, 0f);
            var probe = go.AddComponent<ReflectionProbe>();
            probe.center = Vector3.zero;
            probe.size = new Vector3(24f, 7f, 22f);
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.intensity = 0.65f;
            probe.nearClipPlane = 0.3f;
            probe.farClipPlane = 28f;
        }

        static void BuildAudio(Transform parent)
        {
            string[] ids =
            {
                "Amb_Room", "Amb_Lantern", "Amb_Glass", "Amb_Cards",
                "Amb_Roulette", "Amb_Slots", "Amb_Footsteps", "Amb_Wind", "Amb_Gunshot"
            };
            Vector3[] pos =
            {
                Vector3.up * 2f, new Vector3(0f, 2f, 9f), new Vector3(0f, 1.4f, 9.2f),
                new Vector3(-7f, 1.2f, 7f), new Vector3(-7f, 1.2f, 0.8f), new Vector3(8f, 1.4f, 0.9f),
                Vector3.zero, new Vector3(0f, 1.5f, -10f), new Vector3(-6.5f, 1.4f, -7f)
            };
            for (int i = 0; i < ids.Length; i++)
                Anchor(parent, ids[i], pos[i], Vector3.forward);
        }

        static void BuildCameraRig(GameObject rig, Transform spawns)
        {
            var camGo = Create("GameplayCamera", rig);
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.12f;
            camera.farClipPlane = 80f;
            camera.fieldOfView = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.07f, 0.04f);
            camGo.AddComponent<AudioListener>();
            if (camGo.GetComponent<UniversalAdditionalCameraData>() == null)
                camGo.AddComponent<UniversalAdditionalCameraData>();

            var main = Anchor(rig.transform, "Cam_MainSaloon", new Vector3(9.6f, 15.4f, -14.2f), new Vector3(-9.6f, -14.4f, 15.4f));
            CameraAnchor mainMarker = MarkCamera(main, "main-saloon", 42f, 0.75f);

            CameraAnchor[] found = Object.FindObjectsByType<CameraAnchor>(FindObjectsInactive.Include);
            var ordered = new List<CameraAnchor> { mainMarker };
            string[] order =
            {
                "bar", "poker-green", "poker-red", "roulette", "slots",
                "central-lounge", "duel-gameplay", "gallows", "entrance"
            };
            foreach (string id in order)
            {
                CameraAnchor match = found.FirstOrDefault(a => a != null && a.AnchorId == id);
                if (match != null)
                    ordered.Add(match);
            }

            var rigComp = rig.AddComponent<SaloonCameraRig>();
            var so = new SerializedObject(rigComp);
            so.FindProperty("gameplayCamera").objectReferenceValue = camera;
            so.FindProperty("followTarget").objectReferenceValue = null;
            so.FindProperty("followPlayer").boolValue = true;
            so.FindProperty("initialAnchor").objectReferenceValue = mainMarker;
            SerializedProperty array = so.FindProperty("anchors");
            array.arraySize = ordered.Count;
            for (int i = 0; i < ordered.Count; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = ordered[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            camera.transform.SetPositionAndRotation(main.position, main.rotation);
            _ = spawns;
        }

        static void BuildDuelUi(Transform parent)
        {
            var canvasGo = Create("DuelPresentationUI", parent);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            AddUiText(canvasGo.transform, "Header", "THE LAST BET", new Vector2(0f, 430f), 28, new Color(0.92f, 0.80f, 0.48f));
            AddUiText(canvasGo.transform, "Title", "STEADY...", new Vector2(0f, 40f), 86, new Color(0.96f, 0.86f, 0.55f));
            AddUiText(canvasGo.transform, "Subtitle", "WAIT FOR THE SIGNAL", new Vector2(0f, -40f), 22, new Color(0.78f, 0.68f, 0.42f));
            AddUiText(canvasGo.transform, "Prompt", "DRAW & FIRE", new Vector2(0f, -220f), 26, new Color(0.90f, 0.78f, 0.46f));
            canvasGo.SetActive(false);
        }

        static void AddUiText(Transform parent, string name, string text, Vector2 anchored, int size, Color color)
        {
            var go = Create(name, parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(1400f, 140f);
            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.font = FindFont();
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }

        static void BuildNavMesh(Transform parent)
        {
            Box("WalkableFloor", parent, AssetDatabase.LoadAssetAtPath<Material>($"{SaloonAssetPaths.Materials}/M_WoodFloor.mat"),
                new Vector3(0f, -0.02f, 0f), new Vector3(23.6f, 0.02f, 21.6f));
            var surface = parent.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.BuildNavMesh();
        }

        static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == SaloonAssetPaths.Scene))
                return;
            scenes.Add(new EditorBuildSettingsScene(SaloonAssetPaths.Scene, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
