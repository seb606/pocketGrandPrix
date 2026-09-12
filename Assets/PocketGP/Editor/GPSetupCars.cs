using UnityEngine;
using UnityEditor;
using System.IO;

namespace PocketGP.Editor {
public static class GPSetupCars {
    [MenuItem("Pocket GP/Configurer les voitures 3D")]
    public static void Setup() {
        string resDir = "Assets/PocketGP/Resources/Cars";
        Directory.CreateDirectory(resDir);

        string fbxPath = "Assets/Cars/racing_car/Meshes/ARCADE - FREE Racing Car.fbx";
        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (!modelAsset) {
            Debug.LogError("Impossible de charger le modèle FBX à : " + fbxPath);
            return;
        }

        // Configuration de l'importeur de texture pour une netteté maximale
        string texDir = "Assets/Cars/racing_car/Textures/Color Variations";
        string[] texFiles = {
            "AFRC_Tex_Col1.png",
            "AFRC_Tex_Col2.png",
            "AFRC_Tex_Col3.png",
            "AFRC_Tex_Col4.png",
            "AFRC_Tex_Col5.png"
        };

        Material[] mats = new Material[5];
        Shader shader = Shader.Find("Standard");
        if (!shader) shader = Shader.Find("Mobile/Diffuse");

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
            string matPath = resDir + "/Mat_Car_" + i + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat) {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.shader = shader;
            mat.mainTexture = tex;
            if (mat.HasProperty("_Color")) mat.color = Color.white;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.35f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.08f);
            EditorUtility.SetDirty(mat);
            mats[i] = mat;
        }

        // Mesure des dimensions du FBX original
        var temp = Object.Instantiate(modelAsset);
        temp.transform.position = Vector3.zero;
        temp.transform.rotation = Quaternion.identity;
        temp.transform.localScale = Vector3.one;

        var bounds = new Bounds(temp.transform.position, Vector3.zero);
        var renderers = temp.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        Debug.Log("Dimensions d'origine du FBX : " + bounds.size + " - Centre : " + bounds.center);

        // Facteur d'échelle pour obtenir une longueur de voiture d'environ 1.85m
        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float targetScale = 1.85f / Mathf.Max(0.01f, maxDim);

        Object.DestroyImmediate(temp);

        // Création des 5 prefabs de voiture
        for (int i = 0; i < 5; i++) {
            var carRoot = new GameObject("Car_" + i);
            var visual = Object.Instantiate(modelAsset, carRoot.transform);
            visual.name = "Model";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * targetScale;

            // Recalculer le centrage Y pour poser les roues sur le sol
            var carBounds = new Bounds(carRoot.transform.position, Vector3.zero);
            foreach (var r in carRoot.GetComponentsInChildren<Renderer>()) {
                carBounds.Encapsulate(r.bounds);
                r.sharedMaterial = mats[i];
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }
            float yOffset = -carBounds.min.y;
            visual.transform.localPosition = new Vector3(0, yOffset, 0);

            string prefabPath = resDir + "/Car_" + i + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(carRoot, prefabPath);
            Object.DestroyImmediate(carRoot);
            Debug.Log("Prefab voiture créé : " + prefabPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Configuration des 5 voitures 3D terminée avec succès !");
    }
}
}
