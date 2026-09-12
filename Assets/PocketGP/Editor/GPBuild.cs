using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;
namespace PocketGP.Editor {
[InitializeOnLoad]
public static class GPBuild {
    const string Scene="Assets/PocketGP/Scenes/PocketGrandPrix.unity";
    static GPBuild(){EditorApplication.delayCall+=FirstOpen;}
    static void FirstOpen(){
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||File.Exists("ProjectSettings/PocketGPConfigured.txt"))return;
        Configure();EnsureScene();
        if(!Application.isBatchMode)EditorSceneManager.OpenScene(Scene);
        File.WriteAllText("ProjectSettings/PocketGPConfigured.txt","Configuration initiale terminée. Menu Pocket GP pour réappliquer.");
        Debug.Log("Pocket Grand Prix prêt. Cliquez sur Play. Export : Pocket GP > Construire pour le Web.");
    }
    [MenuItem("Pocket GP/1 - Configurer le projet")]
    public static void Configure(){
        PlayerSettings.companyName="Pocket Studio";PlayerSettings.productName="Pocket Grand Prix";PlayerSettings.bundleVersion="1.0.0";
        PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.runInBackground=false;
        PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
        PlayerSettings.defaultWebScreenWidth=1920;PlayerSettings.defaultWebScreenHeight=1080;
        PlayerSettings.WebGL.template="PROJECT:PocketGP";
        // Uncompressed output is deliberately portable to basic static hosting.
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.dataCaching=true;
        PlayerSettings.WebGL.decompressionFallback=false;
        var settings=new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));
        var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();}
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowCascades=2;QualitySettings.antiAliasing=4;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
        AssetDatabase.SaveAssets();
    }
    static void EnsureScene(){if(File.Exists(Scene))return;Directory.CreateDirectory(Path.GetDirectoryName(Scene));var s=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Pocket GP - Runtime bootstrap");EditorSceneManager.SaveScene(s,Scene);}
    [MenuItem("Pocket GP/2 - Ouvrir la scène")]
    public static void OpenScene(){EnsureScene();if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(Scene);}
    [MenuItem("Pocket GP/3 - Construire pour le Web")]
    public static void BuildWeb(){
        Configure();EnsureScene();
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL))throw new System.InvalidOperationException("Installer le module Web Build Support de cet éditeur via Unity Hub.");
        string output="Builds/Web";Directory.CreateDirectory(output);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Export Web échoué : "+report.summary.result+". Consulter la Console Unity.");
        Debug.Log("Export terminé : "+Path.GetFullPath(output));
        if(!Application.isBatchMode)EditorUtility.RevealInFinder(Path.GetFullPath(output));
    }
}
}
