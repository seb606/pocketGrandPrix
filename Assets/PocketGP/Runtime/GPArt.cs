using UnityEngine;
using System.Collections.Generic;

namespace PocketGP {
public static class GPArt {
    static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

    public static Color Hex(string value) {
        Color c;
        ColorUtility.TryParseHtmlString("#" + value, out c);
        return c;
    }

    public static Material Mat(string color, float gloss = .25f, float metal = 0) {
        string key = color + gloss + metal;
        if (cache.ContainsKey(key)) return cache[key];
        var shader = Resources.Load<Shader>("PocketLit");
        if (!shader) shader = Shader.Find("Standard");
        var m = new Material(shader);
        m.color = Hex(color);
        m.SetFloat("_Glossiness", gloss);
        m.SetFloat("_Metallic", metal);
        cache[key] = m;
        return m;
    }

    public static GameObject Shape(Transform parent, string name, PrimitiveType type, Vector3 p, Vector3 scale, Material mat) {
        var o = GameObject.CreatePrimitive(type);
        o.name = name;
        o.transform.SetParent(parent, false);
        o.transform.localPosition = p;
        o.transform.localScale = scale;
        o.GetComponent<Renderer>().sharedMaterial = mat;
        Object.Destroy(o.GetComponent<Collider>());
        return o;
    }

    public static GameObject Box(Transform parent, string name, Vector3 p, Vector3 s, Material m) { return Shape(parent, name, PrimitiveType.Cube, p, s, m); }
    public static GameObject Sphere(Transform parent, string name, Vector3 p, Vector3 s, Material m) { return Shape(parent, name, PrimitiveType.Sphere, p, s, m); }
    public static GameObject Cylinder(Transform parent, string name, Vector3 p, Vector3 s, Material m) { return Shape(parent, name, PrimitiveType.Cylinder, p, s, m); }

