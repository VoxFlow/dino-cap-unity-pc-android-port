#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DinoCap.PortV2.EditorTools
{
    public static class DinoCapCombatSandboxV2Builder
    {
        [MenuItem("DinoCap/Build Combat Sandbox V2")]
        public static void Build()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            GameObject root=new GameObject("CombatSandboxV2"); root.AddComponent<DinoCap.PortV2.CombatSandboxV2>();
            System.IO.Directory.CreateDirectory("Assets/DinoCap/Scenes");
            EditorSceneManager.SaveScene(scene,"Assets/DinoCap/Scenes/CombatSandboxV2.unity");
            Selection.activeGameObject=root;
            Debug.Log("[DC1-V2] Built Assets/DinoCap/Scenes/CombatSandboxV2.unity. Press Play.");
        }
    }
}
#endif
