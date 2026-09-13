using UnityEngine;

namespace TheLastBet.Saloon
{
    public static class WesternSaloonBuilder
    {
        public const string ModelPath = "Assets/Models/day_5._finalthe_western_saloon.glb";
        public const float ModelScale = 0.01f;
        public const float SeatHeight = 0.9f;
        public const float CharacterScale = 2f;

        public static readonly Vector3 SpawnLocal = new Vector3(4.2f, 0.16f, -3f);
        public static readonly Vector3 SelectBanditLocal = new Vector3(3f, 0.16f, 3f);

        public static bool MatchesCurrentLayout(SaloonVenue venue)
        {
            if (venue == null || venue.PlayerSpawn == null || venue.SelectBandit == null)
                return false;
            Transform model = venue.transform.Find("WesternSaloonModel");
            if (model == null || model.childCount == 0)
                return false;
            if (Mathf.Abs(model.localScale.x - ModelScale) > 0.0004f)
                return false;
            if (Mathf.Abs(model.localScale.y - ModelScale) > 0.0004f)
                return false;
            if (Mathf.Abs(Mathf.DeltaAngle(model.GetChild(0).localEulerAngles.x, 90f)) < 8f)
                return false;
            if ((venue.PlayerSpawn.position - SpawnLocal).sqrMagnitude > 0.25f)
                return false;
            if ((venue.SelectBandit.position - SelectBanditLocal).sqrMagnitude > 0.25f)
                return false;
            if (venue.transform.Find("WalkableFloor") != null)
                return false;
            Transform collision = venue.transform.Find("Collision");
            if (collision == null)
                return false;
            if (collision.Find("SolidGround_box") == null)
                return false;
            if (collision.Find("SolidUpper_box") == null)
                return false;
            if (collision.Find("CatchSlab_box") == null)
                return false;
            return venue.SelectBandit != null;
        }

        public static SaloonVenue Build(Transform parent, GameObject modelSource)
        {
            var root = new GameObject("Venue_Western");
            if (parent != null)
                root.transform.SetParent(parent, false);

            if (modelSource == null)
            {
#if UNITY_EDITOR
                modelSource = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
#endif
            }

            if (modelSource == null)
            {
                Debug.LogError("[The Last Bet] Western saloon model is missing.");
                return root.AddComponent<SaloonVenue>();
            }

            var model = Object.Instantiate(modelSource, root.transform);
            model.name = "WesternSaloonModel";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * ModelScale;
            FlattenImportRotation(model);
            AlignToFloor(model);

            AddColliders(root.transform, model);
            AddLights(root.transform);

            Transform spawn = Marker(root.transform, "PlayerSpawn", SpawnLocal, Vector3.forward);
            Transform bandit = Marker(root.transform, "SelectBandit", SelectBanditLocal, Vector3.back);
            Transform cam = Marker(root.transform, "SelectCamera", new Vector3(3f, 3.1f, 0.36f), Vector3.forward);
            Transform look = Marker(root.transform, "SelectLook", new Vector3(3f, 2.1f, 3f), Vector3.forward);

            var duelGo = new GameObject("DuelArena");
            duelGo.transform.SetParent(root.transform, false);
            duelGo.transform.localPosition = new Vector3(0f, 0f, -3f);
            Transform playerA = Marker(duelGo.transform, "PlayerA", new Vector3(-3.2f, 0.16f, 0f), Vector3.right);
            Transform playerB = Marker(duelGo.transform, "PlayerB", new Vector3(3.2f, 0.16f, 0f), Vector3.left);

            var anchors = duelGo.AddComponent<DuelArenaAnchors>();
            anchors.Configure(playerA, playerB);
            var station = duelGo.AddComponent<InteractableStation>();
            station.ConfigureDuel("western-duel", playerA, playerB, 5.2f);

            var venue = root.AddComponent<SaloonVenue>();
            venue.Configure("Western Saloon", null, spawn, bandit, bandit, bandit, cam, look, anchors, station);
            return venue;
        }

