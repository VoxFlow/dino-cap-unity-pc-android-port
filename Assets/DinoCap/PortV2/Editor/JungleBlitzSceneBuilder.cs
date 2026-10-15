#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

namespace DinoCap.PortV2.EditorTools
{
    public static class JungleBlitzSceneBuilder
    {
        public static void Build()
        {
            const string path="Assets/DinoCap/Scenes/JungleBlitzScene.unity";
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("JungleBlitz");root.AddComponent<JungleBlitzController>();
            root.AddComponent<CityGridController>();
            EditorSceneManager.SaveScene(scene,path);
            if(!EditorBuildSettings.scenes.Any(s=>s.path==path))
                EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(path,true)}).ToArray();
            AssetDatabase.SaveAssets();Debug.Log("[DC1-JUNGLE] Built standalone JungleBlitzScene.");
        }
    }
}
#endif
