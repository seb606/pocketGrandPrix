using UnityEngine;
using UnityEditor;
using System.IO;

namespace PocketGP.Editor {
public static class GPSetupCars {
    [MenuItem("Pocket GP/Configurer les voitures 3D")]
    public static void Setup() {
        string resDir = "Assets/PocketGP/Resources/Cars";
        Directory.CreateDirectory(resDir);

        Shader standardShader = Shader.Find("Standard");
        if (!standardShader) standardShader = Shader.Find("Mobile/Diffuse");

        Color[] teamColors = new Color[] {
            new Color(0.92f, 0.12f, 0.14f), // 0: Rouge vif
            new Color(0.00f, 0.85f, 0.82f), // 1: Cyan électrique
            new Color(1.00f, 0.82f, 0.10f), // 2: Jaune racing
            new Color(0.20f, 0.35f, 0.98f), // 3: Bleu roi
            new Color(0.96f, 0.45f, 0.72f)  // 4: Rose / Magenta sport
        };

        // ==========================================
        // 1. GT / RACING CAR
        // ==========================================
        string gtFbx = "Assets/Cars/racing_car/Meshes/ARCADE - FREE Racing Car.fbx";
        var gtAsset = AssetDatabase.LoadAssetAtPath<GameObject>(gtFbx);
        if (gtAsset) {
            string texDir = "Assets/Cars/racing_car/Textures/Color Variations";
            string[] texFiles = {
                "AFRC_Tex_Col1.png",
                "AFRC_Tex_Col2.png",
                "AFRC_Tex_Col3.png",
                "AFRC_Tex_Col4.png",
                "AFRC_Tex_Col5.png"
            };

            Material[] gtMats = new Material[5];
            for (int i = 0; i < 5; i++) {
                string texPath = texDir + "/" + texFiles[i];
                var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null) {
                    importer.textureType = TextureImporterType.Default;
                    importer.filterMode = FilterMode.Trilinear;
                    importer.anisoLevel = 16;
                    importer.mipMapBias = -0.5f;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                string matPath = resDir + "/Mat_GT_" + i + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (!mat) {
                    mat = new Material(standardShader);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                mat.shader = standardShader;
                mat.mainTexture = tex;
                if (mat.HasProperty("_Color")) mat.color = Color.white;
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.40f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.10f);
                EditorUtility.SetDirty(mat);
                gtMats[i] = mat;
            }

            BuildCarPrefabs(gtAsset, "GT", gtMats, 1.85f, null);
            BuildCarPrefabs(gtAsset, "Car", gtMats, 1.85f, null); // fallback par défaut
        }

        // ==========================================
        // 2. F1 / LOW POLY FORMULA 1
        // ==========================================
        string f1Fbx = "Assets/Cars/f1/lowpolyf1.fbx";
        var f1Asset = AssetDatabase.LoadAssetAtPath<GameObject>(f1Fbx);
        if (f1Asset) {
            Material f1Carbon = GetOrCreateMaterial(resDir + "/Mat_F1_Carbon.mat", standardShader, new Color(0.12f, 0.13f, 0.15f), 0.25f, 0.2f);
            Material f1Tire = GetOrCreateMaterial(resDir + "/Mat_F1_Tire.mat", standardShader, new Color(0.08f, 0.08f, 0.09f), 0.15f, 0.05f);
            Material f1Plank = GetOrCreateMaterial(resDir + "/Mat_F1_Plank.mat", standardShader, new Color(0.55f, 0.42f, 0.28f), 0.1f, 0.0f);
            Material f1Mirror = GetOrCreateMaterial(resDir + "/Mat_F1_Mirror.mat", standardShader, new Color(0.85f, 0.88f, 0.92f), 0.9f, 0.8f);
            Material f1Backlight = GetOrCreateMaterial(resDir + "/Mat_F1_Backlight.mat", standardShader, new Color(1.0f, 0.1f, 0.1f), 0.5f, 0.1f);

            Material[] f1BodyMats = new Material[5];
            for (int i = 0; i < 5; i++) {
                string matPath = resDir + "/Mat_F1_Body_" + i + ".mat";
                f1BodyMats[i] = GetOrCreateMaterial(matPath, standardShader, teamColors[i], 0.55f, 0.15f);
            }

            System.Action<GameObject, int> f1Customizer = (root, colorIdx) => {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) {
                    string rName = r.name.ToLower();
                    if (rName.Contains("tire")) {
                        r.sharedMaterial = f1Tire;
                    } else if (rName.Contains("suspension") || rName.Contains("cover") || rName.Contains("steer") || rName.Contains("spring") || rName.Contains("exhaust") || rName.Contains("cockpit")) {
                        r.sharedMaterial = f1Carbon;
                    } else if (rName.Contains("bottom")) {
                        r.sharedMaterials = new Material[] { f1Carbon, f1Plank };
                    } else if (rName.Contains("mirror")) {
                        r.sharedMaterial = f1Mirror;
                    } else if (rName.Contains("light")) {
                        r.sharedMaterial = f1Backlight;
                    } else {
                        // Body, Body Halo, Front Wing Base, Rear Wing, Rims, etc.
                        r.sharedMaterial = f1BodyMats[colorIdx];
                    }
                }
            };

            BuildCarPrefabs(f1Asset, "F1", f1BodyMats, 2.15f, f1Customizer);
        }