        static void FlattenImportRotation(GameObject model)
        {
            if (model.transform.childCount == 0)
                return;

            Transform child = model.transform.GetChild(0);
            if (Mathf.Abs(Mathf.DeltaAngle(child.localEulerAngles.x, 90f)) < 8f)
                child.localRotation = Quaternion.identity;
        }

        static void AlignToFloor(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            Renderer chandelier = null;
            float seatSum = 0f;
            int seatCount = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                string name = renderers[i].name.ToLowerInvariant();
                if (chandelier == null && name.Contains("chandelier"))
                    chandelier = renderers[i];
                if (!name.Contains("_sit_"))
                    continue;
                float y = renderers[i].bounds.center.y;
                if (y > 2.2f)
                    continue;
                seatSum += y;
                seatCount++;
            }

            Vector3 room = chandelier != null ? chandelier.bounds.center : Vector3.zero;
            float floorY = seatCount > 0 ? (seatSum / seatCount) - SeatHeight : 0f;
            model.transform.position += new Vector3(-room.x, -floorY, -room.z);
        }

        static void AddColliders(Transform parent, GameObject model)
        {
            var collision = new GameObject("Collision");
            collision.transform.SetParent(parent, false);

            MeshFilter[] filters = model.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter == null || filter.sharedMesh == null)
                    continue;
                Renderer renderer = filter.GetComponent<Renderer>();
                if (renderer == null || !renderer.enabled)
                    continue;

                Bounds world = renderer.bounds;
                string name = filter.name.ToLowerInvariant();
                if (ShouldSkip(name, world))
                    continue;

