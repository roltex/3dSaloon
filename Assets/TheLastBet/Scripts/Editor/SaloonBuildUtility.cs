#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheLastBet.Saloon.Editor
{
    internal enum AuthoredFit
    {
        Center,
        MaxZ,
        MinZ
    }

    internal static class SaloonBuildUtility
    {
        public static GameObject Create(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static GameObject Create(string name, GameObject parent) => Create(name, parent != null ? parent.transform : null);

        public static GameObject PrefabRoot(string name)
        {
            var root = new GameObject(name);
            Create("Visual", root);
            Create("Collision", root);
            Create("Anchors", root);
            return root;
        }

        public static Transform Visual(GameObject root) => root.transform.Find("Visual");
        public static Transform Collision(GameObject root) => root.transform.Find("Collision");
        public static Transform Anchors(GameObject root) => root.transform.Find("Anchors");

        public static GameObject MeshPart(
            string name,
            Transform parent,
            Mesh mesh,
            Material material,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            var go = Create(name, parent);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(localEuler);
            go.transform.localScale = localScale;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        public static GameObject Box(
            string name,
            Transform parent,
            Material material,
            Vector3 center,
            Vector3 size,
            Vector3 euler = default)
        {
            return MeshPart(name, parent, SaloonMeshFactory.Cube, material, center, euler, size);
        }

        public static GameObject Cyl(
            string name,
            Transform parent,
            Material material,
            Vector3 center,
            float radius,
            float height,
            Vector3 euler = default)
        {
            return MeshPart(name, parent, SaloonMeshFactory.Cylinder, material, center, euler, new Vector3(radius * 2f, height * 0.5f, radius * 2f));
        }

        public static GameObject Sphere(
            string name,
            Transform parent,
            Material material,
            Vector3 center,
            float radius)
        {
            return MeshPart(name, parent, SaloonMeshFactory.Sphere, material, center, Vector3.zero, Vector3.one * (radius * 2f));
        }

        public static BoxCollider CollisionBox(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var go = Create(name, parent);
            go.transform.localPosition = center;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            return box;
        }

        public static CapsuleCollider CollisionCapsule(Transform parent, string name, Vector3 center, float radius, float height)
        {
            var go = Create(name, parent);
            go.transform.localPosition = center;
            var cap = go.AddComponent<CapsuleCollider>();
            cap.radius = radius;
            cap.height = height;
            return cap;
        }

        public static SphereCollider TriggerSphere(Transform parent, string name, Vector3 center, float radius)
        {
            var go = Create(name, parent);
            go.transform.localPosition = center;
            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;
            return sphere;
        }

        public static Transform Anchor(Transform parent, string name, Vector3 position, Vector3 lookAtLocal)
        {
            var go = Create(name, parent);
            go.transform.localPosition = position;
            if (lookAtLocal.sqrMagnitude > 0.001f)
                go.transform.localRotation = Quaternion.LookRotation(lookAtLocal.normalized, Vector3.up);
            return go.transform;
        }

        public static CameraAnchor MarkCamera(Transform t, string id, float fov = 45f, float blend = 0.65f)
        {
            var marker = t.gameObject.GetComponent<CameraAnchor>() ?? t.gameObject.AddComponent<CameraAnchor>();
            var so = new SerializedObject(marker);
            so.FindProperty("anchorId").stringValue = id;
            so.FindProperty("fieldOfView").floatValue = fov;
            so.FindProperty("blendDuration").floatValue = blend;
            so.ApplyModifiedPropertiesWithoutUndo();
            return marker;
        }

        public static InteractableStation BindStation(
            GameObject root,
            string id,
            string display,
            SaloonStationType type,
            Transform player,
            Transform camera,
            Transform audio,
            Collider trigger,
            Transform opponent = null,
            IList<Transform> spectators = null,
            float radius = 2.2f)
        {
            var station = root.GetComponent<InteractableStation>() ?? root.AddComponent<InteractableStation>();
            var so = new SerializedObject(station);
            so.FindProperty("stationId").stringValue = id;
            so.FindProperty("displayName").stringValue = display;
            so.FindProperty("stationType").enumValueIndex = (int)type;
            so.FindProperty("interactionRadius").floatValue = radius;
            so.FindProperty("stationEnabled").boolValue = true;
            so.FindProperty("interactionTrigger").objectReferenceValue = trigger;
            so.FindProperty("cameraAnchor").objectReferenceValue = camera;
            so.FindProperty("playerAnchor").objectReferenceValue = player;
            so.FindProperty("opponentAnchor").objectReferenceValue = opponent;
            so.FindProperty("audioAnchor").objectReferenceValue = audio;
            SerializedProperty specs = so.FindProperty("spectatorAnchors");
            specs.ClearArray();
            if (spectators != null)
            {
                specs.arraySize = spectators.Count;
                for (int i = 0; i < spectators.Count; i++)
                    specs.GetArrayElementAtIndex(i).objectReferenceValue = spectators[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return station;
        }

        public static void BindDuelAnchors(
            GameObject root,
            Transform playerA,
            Transform playerB,
            Transform intro,
            Transform gameplay,
            Transform resultA,
            Transform resultB,
            Transform[] spectators)
        {
            var duel = root.GetComponent<DuelArenaAnchors>() ?? root.AddComponent<DuelArenaAnchors>();
            var so = new SerializedObject(duel);
            so.FindProperty("playerA").objectReferenceValue = playerA;
            so.FindProperty("playerB").objectReferenceValue = playerB;
            so.FindProperty("introCamera").objectReferenceValue = intro;
            so.FindProperty("gameplayCamera").objectReferenceValue = gameplay;
            so.FindProperty("resultCameraA").objectReferenceValue = resultA;
            so.FindProperty("resultCameraB").objectReferenceValue = resultB;
            SerializedProperty specs = so.FindProperty("spectatorAnchors");
            specs.ClearArray();
            if (spectators != null)
            {
                specs.arraySize = spectators.Length;
                for (int i = 0; i < spectators.Length; i++)
                    specs.GetArrayElementAtIndex(i).objectReferenceValue = spectators[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Light PointLight(Transform parent, string name, Vector3 position, float range, float intensity, float kelvin = 2700f)
        {
            var go = Create(name, parent);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = Kelvin(kelvin);
            light.shadows = LightShadows.None;
            light.shadowStrength = 0.72f;
            return light;
        }

        public static Color Kelvin(float kelvin)
        {
            float t = Mathf.InverseLerp(2200f, 4000f, kelvin);
            return Color.Lerp(new Color(1f, 0.55f, 0.22f), new Color(1f, 0.86f, 0.62f), t);
        }

        public static GameObject Label(Transform parent, string name, string text, Vector3 position, Vector3 euler, float characterSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = Create(name, parent);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(euler);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = characterSize;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.font = FindFont();
            if (tm.font != null)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.sharedMaterial = tm.font.material;
            }

            return go;
        }

        public static Font FindFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Liberation Sans" }, 32);
            return font;
        }

        public static GameObject SavePrefab(GameObject instance, string folder, string fileName)
        {
            SaloonAssetPaths.EnsureFolder(folder);
            string path = $"{folder}/{fileName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        public static GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 localPosition, Vector3 localEuler = default)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(localEuler);
            if (PrefabUtility.IsPartOfPrefabInstance(instance))
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            return instance;
        }

        public static Bounds EncapsulateRenderers(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        public static GameObject PlaceAuthoredModel(
            Transform parent,
            string assetPath,
            string name,
            float targetWidth,
            AuthoredFit fit = AuthoredFit.Center,
            float alignZ = 0f,
            Vector3 localEuler = default)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing authored model at " + assetPath);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(localEuler);
            instance.transform.localScale = Vector3.one;
            if (PrefabUtility.IsPartOfPrefabInstance(instance))
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new System.InvalidOperationException("Authored model has no renderers: " + assetPath);

            var bounds = EncapsulateRenderers(instance);
            float scale = targetWidth / Mathf.Max(0.001f, bounds.size.x);
            instance.transform.localScale = Vector3.one * scale;

            bounds = EncapsulateRenderers(instance);
            float z = fit switch
            {
                AuthoredFit.MaxZ => parent.position.z + alignZ - bounds.max.z,
                AuthoredFit.MinZ => parent.position.z + alignZ - bounds.min.z,
                _ => parent.position.z - bounds.center.z
            };
            instance.transform.position += new Vector3(
                parent.position.x - bounds.center.x,
                -bounds.min.y,
                z);

            foreach (var renderer in renderers)
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                    continue;
                var collider = renderer.GetComponent<MeshCollider>();
                if (collider == null)
                    collider = renderer.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
            }

            return instance;
        }

        public static GameObject Rope(Transform parent, string name, Vector3 a, Vector3 b, float radius, Material material)
        {
            Vector3 mid = (a + b) * 0.5f;
            Vector3 delta = b - a;
            float length = delta.magnitude;
            var go = Cyl(name, parent, material, mid, radius, length);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            return go;
        }
    }
}
#endif
