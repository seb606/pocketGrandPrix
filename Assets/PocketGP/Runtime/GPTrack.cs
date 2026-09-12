using UnityEngine;
using System.Collections.Generic;

namespace PocketGP {
public sealed class GPTrack : MonoBehaviour {
    public enum SurfaceType { Asphalt, Parquet, Dirt, Sand, Snow, Metropolis }

    public static readonly string[] Names = {
        "Petit-déjeuner express",
        "Bureau en folie",
        "Jardin des champions",
        "Dunes du Sahara",
        "Col Alpin Enneigé",
        "Métropole & Grand Pont"
    };

    public SurfaceType CurrentSurface;
    public List<Vector3> Points = new List<Vector3>();
    public float Width = 7.6f;
    public const int SamplesPerSegment = 16;
    public readonly List<Vector3> Gates = new List<Vector3>();
    public Vector3[] Controls;
    readonly List<Mesh> meshes = new List<Mesh>();

    public void Build(int index) {
        // 6 Circuits variés avec dénivelés 3D (Montées & Descentes)
        Vector3[][] all = {
            // 0: Petit-déjeuner express (Asphalte & collines de table)
            new[] {
                new Vector3(-26, 0.0f, -23),
                new Vector3(6, 1.4f, -25),
                new Vector3(30, 2.6f, -18),
                new Vector3(34, 1.8f, 4),
                new Vector3(21, 0.6f, 24),
                new Vector3(-8, 0.0f, 25),
                new Vector3(-32, 1.6f, 14),
                new Vector3(-36, 0.5f, -5)
            },
            // 1: Bureau en folie (Parquet ciré & montées d'étagères)
            new[] {
                new Vector3(-29, 0.0f, -25),
                new Vector3(1, 1.2f, -27),
                new Vector3(32, 3.2f, -23),
                new Vector3(33, 3.5f, -4),
                new Vector3(12, 1.6f, 0),
                new Vector3(26, 0.4f, 21),
                new Vector3(1, 1.9f, 27),
                new Vector3(-29, 2.4f, 21),
                new Vector3(-35, 0.0f, 0)
            },
            // 2: Jardin des champions (Terre battue & bosses de pelouse)
            new[] {
                new Vector3(-28, 0.0f, -26),
                new Vector3(1, 1.5f, -26),
                new Vector3(30, 2.8f, -21),
                new Vector3(35, 1.2f, -1),
                new Vector3(21, 0.0f, 9),
                new Vector3(31, 2.2f, 27),
                new Vector3(6, 3.4f, 29),
                new Vector3(-6, 1.9f, 11),
                new Vector3(-31, 0.6f, 24),
                new Vector3(-37, 0.0f, 0)
            },
            // 3: Dunes du Sahara (Sable doré, crêtes & cuvettes)
            new[] {
                new Vector3(-30, 0.0f, -24),
                new Vector3(-5, 2.0f, -28),
                new Vector3(24, 4.0f, -22),
                new Vector3(36, 1.6f, -2),
                new Vector3(28, 0.2f, 18),
                new Vector3(10, 2.8f, 26),
                new Vector3(-14, 4.2f, 24),
                new Vector3(-32, 1.4f, 10),
                new Vector3(-36, 0.0f, -8)
            },
            // 4: Col Alpin Enneigé (Neige & descentes sinueuses)
            new[] {
                new Vector3(-25, 0.0f, -26),
                new Vector3(2, 2.0f, -24),
                new Vector3(28, 4.2f, -16),
                new Vector3(35, 5.5f, 5),
                new Vector3(22, 3.6f, 25),
                new Vector3(-2, 2.2f, 27),
                new Vector3(-24, 1.2f, 18),
                new Vector3(-35, 0.0f, -4)
            },
            // 5: Métropole & Grand Pont (Pont suspendu 3D au-dessus du vide)
            new[] {
                new Vector3(-32, 0.0f, -24),
                new Vector3(-4, 0.0f, -27),
                new Vector3(25, 2.0f, -22),
                new Vector3(35, 4.5f, -6),
                new Vector3(35, 4.5f, 12),
                new Vector3(20, 2.2f, 26),
                new Vector3(-6, 0.0f, 26),
                new Vector3(-28, 0.0f, 16),
                new Vector3(-36, 0.0f, -4)
            }
        };

        index = Mathf.Clamp(index, 0, all.Length - 1);
        CurrentSurface = (SurfaceType)index;

        var list = all[index];
        Controls = new Vector3[list.Length];
        for (int i = 0; i < list.Length; i++) Controls[i] = list[i];

        for (int i = 0; i < list.Length; i++) {
            for (int k = 0; k < SamplesPerSegment; k++) {
                float t = k / (float)SamplesPerSegment;
                int n = list.Length;
                Vector3 a = Controls[(i + n - 1) % n], b = Controls[i], c = Controls[(i + 1) % n], d = Controls[(i + 2) % n];
                Points.Add(.5f * ((2 * b) + (-a + c) * t + (2 * a - 5 * b + 4 * c - d) * t * t + (-a + 3 * b - 3 * c + d) * t * t * t));
            }
        }
        for (int i = 0; i < Points.Count; i += 8) Gates.Add(Points[i]);

        // Couleurs de revêtements selon la surface du circuit
        string groundColor = "465763";
        string borderColor = "ECE0CA";
        string curbCol1 = "ED7C65", curbCol2 = "F9EBD4";

        switch (CurrentSurface) {
            case SurfaceType.Asphalt:
                groundColor = "465763";
                borderColor = "ECE0CA";
                break;
            case SurfaceType.Parquet:
                groundColor = "8A5A36";
                borderColor = "BF925E";
                curbCol1 = "D35400"; curbCol2 = "F5CBA7";
                break;
            case SurfaceType.Dirt:
                groundColor = "6D4C41";
                borderColor = "A1887F";
                curbCol1 = "8D6E63"; curbCol2 = "D7CCC8";
                break;
            case SurfaceType.Sand:
                groundColor = "D4AC0D";
                borderColor = "F9E79F";
                curbCol1 = "B7950B"; curbCol2 = "FEF9E7";
                break;
            case SurfaceType.Snow:
                groundColor = "D4E6F1";
                borderColor = "EBF5FB";
                curbCol1 = "3498DB"; curbCol2 = "FFFFFF";
                break;
            case SurfaceType.Metropolis:
                groundColor = "2C3E50";
                borderColor = "BDC3C7";
                curbCol1 = "E74C3C"; curbCol2 = "ECF0F1";
                break;
        }

        // Rubans de route 3D
        Ribbon(-Width * .5f - .65f, Width * .5f + .65f, -.01f, GPArt.Mat(borderColor));
        Ribbon(-Width * .5f, Width * .5f, .012f, GPArt.Mat(groundColor));
        Ribbon(-Width * .5f + .22f, -Width * .5f + .34f, .031f, GPArt.Mat("EDE4CE"));
        Ribbon(Width * .5f - .34f, Width * .5f - .22f, .031f, GPArt.Mat("EDE4CE"));

        for (int i = 0; i < Points.Count; i += 2) {
            Vector3 forward = Tangent(i);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            for (int s = -1; s <= 1; s += 2) {
                var curb = GPArt.Box(transform, "Vibreur", Points[i] + right * s * (Width * .5f + .19f) + Vector3.up * .055f, new Vector3(.45f, .10f, .85f), GPArt.Mat(i % 4 == 0 ? curbCol1 : curbCol2));
                curb.transform.rotation = Quaternion.LookRotation(forward);
            }
            if (i % 6 == 0) {
                var dash = GPArt.Box(transform, "Marquage", Points[i] + Vector3.up * .032f, new Vector3(.10f, .018f, .85f), GPArt.Mat("B7C5C3"));
                dash.transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        Vector3 f = Tangent(0), r = Vector3.Cross(Vector3.up, f).normalized;
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
            Vector3 fwd = Tangent(i), rt = Vector3.Cross(Vector3.up, fwd).normalized;
            int s = (i % 28 == 8) ? 1 : -1;
            Vector3 specPos = Points[i] + rt * s * (Width * .5f + 2.4f);
            Quaternion specRot = Quaternion.LookRotation(-rt * s);
            GPArt.Spectator(transform, specPos, specRot, fanColors[(i / 14) % fanColors.Length]);
        }

        // Grand Pont Suspendu 3D sur le circuit 5 (Métropole)
        if (index == 5) {
            Vector3 bridgeCenter = new Vector3(35f, 4.5f, 3.0f);
            Vector3 bridgeDir = Vector3.forward;
            GPArt.Bridge(transform, bridgeCenter, bridgeDir, Width, 22.0f, 4.5f);
        }

        GPArt.Decor(transform, index, Points, Width);
        BakeDecor();
    }

    void BakeDecor() {
        var groups = new Dictionary<Material, List<CombineInstance>>();
        foreach (var filter in GetComponentsInChildren<MeshFilter>()) {
            // Ne pas combiner les éléments animés ni les obstacles physiques avec Rigidbody !
            if (filter.GetComponentInParent<GPAnimatedDecor>()) continue;
            if (filter.GetComponentInParent<GPObstacle>() || filter.GetComponentInParent<Rigidbody>()) continue;
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

    void Ribbon(float a, float b, float yOffset, Material mat) {
        int n = Points.Count;
        Vector3[] v = new Vector3[(n + 1) * 2];
        int[] t = new int[n * 6];
        Vector2[] uvs = new Vector2[(n + 1) * 2];

        for (int i = 0; i <= n; i++) {
            Vector3 p = Points[i % n];
            Vector3 fwd = Tangent(i % n);
            Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
            v[i * 2] = p + right * a + Vector3.up * yOffset;
            v[i * 2 + 1] = p + right * b + Vector3.up * yOffset;
            uvs[i * 2] = new Vector2(0, i * 0.25f);
            uvs[i * 2 + 1] = new Vector2(1, i * 0.25f);

            if (i < n) {
                int j = i * 6, k = i * 2;
                t[j] = k; t[j + 1] = k + 2; t[j + 2] = k + 1;
                t[j + 3] = k + 1; t[j + 4] = k + 2; t[j + 5] = k + 3;
            }
        }
        var mesh = new Mesh { name = "Ruban circuit" };
        mesh.vertices = v;
        mesh.uv = uvs;
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

    public float GetElevation(Vector3 pos) {
        if (Points == null || Points.Count == 0) return 0f;
        float dist;
        int nearest = Nearest(pos, out dist);
        int next = (nearest + 1) % Points.Count;
        Vector3 p0 = Points[nearest], p1 = Points[next];
        Vector3 seg = p1 - p0;
        float segLen = seg.magnitude;
        if (segLen > 0.001f) {
            float proj = Mathf.Clamp01(Vector3.Dot(pos - p0, seg) / (segLen * segLen));
            return Mathf.Lerp(p0.y, p1.y, proj);
        }
        return p0.y;
    }

    void OnDestroy() { foreach (var m in meshes) if (m) Destroy(m); }
}
}
