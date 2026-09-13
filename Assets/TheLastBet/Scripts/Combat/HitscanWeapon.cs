using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private int clipSize = 6;
        [SerializeField] private float fireInterval = 0.45f;
        [SerializeField] private float reloadTime = 1.35f;
        [SerializeField] private float damage = 34f;
        [SerializeField] private float range = 42f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private Transform muzzle;
        [SerializeField] private AudioClip fireClip;
        [SerializeField] private AudioClip reloadClip;

        AudioSource _audio;
        float _nextFire;
        float _reloadUntil;
        int _ammo;
        bool _reloading;

        public int Ammo => _ammo;
        public int ClipSize => clipSize;
        public bool IsReloading => _reloading;
        public bool CanFire => !IsReloading && _ammo > 0 && Time.time >= _nextFire;

        void Awake()
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0.85f;
            if (hitMask == 0)
                hitMask = CombatLayers.HitMask;
            CombatAudio.AssignGenerated(ref fireClip, ref reloadClip);
            ReloadImmediate();
        }

        public void Configure(Transform muzzlePoint, AudioClip fire, AudioClip reload)
        {
            muzzle = muzzlePoint;
            fireClip = fire;
            reloadClip = reload;
        }

        public void ConfigureStats(int clip, float interval, float dmg)
        {
            clipSize = Mathf.Max(1, clip);
            fireInterval = Mathf.Max(0.05f, interval);
            damage = Mathf.Max(1f, dmg);
            ReloadImmediate();
        }

        public bool TryFire(Vector3 origin, Vector3 direction, GameObject owner)
        {
            if (!CanFire)
            {
                if (_ammo <= 0 && !IsReloading)
                    Reload();
                return false;
            }

            _ammo--;
            _nextFire = Time.time + fireInterval;
            Play(fireClip);

            int mask = hitMask.value;
            mask &= ~LayerMask.GetMask("Ignore Raycast", "UI", "TransparentFX", "Water");
            if (owner != null && owner.layer >= 0)
                mask &= ~(1 << owner.layer);
            int viewmodel = CombatLayers.Viewmodel;
            if (viewmodel >= 0)
                mask &= ~(1 << viewmodel);

            Vector3 start = muzzle != null ? muzzle.position : origin;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                SpawnTracer(start, hit.point);
                SpawnImpact(hit.point, hit.normal);
                var health = hit.collider.GetComponentInParent<Health>();
                if (health != null && (owner == null || health.gameObject != owner))
                    health.ApplyDamage(damage, owner);
            }
            else
            {
                SpawnTracer(start, origin + direction * range);
            }

            if (_ammo <= 0)
                Reload();
            return true;
        }

        public void Reload()
        {
            if (IsReloading || _ammo >= clipSize)
                return;
            _reloading = true;
            _reloadUntil = Time.time + reloadTime;
            Play(reloadClip);
        }

        void Update()
        {
            if (_reloading && Time.time >= _reloadUntil)
                ReloadImmediate();
        }

        void ReloadImmediate()
        {
            _ammo = clipSize;
            _reloadUntil = 0f;
            _reloading = false;
        }

        void Play(AudioClip clip)
        {
            if (_audio != null && clip != null)
                _audio.PlayOneShot(clip, 0.7f);
        }

        void SpawnTracer(Vector3 from, Vector3 to)
        {
            var go = new GameObject("Tracer");
            go.transform.position = from;
            var line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startWidth = 0.018f;
            line.endWidth = 0.004f;
            line.material = CreateFxMaterial(new Color(1f, 0.82f, 0.35f, 0.85f));
            line.startColor = new Color(1f, 0.82f, 0.35f, 0.85f);
            line.endColor = new Color(1f, 0.55f, 0.15f, 0f);
            Object.Destroy(go, 0.08f);

            if (muzzle != null)
            {
                var flash = new GameObject("MuzzleFlash");
                flash.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
                var light = flash.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.72f, 0.28f);
                light.range = 2.4f;
                light.intensity = 3.2f;
                Object.Destroy(flash, 0.06f);
            }
        }

        static void SpawnImpact(Vector3 point, Vector3 normal)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Impact";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = point + normal * 0.02f;
            go.transform.localScale = Vector3.one * 0.05f;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateFxMaterial(new Color(1f, 0.78f, 0.32f, 0.9f));
            Object.Destroy(go, 0.12f);
        }

        static Material CreateFxMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            return material;
        }
    }
}
