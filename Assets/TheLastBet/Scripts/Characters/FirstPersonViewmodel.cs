using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheLastBet.Saloon
{
    /// <summary>
    /// First-person revolver and hands from the Meshy viewmodel, parented to the camera.
    /// The mesh has no bones, so shoot is a recoil kick plus a muzzle flash.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class FirstPersonViewmodel : MonoBehaviour
    {
        public const string ModelPath = "Assets/Models/characters/first person gun/Meshy_AI_model.glb";

        Camera _world;
        Camera _overlay;
        Transform _hold;
        Transform _model;
        Transform _flash;
        Vector3 _holdBase;
        Quaternion _holdBaseRot;
        float _recoil;
        float _flashAge = 99f;
        int _worldMask = -1;

        public Transform Muzzle { get; private set; }

        public static FirstPersonViewmodel Attach(Camera worldCamera, GameObject gunPrefab = null)
        {
            if (worldCamera == null)
                return null;

            FirstPersonViewmodel existing = worldCamera.GetComponentInChildren<FirstPersonViewmodel>();
            if (existing != null)
            {
                existing.gameObject.SetActive(false);
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("FirstPersonViewmodel");
            root.transform.SetParent(worldCamera.transform, false);
            var view = root.AddComponent<FirstPersonViewmodel>();
            view.Build(worldCamera, gunPrefab);
            return view;
        }

        void Build(Camera worldCamera, GameObject gunPrefab)
        {
            _world = worldCamera;
            _hold = new GameObject("Hold").transform;
            _hold.SetParent(transform, false);
            _hold.localPosition = new Vector3(0.09f, -0.14f, 0.35f);
            _hold.localRotation = Quaternion.Euler(5f, 2f, 0f);
            _holdBase = _hold.localPosition;
            _holdBaseRot = _hold.localRotation;

            GameObject source = gunPrefab != null ? gunPrefab : LoadModel();
            if (source == null)
            {
                Debug.LogError("[The Last Bet] First-person gun model is missing.");
                return;
            }

            _model = Instantiate(source, _hold).transform;
            _model.name = "GunHands";
            _model.localPosition = Vector3.zero;
            _model.localRotation = Quaternion.Euler(0f, 180f, 0f);
            _model.localScale = Vector3.one * 0.18f;
            StripWorldJunk(_model.gameObject);

            foreach (Renderer renderer in _model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            var muzzleGo = new GameObject("Muzzle");
            muzzleGo.transform.SetParent(_model, false);
            muzzleGo.transform.localPosition = FindBarrelTip(_model);
            muzzleGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Muzzle = muzzleGo.transform;
            BuildFlash(muzzleGo.transform);

            int layer = CombatLayers.Viewmodel;
            if (layer >= 0)
            {
                CombatLayers.SetLayerRecursively(gameObject, layer);
                CreateOverlay(worldCamera, layer);
            }
        }

        void BuildFlash(Transform muzzle)
        {
            _flash = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            _flash.name = "MuzzleFlash";
            _flash.SetParent(_hold, false);
            _flash.localScale = Vector3.zero;
            Collider col = _flash.GetComponent<Collider>();
            if (col != null)
                DestroyObject(col);

            var renderer = _flash.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = CreateFlashMaterial();

            var lightGo = new GameObject("FlashLight");
            lightGo.transform.SetParent(_flash, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.72f, 0.28f);
            light.range = 1.6f;
            light.intensity = 2.4f;
            light.shadows = LightShadows.None;

            _flash.gameObject.SetActive(false);
        }

        void CreateOverlay(Camera worldCamera, int layer)
        {
            _worldMask = worldCamera.cullingMask;
            worldCamera.cullingMask &= ~(1 << layer);

            var camGo = new GameObject("ViewmodelCamera");
            camGo.transform.SetParent(worldCamera.transform, false);
            _overlay = camGo.AddComponent<Camera>();
            _overlay.CopyFrom(worldCamera);
            _overlay.clearFlags = CameraClearFlags.Depth;
            _overlay.cullingMask = 1 << layer;
            _overlay.depth = worldCamera.depth + 1;
            _overlay.fieldOfView = 54f;
            _overlay.nearClipPlane = 0.03f;
            _overlay.farClipPlane = 4f;
            _overlay.allowHDR = worldCamera.allowHDR;
            _overlay.allowMSAA = worldCamera.allowMSAA;

            AudioListener listener = camGo.GetComponent<AudioListener>();
            if (listener != null)
                DestroyObject(listener);

            var overlayData = _overlay.GetComponent<UniversalAdditionalCameraData>();
            if (overlayData == null)
                overlayData = camGo.AddComponent<UniversalAdditionalCameraData>();
            overlayData.renderType = CameraRenderType.Overlay;
            overlayData.renderShadows = false;

            var baseData = worldCamera.GetUniversalAdditionalCameraData();
            if (baseData != null && !baseData.cameraStack.Contains(_overlay))
                baseData.cameraStack.Add(_overlay);
        }

        public void PlayShoot()
        {
            _recoil = 1f;
            _flashAge = 0f;
            ApplyMotion(0f);
        }

        void LateUpdate()
        {
            if (_hold == null)
                return;

            _recoil = Mathf.MoveTowards(_recoil, 0f, Time.deltaTime * 5.2f);
            ApplyMotion(Time.unscaledDeltaTime);
        }

        void ApplyMotion(float flashDelta)
        {
            float kick = _recoil * _recoil;
            float t = Time.unscaledTime;
            Vector3 sway = new Vector3(Mathf.Sin(t * 1.15f) * 0.004f, Mathf.Cos(t * 0.85f) * 0.003f, 0f);
            Vector3 punch = new Vector3(0.008f, 0.034f, -0.055f) * kick;
            _hold.localPosition = _holdBase + sway + punch;
            _hold.localRotation = _holdBaseRot * Quaternion.Euler(-20f * kick, 4f * _recoil, -3f * _recoil);
            UpdateFlash(flashDelta);
            if (_overlay != null)
                _overlay.fieldOfView = 54f;
        }

        void UpdateFlash(float delta)
        {
            if (_flash == null)
                return;

            _flashAge += delta;
            bool on = _flashAge < 0.07f;
            _flash.gameObject.SetActive(on);
            if (!on || Muzzle == null)
                return;

            float u = 1f - _flashAge / 0.07f;
            _flash.position = Muzzle.position + Muzzle.forward * 0.035f;
            _flash.localScale = Vector3.one * (0.026f + 0.018f * u);
        }

        void OnDestroy()
        {
            if (_world == null)
                return;

            if (_overlay != null)
            {
                var baseData = _world.GetComponent<UniversalAdditionalCameraData>();
                if (baseData != null)
                    baseData.cameraStack.Remove(_overlay);
            }

            if (_worldMask != -1)
                _world.cullingMask = _worldMask;
        }

        static GameObject LoadModel()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
#else
            return null;
#endif
        }

        static Vector3 FindBarrelTip(Transform model)
        {
            var filter = model.GetComponentInChildren<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                return new Vector3(0f, 0.05f, -0.7f);

            Vector3[] verts = filter.sharedMesh.vertices;
            Vector3 tip = Vector3.zero;
            float best = float.PositiveInfinity;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].z >= best)
                    continue;
                best = verts[i].z;
                tip = verts[i];
            }

            return tip;
        }

        static Material CreateFlashMaterial()
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            var material = new Material(shader);
            var color = new Color(1f, 0.82f, 0.28f, 1f);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            return material;
        }

        static void StripWorldJunk(GameObject root)
        {
            foreach (var listener in root.GetComponentsInChildren<AudioListener>(true))
                DestroyObject(listener);
            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                DestroyObject(cam);
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
                DestroyObject(col);
        }

        static void DestroyObject(Object obj)
        {
            if (obj == null)
                return;
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }
    }
}
