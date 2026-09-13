using UnityEngine;

namespace TheLastBet.Saloon
{
    public static class ClassicAmbientCrowd
    {
        const string RootName = "AmbientCrowd";

        static readonly Vector3[] Spots =
        {
            new Vector3(0.9f, 0f, 8.7f),
            new Vector3(-1.1f, 0f, 8.5f),
            new Vector3(-7.0f, 0f, 6.6f),
            new Vector3(7.0f, 0f, 6.6f),
            new Vector3(0.8f, 0f, 0.2f),
            new Vector3(-6.4f, 0f, 0.4f),
            new Vector3(0f, 0f, -16.6f),
            new Vector3(0.6f, 0f, 17.4f)
        };

        public static void Sync(SaloonVenue venue, GameObject banditPrefab)
        {
            if (venue != null && venue.name.IndexOf("Western", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Clear();
                return;
            }

            if (venue == null)
            {
                Clear();
                return;
            }

            Ensure(banditPrefab);
        }

        public static void Clear()
        {
            var root = GameObject.Find(RootName);
            if (root != null)
                Object.Destroy(root);
        }

        static void Ensure(GameObject prefab)
        {
            if (GameObject.Find(RootName) != null)
                return;
            if (prefab == null)
                return;

            Transform parent = null;
            var stations = GameObject.Find("GameplayStations");
            if (stations != null)
                parent = stations.transform;

            var root = new GameObject(RootName);
            if (parent != null)
                root.transform.SetParent(parent, false);

            for (int i = 0; i < Spots.Length; i++)
            {
                var go = Object.Instantiate(prefab, Spots[i], Quaternion.Euler(0f, 35f * i, 0f), root.transform);
                go.name = "Patron_" + i;
                go.transform.localScale = Vector3.one;
                StripPlayable(go);
            }
        }

        static void StripPlayable(GameObject go)
        {
            var walk = go.GetComponent<SaloonWalkPreview>();
            if (walk != null)
                Object.Destroy(walk);
            var weapon = go.GetComponent<HitscanWeapon>();
            if (weapon != null)
                Object.Destroy(weapon);
            var rpg = go.GetComponent<RpgCameraBinder>();
            if (rpg != null)
                Object.Destroy(rpg);
            var cc = go.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
            CombatLayers.SetLayerRecursively(go, 0);
            var anim = go.GetComponent<CharacterAnimDriver>();
            if (anim != null)
                anim.SetLocomotion(0f, true);
        }
    }
}