        // ==========================================
        // 3. MUSCLE CAR
        // ==========================================
        string muscleFbx = "Assets/Cars/muscle/muscle-car.fbx";
        var muscleAsset = AssetDatabase.LoadAssetAtPath<GameObject>(muscleFbx);
        if (muscleAsset) {
            Material muscleDark = GetOrCreateMaterial(resDir + "/Mat_Muscle_Dark.mat", standardShader, new Color(0.10f, 0.11f, 0.12f), 0.30f, 0.1f);
            Material muscleTire = GetOrCreateMaterial(resDir + "/Mat_Muscle_Tire.mat", standardShader, new Color(0.08f, 0.08f, 0.09f), 0.15f, 0.05f);
            Material muscleRim = GetOrCreateMaterial(resDir + "/Mat_Muscle_Rim.mat", standardShader, new Color(0.85f, 0.85f, 0.85f), 0.80f, 0.75f);
            Material muscleGlass = GetOrCreateMaterial(resDir + "/Mat_Muscle_Glass.mat", standardShader, new Color(0.12f, 0.18f, 0.22f), 0.95f, 0.3f);
            Material muscleHeadlight = GetOrCreateMaterial(resDir + "/Mat_Muscle_Headlight.mat", standardShader, new Color(1f, 0.98f, 0.85f), 0.9f, 0.2f);
            Material muscleTaillight = GetOrCreateMaterial(resDir + "/Mat_Muscle_Taillight.mat", standardShader, new Color(0.9f, 0.1f, 0.1f), 0.7f, 0.1f);

            Material[] muscleBodyMats = new Material[5];
            for (int i = 0; i < 5; i++) {
                string matPath = resDir + "/Mat_Muscle_Body_" + i + ".mat";
                muscleBodyMats[i] = GetOrCreateMaterial(matPath, standardShader, teamColors[i], 0.50f, 0.12f);
            }

            System.Action<GameObject, int> muscleCustomizer = (root, colorIdx) => {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) {
                    string rName = r.name.ToLower();
                    if (rName.Contains("tire")) {
                        r.sharedMaterial = muscleTire;
                    } else if (rName.Contains("rim")) {
                        r.sharedMaterial = muscleRim;
                    } else if (rName.Contains("brake")) {
                        r.sharedMaterial = muscleDark;
                    } else if (rName.Contains("glass") || rName.Contains("window")) {
                        r.sharedMaterial = muscleGlass;
                    } else if (rName.Contains("body") || rName.Contains("door")) {
                        // Remplacer les slots de matériau
                        var mats = r.sharedMaterials;
                        for (int m = 0; m < mats.Length; m++) {
                            string mName = mats[m] ? mats[m].name.ToLower() : "";
                            if (mName.Contains("body") || m == 0) mats[m] = muscleBodyMats[colorIdx];
                            else if (mName.Contains("glass")) mats[m] = muscleGlass;
                            else if (mName.Contains("light") || mName.Contains("lamp")) mats[m] = muscleHeadlight;
                            else mats[m] = muscleDark;
                        }
                        r.sharedMaterials = mats;
                    } else {
                        r.sharedMaterial = muscleDark;
                    }
                }
            };

            BuildCarPrefabs(muscleAsset, "Muscle", muscleBodyMats, 2.05f, muscleCustomizer);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== Configuration des 3 modèles de voitures (GT, F1, Muscle) terminée avec succès ! ===");
    }

    static Material GetOrCreateMaterial(string path, Shader shader, Color color, float gloss, float metallic) {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.shader = shader;
        if (mat.HasProperty("_Color")) mat.color = color;
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void BuildCarPrefabs(GameObject modelAsset, string prefix, Material[] mats, float targetLength, System.Action<GameObject, int> customizer) {
        string resDir = "Assets/PocketGP/Resources/Cars";
        var temp = Object.Instantiate(modelAsset);
        temp.transform.position = Vector3.zero;
        temp.transform.rotation = Quaternion.identity;
        temp.transform.localScale = Vector3.one;

        var bounds = new Bounds(temp.transform.position, Vector3.zero);
        foreach (var r in temp.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float targetScale = targetLength / Mathf.Max(0.01f, maxDim);
        Object.DestroyImmediate(temp);

        for (int i = 0; i < 5; i++) {
            var carRoot = new GameObject(prefix + "_" + i);
            var visual = Object.Instantiate(modelAsset, carRoot.transform);
            visual.name = "Model";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * targetScale;

            if (customizer != null) {
                customizer(visual, i);
            } else {
                foreach (var r in carRoot.GetComponentsInChildren<Renderer>()) {
                    r.sharedMaterial = mats[i];
                }
            }

            var carBounds = new Bounds(carRoot.transform.position, Vector3.zero);
            foreach (var r in carRoot.GetComponentsInChildren<Renderer>()) {
                carBounds.Encapsulate(r.bounds);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }
            float yOffset = -carBounds.min.y;
            visual.transform.localPosition = new Vector3(0, yOffset, 0);

            string prefabPath = resDir + "/" + prefix + "_" + i + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(carRoot, prefabPath);
            Object.DestroyImmediate(carRoot);
            Debug.Log("Prefab créé : " + prefabPath);
        }
    }
}
}
