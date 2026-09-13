#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheLastBet.Saloon.Editor
{
    internal static class SaloonMeshFactory
    {
        public static Mesh Cube { get; private set; }
        public static Mesh Cylinder { get; private set; }
        public static Mesh Sphere { get; private set; }
        public static Mesh Capsule { get; private set; }
        public static Mesh Quad { get; private set; }

        public static void BuildCore()
        {
            SaloonAssetPaths.RecreateFolder(SaloonAssetPaths.Meshes);
            EnsureBuiltins();
        }

        public static void EnsureBuiltins()
        {
            if (Cube == null) Cube = Builtin(PrimitiveType.Cube);
            if (Cylinder == null) Cylinder = Builtin(PrimitiveType.Cylinder);
            if (Sphere == null) Sphere = Builtin(PrimitiveType.Sphere);
            if (Capsule == null) Capsule = Builtin(PrimitiveType.Capsule);
            if (Quad == null) Quad = Builtin(PrimitiveType.Quad);
        }

        public static Mesh Stadium(string name, float length, float width, float height, int capSegments = 10)
        {
            float radius = width * 0.5f;
            float straight = Mathf.Max(0.01f, length - width);
            var outline = new List<Vector2>(capSegments * 2 + 4);
            for (int i = 0; i <= capSegments; i++)
            {
                float t = Mathf.PI * 0.5f + Mathf.PI * i / capSegments;
                outline.Add(new Vector2(-straight * 0.5f + Mathf.Cos(t) * radius, Mathf.Sin(t) * radius));
            }

            for (int i = 0; i <= capSegments; i++)
            {
                float t = -Mathf.PI * 0.5f + Mathf.PI * i / capSegments;
                outline.Add(new Vector2(straight * 0.5f + Mathf.Cos(t) * radius, Mathf.Sin(t) * radius));
            }

            return SaveExtrude(name, outline, height);
        }

        public static Mesh RegularPrism(string name, int sides, float radius, float height)
        {
            var outline = new List<Vector2>(sides);
            for (int i = 0; i < sides; i++)
            {
                float t = (i / (float)sides) * Mathf.PI * 2f + Mathf.PI / sides;
                outline.Add(new Vector2(Mathf.Cos(t) * radius, Mathf.Sin(t) * radius));
            }

            return SaveExtrude(name, outline, height);
        }

        public static Mesh CirclePrism(string name, float radius, float height, int segments = 24)
        {
            return RegularPrism(name, segments, radius, height);
        }

        public static Mesh Star(string name, float outerRadius, float innerRadius, float height, int points = 5)
        {
            var outline = new List<Vector2>(points * 2);
            for (int i = 0; i < points * 2; i++)
            {
                float t = -Mathf.PI * 0.5f + (i / (float)(points * 2)) * Mathf.PI * 2f;
                float r = (i % 2 == 0) ? outerRadius : innerRadius;
                outline.Add(new Vector2(Mathf.Cos(t) * r, Mathf.Sin(t) * r));
            }

            return SaveExtrude(name, outline, height);
        }

        public static Mesh Torus(string name, float radius, float tube, int radial = 28, int tubular = 10, float arcDegrees = 360f)
        {
            var mesh = new Mesh { name = name };
            float arc = arcDegrees * Mathf.Deg2Rad;
            int rings = radial + 1;
            var verts = new Vector3[rings * (tubular + 1)];
            var norms = new Vector3[verts.Length];
            var uvs = new Vector2[verts.Length];

            for (int i = 0; i < rings; i++)
            {
                float u = (i / (float)radial) * arc;
                var center = new Vector3(Mathf.Cos(u) * radius, 0f, Mathf.Sin(u) * radius);
                for (int j = 0; j <= tubular; j++)
                {
                    float v = (j / (float)tubular) * Mathf.PI * 2f;
                    var n = new Vector3(Mathf.Cos(u) * Mathf.Cos(v), Mathf.Sin(v), Mathf.Sin(u) * Mathf.Cos(v));
                    int idx = i * (tubular + 1) + j;
                    verts[idx] = center + n * tube;
                    norms[idx] = n;
                    uvs[idx] = new Vector2(i / (float)radial, j / (float)tubular);
                }
            }

            var tris = new List<int>(radial * tubular * 6);
            for (int i = 0; i < radial; i++)
            {
                for (int j = 0; j < tubular; j++)
                {
                    int a = i * (tubular + 1) + j;
                    int b = a + tubular + 1;
                    tris.Add(a);
                    tris.Add(b);
                    tris.Add(a + 1);
                    tris.Add(a + 1);
                    tris.Add(b);
                    tris.Add(b + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        static Mesh SaveExtrude(string name, List<Vector2> outline, float height)
        {
            var mesh = new Mesh { name = name };
            BuildExtrude(mesh, outline, height);
            return Save(mesh);
        }

        static void BuildExtrude(Mesh mesh, List<Vector2> outline, float height)
        {
            outline = new List<Vector2>(outline);
            outline.Reverse();
            int n = outline.Count;
            float y0 = -height * 0.5f;
            float y1 = height * 0.5f;
            var verts = new List<Vector3>(n * 6);
            var norms = new List<Vector3>(n * 6);
            var uvs = new List<Vector2>(n * 6);
            var tris = new List<int>(n * 12);

            float perimeter = 0f;
            var edgeLen = new float[n];
            for (int i = 0; i < n; i++)
            {
                edgeLen[i] = Vector2.Distance(outline[i], outline[(i + 1) % n]);
                perimeter += edgeLen[i];
            }

            // Caps
            int bottom = verts.Count;
            Vector2 min = outline[0], max = outline[0];
            for (int i = 0; i < n; i++)
            {
                min = Vector2.Min(min, outline[i]);
                max = Vector2.Max(max, outline[i]);
            }

            Vector2 size = max - min;
            if (size.x < 0.001f) size.x = 1f;
            if (size.y < 0.001f) size.y = 1f;

            for (int i = 0; i < n; i++)
            {
                var p = outline[i];
                verts.Add(new Vector3(p.x, y0, p.y));
                norms.Add(Vector3.down);
                uvs.Add(new Vector2((p.x - min.x) / size.x, (p.y - min.y) / size.y));
            }

            int top = verts.Count;
            for (int i = 0; i < n; i++)
            {
                var p = outline[i];
                verts.Add(new Vector3(p.x, y1, p.y));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2((p.x - min.x) / size.x, (p.y - min.y) / size.y));
            }

            for (int i = 1; i < n - 1; i++)
            {
                tris.Add(bottom);
                tris.Add(bottom + i + 1);
                tris.Add(bottom + i);
                tris.Add(top);
                tris.Add(top + i);
                tris.Add(top + i + 1);
            }

            // Sides
            float dist = 0f;
            for (int i = 0; i < n; i++)
            {
                int i1 = (i + 1) % n;
                var a = outline[i];
                var b = outline[i1];
                var outward = new Vector3(b.y - a.y, 0f, -(b.x - a.x)).normalized;
                if (outward.sqrMagnitude < 0.001f)
                    outward = Vector3.right;

                float u0 = dist / Mathf.Max(0.001f, perimeter) * 4f;
                float u1 = (dist + edgeLen[i]) / Mathf.Max(0.001f, perimeter) * 4f;
                int s = verts.Count;
                verts.Add(new Vector3(a.x, y0, a.y));
                verts.Add(new Vector3(b.x, y0, b.y));
                verts.Add(new Vector3(b.x, y1, b.y));
                verts.Add(new Vector3(a.x, y1, a.y));
                for (int k = 0; k < 4; k++)
                    norms.Add(outward);
                uvs.Add(new Vector2(u0, 0f));
                uvs.Add(new Vector2(u1, 0f));
                uvs.Add(new Vector2(u1, 1f));
                uvs.Add(new Vector2(u0, 1f));
                tris.Add(s);
                tris.Add(s + 3);
                tris.Add(s + 2);
                tris.Add(s);
                tris.Add(s + 2);
                tris.Add(s + 1);
                dist += edgeLen[i];
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
        }

        static Mesh Save(Mesh mesh)
        {
            string path = $"{SaloonAssetPaths.Meshes}/{mesh.name}.asset";
            AssetDatabase.CreateAsset(mesh, path);
            return AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }

        static Mesh Builtin(PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(go);
            return mesh;
        }
    }
}
#endif
