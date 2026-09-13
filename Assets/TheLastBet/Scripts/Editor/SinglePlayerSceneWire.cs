#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using static TheLastBet.Saloon.Editor.SaloonBuildUtility;

namespace TheLastBet.Saloon.Editor
{
    internal static class SinglePlayerSceneWire
    {
        [MenuItem("The Last Bet/Wire Single Player")]
        public static void WireMenu()
        {
            AddToOpenScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[The Last Bet] Single-player director, UI, and camera wiring saved.");
        }

        public static void AddToOpenScene()
        {
            CharacterPrefabBuilder.EnsureLayers();
            DestroyNamed("WalkPreview");
            DestroyNamed("SK_Male_Placeholder");
            DestroyNamed("SK_Female_Placeholder");
            DestroyNamed("_MeasureMale");
            DestroyNamed("_GlbMeasure");

            GameObject gameplay = GameObject.Find("Gameplay");
            if (gameplay == null)
                gameplay = new GameObject("Gameplay");

            GameObject ui = GameObject.Find("UI");
            if (ui == null)
                ui = new GameObject("UI");

            var director = Object.FindAnyObjectByType<GameDirector>(FindObjectsInactive.Include);
            if (director == null)
                director = Create("GameDirector", gameplay).AddComponent<GameDirector>();

            SaloonSelectUI saloonSelect = Object.FindAnyObjectByType<SaloonSelectUI>(FindObjectsInactive.Include);
            if (saloonSelect == null)
                saloonSelect = BuildSaloonSelect(ui.transform);

            CharacterSelectUI select = Object.FindAnyObjectByType<CharacterSelectUI>(FindObjectsInactive.Include);
            if (select != null)
                Object.DestroyImmediate(select.gameObject);
            select = BuildSelect(ui.transform);

            HudView hud = Object.FindAnyObjectByType<HudView>(FindObjectsInactive.Include);
            if (hud == null)
                hud = BuildHud(ui.transform);

            ResultView result = Object.FindAnyObjectByType<ResultView>(FindObjectsInactive.Include);
            if (result == null)
                result = BuildResult(ui.transform);

            EnsureEventSystem();

            var cameraRig = Object.FindAnyObjectByType<SaloonCameraRig>(FindObjectsInactive.Include);
            if (cameraRig != null)
            {
                var camSo = new SerializedObject(cameraRig);
                camSo.FindProperty("followTarget").objectReferenceValue = null;
                camSo.FindProperty("followPlayer").boolValue = true;
                int playerLayer = CombatLayers.Player;
                if (playerLayer >= 0)
                    camSo.FindProperty("collisionMask").intValue &= ~(1 << playerLayer);
                camSo.ApplyModifiedPropertiesWithoutUndo();
            }

            SaloonVenue classic = BuildClassicVenue();
            EnsureBanditMarker(classic, new Vector3(0f, 0.05f, -7.55f));
            SaloonVenue western = BuildWesternVenue();

            var so = new SerializedObject(director);
            so.FindProperty("malePrefab").objectReferenceValue = null;
            so.FindProperty("femalePrefab").objectReferenceValue = null;
            so.FindProperty("banditPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(SaloonAssetPaths.BanditPrefab);
            so.FindProperty("viewmodelPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(SaloonAssetPaths.FirstPersonGun);
            so.FindProperty("westernSaloonPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(SaloonAssetPaths.WesternSaloon);
            so.FindProperty("cameraRig").objectReferenceValue = cameraRig;
            so.FindProperty("saloonSelectUi").objectReferenceValue = saloonSelect;
            so.FindProperty("selectUi").objectReferenceValue = select;
            so.FindProperty("hud").objectReferenceValue = hud;
            so.FindProperty("result").objectReferenceValue = result;
            so.FindProperty("fireClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SaloonAssetPaths.FireSound);
            so.FindProperty("reloadClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SaloonAssetPaths.ReloadSound);
            SerializedProperty venueProp = so.FindProperty("venues");
            venueProp.arraySize = 2;
            venueProp.GetArrayElementAtIndex(0).objectReferenceValue = classic;
            venueProp.GetArrayElementAtIndex(1).objectReferenceValue = western;
            so.ApplyModifiedPropertiesWithoutUndo();

            select.gameObject.SetActive(false);
            saloonSelect.gameObject.SetActive(true);
            hud.gameObject.SetActive(false);
            result.gameObject.SetActive(false);
            EditorUtility.SetDirty(director);
        }

        static SaloonSelectUI BuildSaloonSelect(Transform parent)
        {
            var canvas = Overlay(parent, "SaloonSelect", 22);
            AddLabel(canvas.transform, "Title", "THE LAST BET", new Vector2(0f, 360f), 42, new Color(0.92f, 0.80f, 0.48f));
            AddLabel(canvas.transform, "Subtitle", "CHOOSE YOUR SALOON", new Vector2(0f, 290f), 22, new Color(0.78f, 0.68f, 0.42f));
            Button classic = Card(canvas.transform, "ClassicCard", "CLASSIC SALOON", new Vector2(-280f, -40f), null);
            Button western = Card(canvas.transform, "WesternCard", "WESTERN SALOON", new Vector2(280f, -40f), null);
            var view = canvas.AddComponent<SaloonSelectUI>();
            view.Bind(classic, western);
            return view;
        }

        static SaloonVenue BuildClassicVenue()
        {
            var existing = GameObject.Find("Venue_Classic");
            if (existing != null)
                return existing.GetComponent<SaloonVenue>();

            var root = new GameObject("Venue_Classic");
            var extras = new System.Collections.Generic.List<GameObject>();
            var env = GameObject.Find("Environment");
            if (env != null)
            {
                Transform shell = env.transform.Find("SaloonShell");
                if (shell != null)
                    extras.Add(shell.gameObject);
            }

            var stations = GameObject.Find("GameplayStations");
            if (stations != null)
                extras.Add(stations);
            var nav = GameObject.Find("Navigation");
            if (nav != null)
                extras.Add(nav);

            Transform spawn = Marker(root.transform, "PlayerSpawn", new Vector3(0f, 0f, -5.2f), Vector3.forward);
            Transform male = Marker(root.transform, "SelectMale", new Vector3(-1.45f, 0.05f, -7.55f), Vector3.forward);
            Transform female = Marker(root.transform, "SelectFemale", new Vector3(1.45f, 0.05f, -7.55f), Vector3.forward);
            Transform bandit = Marker(root.transform, "SelectBandit", new Vector3(0f, 0.05f, -7.55f), Vector3.forward);
            Transform cam = Marker(root.transform, "SelectCamera", new Vector3(0f, 1.55f, -3.55f), Vector3.back);
            Transform look = Marker(root.transform, "SelectLook", new Vector3(0f, 1.05f, -7.55f), Vector3.forward);
            DuelArenaAnchors duel = null;
            DuelArenaAnchors[] duels = Object.FindObjectsByType<DuelArenaAnchors>(FindObjectsInactive.Include);
            for (int i = 0; i < duels.Length; i++)
            {
                if (duels[i] != null && duels[i].transform.root.name.IndexOf("Western", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    duel = duels[i];
                    break;
                }
            }

            InteractableStation station = null;
            InteractableStation[] all = Object.FindObjectsByType<InteractableStation>(FindObjectsInactive.Include);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].StationType == SaloonStationType.DuelArena &&
                    all[i].transform.root.name.IndexOf("Western", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    station = all[i];
                    break;
                }
            }

            var venue = root.AddComponent<SaloonVenue>();
            venue.Configure("Classic Saloon", extras.ToArray(), spawn, male, female, bandit, cam, look, duel, station);
            return venue;
        }

        static SaloonVenue BuildWesternVenue()
        {
            SaloonVenue[] found = Object.FindObjectsByType<SaloonVenue>(FindObjectsInactive.Include);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null && found[i].name.IndexOf("Western", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    Object.DestroyImmediate(found[i].gameObject);
            }

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(SaloonAssetPaths.WesternSaloon);
            SaloonVenue venue = WesternSaloonBuilder.Build(null, model);
            if (venue != null)
                venue.SetVenueActive(false);
            return venue;
        }

        static Transform Marker(Transform parent, string name, Vector3 pos, Vector3 forward)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(forward, Vector3.up));
            return go.transform;
        }

        static CharacterSelectUI BuildSelect(Transform parent)
        {
            var canvas = Overlay(parent, "CharacterSelect", 20);
            AddLabel(canvas.transform, "Title", "THE LAST BET", new Vector2(0f, 360f), 42, new Color(0.92f, 0.80f, 0.48f));
            AddLabel(canvas.transform, "Subtitle", "THE BANDIT", new Vector2(0f, 290f), 22, new Color(0.78f, 0.68f, 0.42f));
            Button bandit = Card(canvas.transform, "BanditCard", "PLAY", new Vector2(0f, -40f), null);
            var view = canvas.AddComponent<CharacterSelectUI>();
            view.Bind(null, null, bandit);
            return view;
        }

        static void EnsureBanditMarker(SaloonVenue venue, Vector3 pos)
        {
            if (venue == null || venue.SelectBandit != null)
                return;
            Transform existing = venue.transform.Find("SelectBandit");
            if (existing == null)
                existing = Marker(venue.transform, "SelectBandit", pos, Vector3.forward);
            venue.BindSelectBandit(existing);
            EditorUtility.SetDirty(venue);
        }

        static HudView BuildHud(Transform parent)
        {
            var canvas = Overlay(parent, "Hud", 15);
            Text health = AddLabel(canvas.transform, "Health", "HEALTH  100 / 100", new Vector2(-700f, -460f), 22, new Color(0.90f, 0.78f, 0.46f));
            Text ammo = AddLabel(canvas.transform, "Ammo", "AMMO  6 / 6", new Vector2(700f, -460f), 22, new Color(0.90f, 0.78f, 0.46f));
            Text remaining = AddLabel(canvas.transform, "Remaining", "GUNSLINGERS  5", new Vector2(0f, 430f), 20, new Color(0.86f, 0.74f, 0.44f));
            Text sight = AddLabel(canvas.transform, "Crosshair", "+", Vector2.zero, 36, new Color(0.95f, 0.86f, 0.55f, 0.8f));
            var view = canvas.AddComponent<HudView>();
            view.Bind(health, ammo, remaining, sight);
            canvas.SetActive(false);
            return view;
        }

        static ResultView BuildResult(Transform parent)
        {
            var canvas = Overlay(parent, "Result", 25);
            Text title = AddLabel(canvas.transform, "Title", "YOU STAND", new Vector2(0f, 70f), 72, new Color(0.96f, 0.86f, 0.55f));
            Text subtitle = AddLabel(canvas.transform, "Subtitle", "THE LAST BET IS YOURS", new Vector2(0f, -20f), 22, new Color(0.78f, 0.68f, 0.42f));
            Button again = Card(canvas.transform, "PlayAgain", "PLAY AGAIN", new Vector2(0f, -180f), null);
            var view = canvas.AddComponent<ResultView>();
            view.Bind(title, subtitle, again);
            canvas.SetActive(false);
            return view;
        }

        static GameObject UiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject Overlay(Transform parent, string name, int order)
        {
            var go = UiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static Text AddLabel(Transform parent, string name, string text, Vector2 anchored, int size, Color color)
        {
            var go = UiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(1100f, 140f);
            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.font = FindFont();
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        static Button Card(Transform parent, string name, string label, Vector2 anchored, string portraitPath)
        {
            var go = UiObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(300f, 400f);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.16f, 0.09f, 0.05f, 0.94f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.42f, 0.28f, 0.12f, 1f);
            colors.pressedColor = new Color(0.55f, 0.38f, 0.16f, 1f);
            button.colors = colors;

            if (!string.IsNullOrEmpty(portraitPath))
            {
                Texture2D portrait = AssetDatabase.LoadAssetAtPath<Texture2D>(portraitPath);
                if (portrait != null)
                {
                    var rawGo = UiObject("Portrait", go.transform);
                    var rawRect = rawGo.GetComponent<RectTransform>();
                    rawRect.anchorMin = new Vector2(0.5f, 0.5f);
                    rawRect.anchorMax = new Vector2(0.5f, 0.5f);
                    rawRect.anchoredPosition = new Vector2(0f, 40f);
                    rawRect.sizeDelta = new Vector2(260f, 260f);
                    var raw = rawGo.AddComponent<RawImage>();
                    raw.texture = portrait;
                    raw.color = Color.white;
                }
            }

            AddLabel(go.transform, "Label", label, new Vector2(0f, -160f), 28, new Color(0.92f, 0.80f, 0.48f));
            return button;
        }

        static void EnsureEventSystem()
        {
            var existing = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            GameObject go = existing != null ? existing.gameObject : Create("EventSystem", (Transform)null);
            if (existing == null)
                go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            if (go.GetComponent<InputSystemUIInputModule>() == null)
                go.AddComponent<InputSystemUIInputModule>();
            var legacy = go.GetComponent<StandaloneInputModule>();
            if (legacy != null)
                Object.DestroyImmediate(legacy);
#else
            if (go.GetComponent<StandaloneInputModule>() == null)
                go.AddComponent<StandaloneInputModule>();
#endif
        }

        static void DestroyNamed(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
                Object.DestroyImmediate(go);
        }
    }
}
#endif
