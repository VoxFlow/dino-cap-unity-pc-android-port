#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    // QoL 2026-10-13: first-use lag in the Main Menu (Options slide-in) came from lazy texture/audio loading.
    // The menu now preloads its textures + click sound once per process, a few per frame, and really caches textures.
    public class DC1MenuPreloadRepro
    {
        const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator MenuPreloadCompletesAndPopulatesTheTextureCache()
        {
            typeof(MainMenuController).GetField("menuAssetsPreloaded", Static).SetValue(null, false);
            ((Dictionary<string, Texture2D>)typeof(MainMenuController).GetField("textureCache", Static).GetValue(null)).Clear();
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            float deadline = Time.realtimeSinceStartup + 20f;
            var cache = (Dictionary<string, Texture2D>)typeof(MainMenuController).GetField("textureCache", Static).GetValue(null);
            var flag = typeof(MainMenuController).GetField("menuAssetsPreloaded", Static);
            while (Time.realtimeSinceStartup < deadline && !(bool)flag.GetValue(null)) yield return null;   // completion flag is set only when the preload finishes
            Assert.GreaterOrEqual(cache.Count, 20, "Menu textures must be preloaded into the cache shortly after the menu starts.");
            Assert.IsTrue((bool)typeof(MainMenuController).GetField("menuAssetsPreloaded", Static).GetValue(null));
            Assert.IsNotNull(cache["menu_bgFrame"], "The background texture must be cached.");
            var get = typeof(MainMenuController).GetMethod("CachedTexture", Static);
            Assert.AreSame(cache["menu_bgFrame"], get.Invoke(null, new object[] { "menu_bgFrame" }), "CachedTexture must return the cached instance.");
        }

        // Native: assets of the gameplay scene are loaded when the "Loading..." slide completes (menu state 4), before the scene appears.
        [UnityTest]
        public IEnumerator GameplayAssetsArePreloadedDuringTheLoadingSequenceBeforeTheSceneStarts()
        {
            foreach(var n in new[]{"cityAssetsPreloaded","jungleAssetsPreloaded","gameplayAudioPreloaded"})
                typeof(MainMenuController).GetField(n, Static).SetValue(null, false);
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/MainMenuScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            var menu = Object.FindFirstObjectByType<MainMenuController>();
            menu.TestHarness_SkipBootSplash();
            menu.TestHarness_GoLifeMode();                       // Jungle path: leave animation -> "Loading..." slide -> load
            float deadline = Time.realtimeSinceStartup + 40f;
            bool sawPendingWithoutScene = false;
            while (Time.realtimeSinceStartup < deadline && SceneManager.GetActiveScene().name != "JungleBlitzScene")
            {
                if (menu != null && menu.TestHarness_GameplayLoadPending) sawPendingWithoutScene = true;
                yield return null;
            }
            Assert.AreEqual("JungleBlitzScene", SceneManager.GetActiveScene().name, "The gameplay scene must still start after the preload.");
            Assert.IsTrue((bool)typeof(MainMenuController).GetField("cityAssetsPreloaded", Static).GetValue(null), "City Grind sprites preloaded.");
            Assert.IsTrue((bool)typeof(MainMenuController).GetField("jungleAssetsPreloaded", Static).GetValue(null), "Jungle Blitz sprites preloaded.");
            Assert.IsTrue(sawPendingWithoutScene, "Preload happens while the Loading state is pending, before the scene changes.");
        }
    }
}
#endif