                if (IsWalkableSurface(name, world) || IsStair(name))
                    BakeMeshCollider(collision.transform, filter);
                else if (IsSolidProp(name, world) || IsWallSlab(name, world))
                    AddWorldBox(collision.transform, filter.name, world);
            }

            AddSolidSlabs(collision.transform, model, filters);
            AddStairBoxes(collision.transform, filters);
            AddBalconyRails(collision.transform, filters);
        }

        static void AddSolidSlabs(Transform parent, GameObject model, MeshFilter[] filters)
        {
            Bounds hall = EncapsulateRenderers(model);
            float pad = 2.4f;
            float groundH = 0.4f;
            AddWorldBox(parent, "SolidGround", new Bounds(
                new Vector3(hall.center.x, -groundH * 0.5f, hall.center.z),
                new Vector3(hall.size.x + pad, groundH, hall.size.z + pad)));

            Bounds upper = default;
            bool hasUpper = TryFindNamedBounds(filters, "Plane.003", out upper);
            if (!hasUpper)
                upper = new Bounds(new Vector3(hall.center.x, 4.5f, hall.center.z), new Vector3(hall.size.x * 0.7f, 0.2f, hall.size.z * 0.55f));
            AddWorldBox(parent, "SolidUpper", new Bounds(
                new Vector3(upper.center.x, upper.max.y - 0.12f, upper.center.z),
                new Vector3(Mathf.Max(4f, upper.size.x), 0.36f, Mathf.Max(4f, upper.size.z))));

            AddWorldBox(parent, "CatchSlab", new Bounds(new Vector3(hall.center.x, -2f, hall.center.z), new Vector3(80f, 0.4f, 80f)));
        }

        static void AddStairBoxes(Transform parent, MeshFilter[] filters)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter == null || !IsStair(filter.name.ToLowerInvariant()))
                    continue;
                Renderer renderer = filter.GetComponent<Renderer>();
                if (renderer == null)
                    continue;

                Bounds b = renderer.bounds;
                bool alongZ = b.size.z >= b.size.x;
                float rise = Mathf.Max(0.35f, b.size.y);
                int steps = Mathf.Clamp(Mathf.RoundToInt(rise / 0.26f), 5, 16);
                float stepH = rise / steps;
                float run = alongZ ? b.size.z : b.size.x;
                float stepD = run / steps;
                float width = alongZ ? b.size.x : b.size.z;
                float start = alongZ ? b.min.z : b.min.x;
                for (int s = 0; s < steps; s++)
                {
                    float y = b.min.y + stepH * (s + 0.5f);
                    float along = start + stepD * (s + 0.5f);
                    Vector3 center = alongZ
                        ? new Vector3(b.center.x, y, along)
                        : new Vector3(along, y, b.center.z);
                    Vector3 size = alongZ
                        ? new Vector3(width + 0.12f, stepH + 0.04f, stepD + 0.06f)
                        : new Vector3(stepD + 0.06f, stepH + 0.04f, width + 0.12f);
                    AddWorldBox(parent, filter.name + "_step" + s, new Bounds(center, size));
                }
            }
        }

        static bool TryFindNamedBounds(MeshFilter[] filters, string token, out Bounds bounds)
        {
            bounds = default;
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i] == null || filters[i].name.IndexOf(token, System.StringComparison.Ordinal) < 0)
                    continue;
                Renderer renderer = filters[i].GetComponent<Renderer>();
                if (renderer == null)
                    continue;
                bounds = renderer.bounds;
                return true;
            }

            return false;
        }

        static Bounds EncapsulateRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = new Bounds(root.transform.position, Vector3.one);
            bool started = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || !renderers[i].enabled)
                    continue;
                if (!started)
                {
                    bounds = renderers[i].bounds;
                    started = true;
                }
                else
                    bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        static bool ShouldSkip(string name, Bounds world)
        {
            if (name.Contains("candle") || name.Contains("chand") || name.Contains("emissio") ||
                name.Contains("light.") || name.Contains("glass") || name.Contains("wine") ||
                name.Contains("bottle") || name.Contains("coffee") || name.Contains("windiw") ||
                name.Contains("window") || name.Contains("curtain") || name.Contains("cloth"))
                return true;

            Vector3 size = world.size;
            float volume = size.x * size.y * size.z;
            if (volume < 0.012f)
                return true;
            if (size.x < 0.08f && size.y < 0.08f && size.z < 0.08f)
                return true;
            return false;
        }

        static bool IsStair(string name)
        {
            return name.Contains("step") || name.Contains("stair");
        }

        static bool IsWalkableSurface(string name, Bounds world)
        {
            if (IsStair(name))
                return false;
            if (name.Contains("lak.wood") || name.Contains("tab") || name.Contains("dask"))
                return false;

            Vector3 size = world.size;
            float area = size.x * size.z;
            if (size.y > 0.35f || area < 6f)
                return false;

            float y = world.center.y;
            bool ground = y < 0.55f;
            bool upper = y > 3.4f && y < 5.8f;
            if (!ground && !upper)
                return false;

            return name.Contains("plane") || name.Contains("floor") || name.Contains("wood") ||
                   name.Contains("material");
        }

        static bool IsSolidProp(string name, Bounds world)
        {
            if (name.Contains("_sit_") || name.Contains("chair") || name.Contains("stool") ||
                name.Contains("tab_") || name.Contains("table") || name.Contains("wand.bar") ||
                name.Contains("bar_"))
                return true;
            if (name.Contains("lak.wood"))
                return world.size.y > 0.35f && world.size.x * world.size.z < 25f;
            if (name.Contains("dark.wood") || name.Contains("wood.text"))
            {
                if (world.size.y < 0.4f)
                    return false;
                if (world.size.x * world.size.z > 18f && world.size.y < 1.2f)
                    return false;
                return true;
            }
            return false;
        }

        static bool IsWallSlab(string name, Bounds world)
        {
            Vector3 size = world.size;
            if (size.y < 1.4f)
                return false;
            float thin = Mathf.Min(size.x, size.z);
            float wide = Mathf.Max(size.x, size.z);
            if (thin > 1.4f || wide < 0.8f)
                return false;
            return name.Contains("wood") || name.Contains("wall") || name.Contains("cube") ||
                   name.Contains("plane") || name.Contains("door");
        }

        static void BakeMeshCollider(Transform parent, MeshFilter filter)
        {
            Mesh source = filter.sharedMesh;
            if (source.vertexCount < 3 || source.triangles == null || source.triangles.Length < 3)
                return;

            var baked = new Mesh { name = filter.name + "_col" };
            if (source.vertexCount > 65000)
                baked.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            Vector3[] src = source.vertices;
            Vector3[] world = new Vector3[src.Length];
            Matrix4x4 matrix = filter.transform.localToWorldMatrix;
            for (int i = 0; i < src.Length; i++)
                world[i] = matrix.MultiplyPoint3x4(src[i]);
            baked.vertices = world;
            baked.triangles = source.triangles;
            baked.RecalculateBounds();

            var go = new GameObject(filter.name + "_col");
            go.transform.SetParent(parent, false);
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = baked;
            collider.convex = false;
            go.AddComponent<BakedMeshCollider>().Bind(baked);
        }

        static void AddBalconyRails(Transform parent, MeshFilter[] filters)
        {
            Bounds floor = default;
            bool found = false;
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i] == null || filters[i].name.IndexOf("Plane.003", System.StringComparison.Ordinal) < 0)
                    continue;
                Renderer renderer = filters[i].GetComponent<Renderer>();
                if (renderer == null)
                    continue;
                floor = renderer.bounds;
                found = true;
                break;
            }

            if (!found)
                return;

            float top = floor.max.y + 1.05f;
            float thickness = 0.16f;
            float height = 1.85f;
            float minX = floor.min.x;
            float maxX = floor.max.x;
            float minZ = floor.min.z;
            float maxZ = floor.max.z;
            float widthX = maxX - minX;
            float widthZ = maxZ - minZ;

            AddWorldBox(parent, "Rail_North", new Bounds(new Vector3(floor.center.x, top, maxZ), new Vector3(widthX, height, thickness)));
            AddWorldBox(parent, "Rail_South", new Bounds(new Vector3(floor.center.x, top, minZ), new Vector3(widthX, height, thickness)));
            AddWorldBox(parent, "Rail_East", new Bounds(new Vector3(maxX, top, floor.center.z), new Vector3(thickness, height, widthZ)));

            // Leave a gap on the west side where the staircase meets the balcony.
            float gapCenterZ = 0.24f;
            float gap = 1.6f;
            float southLen = Mathf.Max(0.5f, (gapCenterZ - gap) - minZ);
            float northLen = Mathf.Max(0.5f, maxZ - (gapCenterZ + gap));
            float southMid = minZ + southLen * 0.5f;
            float northMid = maxZ - northLen * 0.5f;
            AddWorldBox(parent, "Rail_WestSouth", new Bounds(new Vector3(minX, top, southMid), new Vector3(thickness, height, southLen)));
            AddWorldBox(parent, "Rail_WestNorth", new Bounds(new Vector3(minX, top, northMid), new Vector3(thickness, height, northLen)));
        }

        static void AddWorldBox(Transform parent, string name, Bounds world)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name + "_box";
            go.transform.SetParent(parent, false);
            go.transform.position = world.center;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.Max(world.size, new Vector3(0.08f, 0.08f, 0.08f));
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;
        }

        static void AddLights(Transform parent)
        {
            AddPoint(parent, "Lamp_Center", new Vector3(0f, 4.2f, 0f), 2.8f, 16f);
            AddPoint(parent, "Lamp_Bar", new Vector3(0f, 3.4f, -6.5f), 2.2f, 13f);
            AddPoint(parent, "Lamp_South", new Vector3(0f, 3.2f, 3.8f), 2f, 12f);
        }

        static void AddPoint(Transform parent, string name, Vector3 pos, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.82f, 0.58f);
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        static Transform Marker(Transform parent, string name, Vector3 localPos, Vector3 forward)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);
            return go.transform;
        }
    }
}