    public static Transform Car(Transform parent, int color) {
        var prefab = Resources.Load<GameObject>("Cars/Car_" + (color % 5));
        if (prefab != null) {
            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Carrosserie_3D";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            return instance.transform;
        }

        string[] colors = { "FF4838", "2BE4D8", "FFC83B", "9A7BFF", "FF5AA8" };
        var root = new GameObject("Carrosserie").transform;
        root.SetParent(parent, false);
        var paint = Mat(colors[color % 5], .85f, .25f);
        var black = Mat("0F1E28", .4f);
        var glass = Mat("1C3B4E", .95f, .5f);
        var white = Mat("FFFBF0", .8f);
        var chrome = Mat("DCEEF5", .95f, .85f);
        var shadow = Mat("081017", .15f);

        // Ombre de contact au sol (anti-flou & contraste fort)
        Box(root, "OmbreSol", new Vector3(0, .018f, 0), new Vector3(1.28f, .012f, 2.15f), shadow);

        // Châssis & carrosserie sculptée haute précision
        Box(root, "Chassis", new Vector3(0, .26f, 0), new Vector3(.96f, .22f, 1.76f), black);
        Sphere(root, "Carrosserie galbee", new Vector3(0, .47f, 0), new Vector3(1.08f, .62f, 1.86f), paint);
        Box(root, "Capot", new Vector3(0, .49f, .55f), new Vector3(.88f, .21f, .68f), paint);
        Sphere(root, "Cockpit", new Vector3(0, .73f, -.12f), new Vector3(.78f, .58f, .90f), glass);
        Box(root, "Toit", new Vector3(0, .96f, -.21f), new Vector3(.62f, .075f, .42f), paint);
        Box(root, "Bande capot", new Vector3(0, .61f, .58f), new Vector3(.15f, .026f, .60f), white);
        Box(root, "Bande toit", new Vector3(0, 1.005f, -.21f), new Vector3(.15f, .02f, .40f), white);

        // Aileron arrière sport avec montants
        Box(root, "Aileron", new Vector3(0, .75f, -.80f), new Vector3(1.18f, .085f, .22f), paint);
        Box(root, "MontantG", new Vector3(-.36f, .60f, -.79f), new Vector3(.045f, .22f, .06f), black);
        Box(root, "MontantD", new Vector3(.36f, .60f, -.79f), new Vector3(.045f, .22f, .06f), black);

        // Calandre avant sportive
        Box(root, "Calandre", new Vector3(0, .36f, .90f), new Vector3(.72f, .12f, .05f), black);

        // Sorties d'échappement double chrome
        for (int i = -1; i <= 1; i += 2) {
            var pot = Cylinder(root, "Echappement", new Vector3(i * .31f, .34f, -.89f), new Vector3(.09f, .12f, .09f), chrome);
            pot.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        // Roues larges & jantes sport brillantes
        for (int i = -1; i <= 1; i += 2) {
            Box(root, "Phare", new Vector3(i * .31f, .47f, .91f), new Vector3(.24f, .13f, .055f), Mat("FFFDE6", 1f));
            Box(root, "Feu", new Vector3(i * .32f, .44f, -.87f), new Vector3(.23f, .12f, .06f), Mat("FF2233", 1f));
            for (int j = -1; j <= 1; j += 2) {
                var w = Cylinder(root, "Roue", new Vector3(i * .51f, .28f, j * .57f), new Vector3(.48f, .13f, .48f), black);
                w.transform.localRotation = Quaternion.Euler(0, 0, 90);
                var hub = Cylinder(root, "Jante", new Vector3(i * .625f, .28f, j * .57f), new Vector3(.27f, .015f, .27f), chrome);
                hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
        }
        return root;
    }

    public static GameObject ItemBox(Transform parent) {
        var root = new GameObject("Boite Objet");
        root.transform.SetParent(parent, false);
        var cube = Box(root.transform, "Cube", Vector3.zero, Vector3.one * 1.05f, Mat("FFD23F", .85f, .2f));
        cube.transform.localRotation = Quaternion.Euler(45, 45, 0);
        var inner = Box(root.transform, "Centre", Vector3.zero, Vector3.one * 0.75f, Mat("FF6B6B", .9f, .4f));
        var anim = root.AddComponent<GPAnimatedDecor>();
        anim.Type = GPAnimType.ItemBox;
        return root;
    }

    public static GameObject Missile(Transform parent) {
        var root = new GameObject("Missile");
        root.transform.SetParent(parent, false);
        var body = Cylinder(root.transform, "Corps", Vector3.zero, new Vector3(.32f, .9f, .32f), Mat("FF3838", .8f, .3f));
        body.transform.localRotation = Quaternion.Euler(90, 0, 0);
        var tip = Sphere(root.transform, "Pointe", new Vector3(0, 0, .48f), Vector3.one * .34f, Mat("FFF275", .9f));
        for (int k = 0; k < 3; k++) {
            float a = k * 120 * Mathf.Deg2Rad;
            var fin = Box(root.transform, "Ailette", new Vector3(Mathf.Cos(a) * .22f, Mathf.Sin(a) * .22f, -.35f), new Vector3(.04f, .18f, .25f), Mat("2B2D42"));
            fin.transform.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
        }
        return root;
    }

    public static GameObject OilPuddle(Transform parent) {
        var root = new GameObject("Flaque Huile");
        root.transform.SetParent(parent, false);
        var p = Cylinder(root.transform, "Flaque", Vector3.zero, new Vector3(2.4f, .035f, 2.4f), Mat("111D28", .95f, .8f));
        var drop = Sphere(root.transform, "Goutte", new Vector3(.85f, 0, .6f), new Vector3(.7f, .04f, .6f), Mat("111D28", .95f, .8f));
        return root;
    }

    public static GameObject Shield(Transform parent) {
        var s = Sphere(parent, "Bouclier", new Vector3(0, .6f, 0), new Vector3(2.4f, 1.9f, 2.8f), Mat("54E3C5", .9f, .1f));
        return s;
    }

    public static GameObject Flame(Transform parent, Vector3 localPos) {
        var root = new GameObject("Flamme Turbo");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        // Plume principale bleue cyan électrique
        var plume = Cylinder(root.transform, "Plume", new Vector3(0, 0, -.65f), new Vector3(.24f, .75f, .24f), Mat("00E5FF", .95f));
        plume.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Flamme intermédiaire dorée
        var mid = Cylinder(root.transform, "Mid", new Vector3(0, 0, -.48f), new Vector3(.18f, .55f, .18f), Mat("FFB800", .95f));
        mid.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Cœur plasma blanc chaud
        var core = Cylinder(root.transform, "Coeur", new Vector3(0, 0, -.32f), new Vector3(.11f, .38f, .11f), Mat("FFFFFF", 1f));
        core.transform.localRotation = Quaternion.Euler(90, 0, 0);
        root.SetActive(false);
        return root;
    }

    public static GameObject JumpRamp(Transform parent) {
        var root = new GameObject("Tremplin");
        root.transform.SetParent(parent, false);

        // Corps solide en biseau reposant intégralement sur le sol (Y = 0)
        float w = 3.6f, l = 3.2f, h0 = 0.02f, h1 = 0.82f;
        var wedgeMesh = CreateWedgeMesh(w, l, h0, h1);
        var body = new GameObject("Corps_Tremplin");
        body.transform.SetParent(root.transform, false);
        var mf = body.AddComponent<MeshFilter>();
        mf.sharedMesh = wedgeMesh;
        var mr = body.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Mat("F5B700", .65f, .1f);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;

        // Bandes de guidage et chevrons sur la surface inclinée
        float angle = Mathf.Atan2(h1 - h0, l) * Mathf.Rad2Deg;
        var stripeMat = Mat("1A232E", .4f);
        var yellowMat = Mat("F5B700", .8f);

        // Rebords de sécurité latéraux solides
        for (int side = -1; side <= 1; side += 2) {
            var curbMesh = CreateWedgeMesh(0.24f, l + 0.1f, h0 + 0.22f, h1 + 0.28f);
            var curb = new GameObject(side < 0 ? "Rebord_G" : "Rebord_D");
            curb.transform.SetParent(root.transform, false);
            curb.transform.localPosition = new Vector3(side * (w * 0.5f - 0.12f), 0, 0);
            var cmf = curb.AddComponent<MeshFilter>();
            cmf.sharedMesh = curbMesh;
            var cmr = curb.AddComponent<MeshRenderer>();
            cmr.sharedMaterial = stripeMat;
            cmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            cmr.receiveShadows = true;
        }

        // Flèches d'accélération orientées sur la pente
        for (int k = -1; k <= 1; k++) {
            float zRel = k * 0.72f;
            float t = (zRel + l * 0.5f) / l;
            float yRel = Mathf.Lerp(h0, h1, t) + 0.015f;
            var arrow = Box(root.transform, "Fleche_" + k, new Vector3(0, yRel, zRel), new Vector3(1.6f, .03f, .28f), stripeMat);
            arrow.transform.localRotation = Quaternion.Euler(-angle, 0, 0);
        }

        return root;
    }

    public static Mesh CreateWedgeMesh(float width, float length, float h0, float h1) {
        var mesh = new Mesh { name = "Wedge" };
        float w2 = width * 0.5f, l2 = length * 0.5f;

        Vector3 p0 = new Vector3(-w2, 0, -l2); // sol avant gauche
        Vector3 p1 = new Vector3( w2, 0, -l2); // sol avant droit
        Vector3 p2 = new Vector3( w2, 0,  l2); // sol arrière droit
        Vector3 p3 = new Vector3(-w2, 0,  l2); // sol arrière gauche
        Vector3 p4 = new Vector3(-w2, h0, -l2); // crête avant gauche
        Vector3 p5 = new Vector3( w2, h0, -l2); // crête avant droit
        Vector3 p6 = new Vector3( w2, h1,  l2); // crête arrière droit
        Vector3 p7 = new Vector3(-w2, h1,  l2); // crête arrière gauche

        var verts = new List<Vector3>();
        var tris = new List<int>();

        System.Action<Vector3, Vector3, Vector3, Vector3> addQuad = (a, b, c, d) => {
            int idx = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
            tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
            tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 3);
        };

        // Dessus incliné
        addQuad(p4, p5, p6, p7);
        // Sol
        addQuad(p3, p2, p1, p0);
        // Mur arrière vertical
        addQuad(p7, p6, p2, p3);
        // Entrée avant
        addQuad(p0, p1, p5, p4);
        // Côté gauche
        addQuad(p3, p0, p4, p7);
        // Côté droit
        addQuad(p1, p2, p6, p5);

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static GameObject TrafficCone(Transform parent) {
        var root = new GameObject("Cone");
        root.transform.SetParent(parent, false);

        // Socle noir lesté à bords biseautés
        Box(root.transform, "SocleLeste", new Vector3(0, .04f, 0), new Vector3(.76f, .08f, .76f), Mat("1A1A1A", .4f));
        Box(root.transform, "SocleBase", new Vector3(0, .085f, 0), new Vector3(.66f, .03f, .66f), Mat("FF4800", .6f));

        // Corps du cône conique effilé
        Cylinder(root.transform, "ConeBase", new Vector3(0, .24f, 0), new Vector3(.44f, .30f, .44f), Mat("FF4800", .6f));
        Cylinder(root.transform, "Bande1", new Vector3(0, .42f, 0), new Vector3(.35f, .16f, .35f), Mat("FFFFFF", .85f, .1f));
        Cylinder(root.transform, "ConeMilieu", new Vector3(0, .54f, 0), new Vector3(.28f, .14f, .28f), Mat("FF4800", .6f));
        Cylinder(root.transform, "Bande2", new Vector3(0, .66f, 0), new Vector3(.22f, .14f, .22f), Mat("FFFFFF", .85f, .1f));
        Cylinder(root.transform, "ConePointe", new Vector3(0, .78f, 0), new Vector3(.16f, .14f, .16f), Mat("FF4800", .6f));
        Cylinder(root.transform, "CollierNoir", new Vector3(0, .85f, 0), new Vector3(.11f, .03f, .11f), Mat("1A1A1A", .4f));

        var obs = root.AddComponent<GPObstacle>();
        obs.IsBarrel = false;
        obs.Radius = 1.0f;
        return root;
    }

    public static GameObject Barrel(Transform parent) {
        var root = new GameObject("Baril");
        root.transform.SetParent(parent, false);

        var redMat = Mat("D32F2F", .7f, .25f);
        var rimMat = Mat("2B2D42", .6f, .5f);
        var hazardMat = Mat("FBC02D", .8f);

        // Fût en acier avec cannelures de renfort
        Cylinder(root.transform, "CorpsFut", new Vector3(0, .60f, 0), new Vector3(.80f, 1.18f, .80f), redMat);
        // Rebord supérieur et inférieur
        Cylinder(root.transform, "RebordHaut", new Vector3(0, 1.18f, 0), new Vector3(.83f, .05f, .83f), rimMat);
        Cylinder(root.transform, "RebordBas", new Vector3(0, .03f, 0), new Vector3(.83f, .05f, .83f), rimMat);
        // Bourrelets centraux de sertissage
        Cylinder(root.transform, "AnneauRenfort1", new Vector3(0, .42f, 0), new Vector3(.84f, .06f, .84f), rimMat);
        Cylinder(root.transform, "AnneauRenfort2", new Vector3(0, .78f, 0), new Vector3(.84f, .06f, .84f), rimMat);
        // Bande d'avertissement de sécurité
        Cylinder(root.transform, "BandeSecurite", new Vector3(0, .60f, 0), new Vector3(.81f, .22f, .81f), hazardMat);
        Cylinder(root.transform, "BandeNoire", new Vector3(0, .60f, 0), new Vector3(.815f, .08f, .815f), rimMat);

        var obs = root.AddComponent<GPObstacle>();
        obs.IsBarrel = true;
        obs.Radius = 1.25f;
        return root;
    }

    public static GameObject Spectator(Transform parent, Vector3 pos, Quaternion rot, string color) {
        var root = new GameObject("Spectateur");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        root.transform.localRotation = rot;
        var body = Cylinder(root.transform, "Corps", new Vector3(0, .45f, 0), new Vector3(.46f, .45f, .46f), Mat(color, .4f));
        var head = Sphere(root.transform, "Tete", new Vector3(0, 1.05f, 0), Vector3.one * .42f, Mat("FFE3C2", .4f));
        var cap = Box(root.transform, "Casquette", new Vector3(0, 1.22f, .06f), new Vector3(.44f, .12f, .55f), Mat(color, .5f));
        var anim = root.AddComponent<GPAnimatedDecor>();
        anim.Type = GPAnimType.Spectator;
        anim.Part1 = head.transform;
        return root;
    }

    public static void Decor(Transform parent, int theme, List<Vector3> path, float width) {
        var random = new System.Random(127 + theme);
        var wood = Mat(theme == 0 ? "DDB17A" : theme == 1 ? "647785" : "7CAE72");
        Box(parent, "Table", new Vector3(0, -.7f, 0), new Vector3(110, 1.2f, 100), wood);

        if (theme < 2) {
            for (int i = -5; i <= 5; i++)
                Box(parent, "Joint de plateau", new Vector3(i * 10, -.089f, 0), new Vector3(.07f, .018f, 100), Mat(theme == 0 ? "BF925E" : "5E707B"));
        }

        // Décors thématiques animés
        if (theme == 0) {
            // Grille-pain animé avec toasts
            var toaster = new GameObject("Grille-pain anime").transform;
            toaster.SetParent(parent, false);
            toaster.localPosition = new Vector3(-18f, 0, 10f);
            Box(toaster, "Corps", new Vector3(0, 1.5f, 0), new Vector3(3.6f, 3f, 2.2f), Mat("E2E8F0", .9f, .8f));
            var toast = Box(toaster, "Toast", new Vector3(0, 1.8f, 0), new Vector3(2.4f, 2.2f, .35f), Mat("C48847", .3f));
            var tAnim = toaster.gameObject.AddComponent<GPAnimatedDecor>();
            tAnim.Type = GPAnimType.Toaster;
            tAnim.Part1 = toast.transform;
        } else if (theme == 1) {
            // Ventilateur de bureau avec pales tournantes
            var fan = new GameObject("Ventilateur anime").transform;
            fan.SetParent(parent, false);
            fan.localPosition = new Vector3(20f, 0, -8f);
            Cylinder(fan, "Pied", new Vector3(0, .2f, 0), new Vector3(3f, .2f, 3f), Mat("334155", .6f));
            Cylinder(fan, "Tige", new Vector3(0, 2f, 0), new Vector3(.4f, 1.8f, .4f), Mat("94A3B8", .8f, .6f));
            var head = Box(fan, "Moteur", new Vector3(0, 3.8f, 0), new Vector3(1.2f, 1.2f, 1.8f), Mat("334155", .6f));
            var rotor = new GameObject("Pales").transform;
            rotor.SetParent(head.transform, false);
            rotor.localPosition = new Vector3(0, 0, 1.1f);
            for (int k = 0; k < 4; k++) {
                var blade = Box(rotor, "Pale", Vector3.zero, new Vector3(.3f, 2.2f, .04f), Mat("38BDF8", .7f));
                blade.transform.localRotation = Quaternion.Euler(0, 0, k * 90);
            }
            var fAnim = fan.gameObject.AddComponent<GPAnimatedDecor>();
            fAnim.Type = GPAnimType.Fan;
            fAnim.Part1 = rotor;
        } else {
            // Moulin à vent animé
            var mill = new GameObject("Moulin anime").transform;
            mill.SetParent(parent, false);
            mill.localPosition = new Vector3(-20f, 0, -8f);
            Cylinder(mill, "Tour", new Vector3(0, 4.5f, 0), new Vector3(4f, 4.5f, 4f), Mat("FFF1D4", .4f));
            var roof = Sphere(mill, "Toit", new Vector3(0, 9f, 0), new Vector3(4.4f, 2.2f, 4.4f), Mat("EF4444", .6f));
            var sails = new GameObject("Ailes").transform;
            sails.SetParent(roof.transform, false);
            sails.localPosition = new Vector3(0, 0, 2.3f);
            for (int k = 0; k < 4; k++) {
                var sail = Box(sails, "Aile", Vector3.zero, new Vector3(.5f, 7.5f, .06f), Mat("F8FAFC", .8f));
                sail.transform.localRotation = Quaternion.Euler(0, 0, k * 90);
            }
            var mAnim = mill.gameObject.AddComponent<GPAnimatedDecor>();
            mAnim.Type = GPAnimType.Windmill;
            mAnim.Part1 = sails;

            // Papillons
            for (int b = 0; b < 3; b++) {
                var bfly = new GameObject("Papillon " + b);
                bfly.transform.SetParent(parent, false);
                bfly.transform.localPosition = new Vector3(15f + b * 5f, 1.5f, 12f - b * 4f);
                Box(bfly.transform, "AileG", new Vector3(-.3f, 0, 0), new Vector3(.5f, .03f, .4f), Mat("F472B6", .8f));
                Box(bfly.transform, "AileD", new Vector3(.3f, 0, 0), new Vector3(.5f, .03f, .4f), Mat("F472B6", .8f));
                var bAnim = bfly.AddComponent<GPAnimatedDecor>();
                bAnim.Type = GPAnimType.Butterfly;
            }
        }

        // Décors statiques habituels
        for (int i = 0; i < 66; i++) {
            var pos = new Vector3((float)random.NextDouble() * 100 - 50, 0, (float)random.NextDouble() * 88 - 44);
            float min = 1000;
            foreach (var q in path) min = Mathf.Min(min, Vector3.Distance(q, pos));
            if (min < width * .5f + 5.5f || pos.magnitude < 4) continue;
            var root = new GameObject("Decor " + i).transform;
            root.SetParent(parent, false);
            root.localPosition = pos;
            root.localRotation = Quaternion.Euler(0, random.Next(360), 0);

            string[] palette = { "F28D79", "67C7BB", "ECC860", "98A0DA" };
            Material col = Mat(palette[i % 4], .5f);

            if (theme == 0) {
                if (i % 3 == 0) {
                    Cylinder(root, "Tasse", new Vector3(0, 2, 0), new Vector3(4, 2, 4), col);
                    Cylinder(root, "Cafe", new Vector3(0, 4.02f, 0), new Vector3(3.4f, .025f, 3.4f), Mat("56382D"));
                    Sphere(root, "Anse", new Vector3(2, 2, 0), new Vector3(2.1f, 2.6f, 1), col);
                } else if (i % 3 == 1) {
                    Cylinder(root, "Assiette", new Vector3(0, .2f, 0), new Vector3(7, .17f, 7), Mat("FFF1D4", .65f));
                    for (int k = 0; k < 3; k++) Cylinder(root, "Biscuit", new Vector3(k - 1, .65f + k * .2f, 0), new Vector3(2.5f, .24f, 2.5f), Mat("C48847"));
                } else {
                    Box(root, "Boite cereales", new Vector3(0, 3, 0), new Vector3(4, 6, 1.9f), col);
                    Box(root, "Etiquette", new Vector3(0, 3, 1), new Vector3(3, 3, .03f), Mat("FFF2CC"));
                }
            } else if (theme == 1) {
                if (i % 3 == 0) {
                    Box(root, "Livre", new Vector3(0, .5f, 0), new Vector3(7, 1, 5), col);
                    Box(root, "Pages", new Vector3(0, .51f, 0), new Vector3(6.7f, .65f, 4.8f), Mat("FFF4DB"));
                    Box(root, "Couverture", new Vector3(0, 1.04f, 0), new Vector3(7, .15f, 5), col);
                } else if (i % 3 == 1) {
                    var pencil = Cylinder(root, "Crayon", new Vector3(0, .6f, 0), new Vector3(.8f, 5, .8f), col);
                    pencil.transform.localRotation = Quaternion.Euler(90, 0, 0);
                } else {
                    Box(root, "Gomme", new Vector3(0, .7f, 0), new Vector3(3, 1.4f, 2), col);
                    Box(root, "Bande papier", new Vector3(0, .72f, 0), new Vector3(1.3f, 1.45f, 2.03f), Mat("F5EEE0"));
                }
            } else {
                if (i % 3 == 0) {
                    Cylinder(root, "Pot", new Vector3(0, 1.2f, 0), new Vector3(3.5f, 1.2f, 3.5f), Mat("C97755"));
                    Cylinder(root, "Tronc", new Vector3(0, 3, 0), new Vector3(.55f, 2, .55f), Mat("886347"));
                    Sphere(root, "Feuillage", new Vector3(0, 5, 0), new Vector3(5, 5, 5), Mat(i % 2 == 0 ? "4D9166" : "65AC70"));
                } else if (i % 3 == 1) {
                    Sphere(root, "Galet", new Vector3(0, .65f, 0), new Vector3(3.8f, 1.8f, 3), Mat("A4ADB1", .4f));
                } else {
                    Cylinder(root, "Tige", new Vector3(0, 1, 0), new Vector3(.2f, 1, .2f), Mat("4A8055"));
                    for (int k = 0; k < 5; k++) {
                        float a = k * Mathf.PI * 2 / 5;
                        Sphere(root, "Petale", new Vector3(Mathf.Cos(a) * .7f, 2, Mathf.Sin(a) * .7f), new Vector3(1.2f, .4f, 1.2f), col);
                    }
                    Sphere(root, "Coeur", new Vector3(0, 2.2f, 0), Vector3.one * .65f, Mat("FFE380"));
                }
            }
        }
    }
}
}
