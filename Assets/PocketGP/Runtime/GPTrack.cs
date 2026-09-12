using UnityEngine;
using System.Collections.Generic;

namespace PocketGP {
public sealed class GPTrack : MonoBehaviour {
    public static readonly string[] Names = { "Petit-déjeuner express", "Bureau en folie", "Jardin des champions" };
    public List<Vector3> Points = new List<Vector3>();
    public float Width = 7.6f;
    public const int SamplesPerSegment = 16;
    public readonly List<Vector3> Gates = new List<Vector3>();
    public Vector3[] Controls;
    readonly List<Mesh> meshes = new List<Mesh>();

    public void Build(int index) {
        Vector2[][] all = {
            new[] { new Vector2(-26,-23), new Vector2(6,-25), new Vector2(30,-18), new Vector2(34,4), new Vector2(21,24), new Vector2(-8,25), new Vector2(-32,14), new Vector2(-36,-5) },
            new[] { new Vector2(-29,-25), new Vector2(1,-27), new Vector2(32,-23), new Vector2(33,-4), new Vector2(12,0), new Vector2(26,21), new Vector2(1,27), new Vector2(-29,21), new Vector2(-35,0) },
            new[] { new Vector2(-28,-26), new Vector2(1,-26), new Vector2(30,-21), new Vector2(35,-1), new Vector2(21,9), new Vector2(31,27), new Vector2(6,29), new Vector2(-6,11), new Vector2(-31,24), new Vector2(-37,0) }
        };
        var list = all[index];
        Controls = new Vector3[list.Length];
        for (int i = 0; i < list.Length; i++) Controls[i] = new Vector3(list[i].x, 0, list[i].y);
        for (int i = 0; i < list.Length; i++) {
            for (int k = 0; k < SamplesPerSegment; k++) {
                float t = k / (float)SamplesPerSegment;
                int n = list.Length;
                Vector3 a = Controls[(i + n - 1) % n], b = Controls[i], c = Controls[(i + 1) % n], d = Controls[(i + 2) % n];
                Points.Add(.5f * ((2 * b) + (-a + c) * t + (2 * a - 5 * b + 4 * c - d) * t * t + (-a + 3 * b - 3 * c + d) * t * t * t));
            }
        }
        for (int i = 0; i < Points.Count; i += 8) Gates.Add(Points[i]);

        Ribbon(-Width * .5f - .65f, Width * .5f + .65f, -.01f, GPArt.Mat("ECE0CA"));
        Ribbon(-Width * .5f, Width * .5f, .012f, GPArt.Mat(index == 2 ? "657777" : "465763"));
        Ribbon(-Width * .5f + .22f, -Width * .5f + .34f, .031f, GPArt.Mat("EDE4CE"));
        Ribbon(Width * .5f - .34f, Width * .5f - .22f, .031f, GPArt.Mat("EDE4CE"));

        for (int i = 0; i < Points.Count; i += 2) {
            Vector3 forward = Tangent(i), right = Vector3.Cross(Vector3.up, forward);
            for (int s = -1; s <= 1; s += 2) {
                var curb = GPArt.Box(transform, "Vibreur", Points[i] + right * s * (Width * .5f + .19f) + Vector3.up * .055f, new Vector3(.45f, .10f, .85f), GPArt.Mat(i % 4 == 0 ? "ED7C65" : "F9EBD4"));
                curb.transform.rotation = Quaternion.LookRotation(forward);
            }
            if (i % 6 == 0) {
                var dash = GPArt.Box(transform, "Marquage", Points[i] + Vector3.up * .032f, new Vector3(.10f, .018f, .85f), GPArt.Mat("B7C5C3"));
                dash.transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        Vector3 f = Tangent(0), r = Vector3.Cross(Vector3.up, f);
        for (int x = 0; x < 10; x++) {
            for (int z = 0; z < 2; z++) {
                var tile = GPArt.Box(transform, "Damier", Points[0] + r * ((x - 4.5f) * Width / 10) + f * ((z - .5f) * .46f) + Vector3.up * .055f, new Vector3(Width / 10, .03f, .46f), GPArt.Mat((x + z) % 2 == 0 ? "F8F0D9" : "1D3041"));
                tile.transform.rotation = Quaternion.LookRotation(f);
            }
        }

        // Grande Arche d'arrivée
        GPArt.Cylinder(transform, "Arche G", Points[0] - r * (Width * .5f + 1.2f) + Vector3.up * 2.5f, new Vector3(.35f, 2.5f, .35f), GPArt.Mat("F6C754", .6f));
        GPArt.Cylinder(transform, "Arche D", Points[0] + r * (Width * .5f + 1.2f) + Vector3.up * 2.5f, new Vector3(.35f, 2.5f, .35f), GPArt.Mat("F6C754", .6f));
        var beam = GPArt.Box(transform, "Traverse", Points[0] + Vector3.up * 5.1f, new Vector3(Width + 2.8f, .5f, .6f), GPArt.Mat("1E293B", .8f));
        beam.transform.rotation = Quaternion.LookRotation(f);
        var banner = GPArt.Box(transform, "Banderole Arrivee", Points[0] + Vector3.up * 4.3f, new Vector3(Width + 1.8f, .95f, .1f), GPArt.Mat("54E3C5", .9f));
        banner.transform.rotation = Quaternion.LookRotation(f);

        // Spectateurs animés tout autour du circuit
        string[] fanColors = { "FF5E57", "4BCFFA", "FFDD59", "0BE881", "FFA801" };
        for (int i = 8; i < Points.Count; i += 14) {
            Vector3 fwd = Tangent(i), rt = Vector3.Cross(Vector3.up, fwd);
            int s = (i % 28 == 8) ? 1 : -1;
            Vector3 specPos = Points[i] + rt * s * (Width * .5f + 2.4f);
            Quaternion specRot = Quaternion.LookRotation(-rt * s);
            GPArt.Spectator(transform, specPos, specRot, fanColors[(i / 14) % fanColors.Length]);
        }

        GPArt.Decor(transform, index, Points, Width);
        BakeDecor();
    }

    void BakeDecor() {
        var groups = new Dictionary<Material, List<CombineInstance>>();
        foreach (var filter in GetComponentsInChildren<MeshFilter>()) {
            // Ne pas combiner les éléments animés !
            if (filter.GetComponentInParent<GPAnimatedDecor>()) continue;
            var renderer = filter.GetComponent<MeshRenderer>();
            if (!renderer || !filter.sharedMesh) continue;
            Material material = renderer.sharedMaterial;
            if (!groups.ContainsKey(material)) groups[material] = new List<CombineInstance>();
            groups[material].Add(new CombineInstance { mesh = filter.sharedMesh, transform = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix });
            renderer.enabled = false;
        }
        foreach (var entry in groups) {
            var mesh = new Mesh { name = "Decor combine" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(entry.Value.ToArray(), true, true);
            mesh.RecalculateBounds();
            meshes.Add(mesh);
            var obj = new GameObject("Decor - " + entry.Key.name);
            obj.transform.SetParent(transform, false);
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial = entry.Key;
        }
    }

    void Ribbon(float a, float b, float y, Material mat) {
        int n = Points.Count;
        Vector3[] v = new Vector3[(n + 1) * 2];
        int[] t = new int[n * 6];
        for (int i = 0; i <= n; i++) {
            Vector3 p = Points[i % n], r = Vector3.Cross(Vector3.up, Tangent(i % n));
            v[i * 2] = p + r * a + Vector3.up * y;
            v[i * 2 + 1] = p + r * b + Vector3.up * y;
            if (i < n) {
                int j = i * 6, k = i * 2;
                t[j] = k; t[j + 1] = k + 2; t[j + 2] = k + 1;
                t[j + 3] = k + 1; t[j + 4] = k + 2; t[j + 5] = k + 3;
            }
        }
        var mesh = new Mesh { name = "Ruban circuit" };
        mesh.vertices = v;
        mesh.triangles = t;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshes.Add(mesh);
        var o = new GameObject("Piste");
        o.transform.SetParent(transform, false);
        o.AddComponent<MeshFilter>().sharedMesh = mesh;
        o.AddComponent<MeshRenderer>().sharedMaterial = mat;
    }

    public Vector3 Tangent(int i) {
        int n = Points.Count;
        return (Points[(i + 1) % n] - Points[(i + n - 1) % n]).normalized;
    }

    public int Nearest(Vector3 p, out float distance) {
        int best = 0;
        float sq = float.MaxValue;
        for (int i = 0; i < Points.Count; i++) {
            float d = (p - Points[i]).sqrMagnitude;
            if (d < sq) { sq = d; best = i; }
        }
        distance = Mathf.Sqrt(sq);
        return best;
    }

    void OnDestroy() { foreach (var m in meshes) if (m) Destroy(m); }
}
}
