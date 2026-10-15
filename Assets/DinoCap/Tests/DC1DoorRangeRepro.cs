#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DinoCap.PortV2.Tests
{
    // Native sub_CE2C (houseClassiPad proximity, drives the pulsing icon_houseFrame via sub_56030):
    // |player.x - com2.x| < com2.w*0.7 + 5, com2 = the house composite's "md" child.
    // Independent expectations decoded from the a2b IR (frame 0 com2): house1 x=-4.70587 w=116, house2 x=0.555573 w=174, house3 x=5 w=158.
    public class DC1DoorRangeRepro
    {
        CityGridController city;
        static readonly float[] ExpectedCentreOffset = { -4.70587f, 0.555573f, 5f };
        static readonly float[] ExpectedWidth = { 116f, 174f, 158f };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/DinoCap/Scenes/CityGrindScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            city = Object.FindFirstObjectByType<CityGridController>();
            Assert.IsNotNull(city);
            city.TestHarness_SkipLevelIntro();
            city.TestHarness_ClearDinos();
        }

        [UnityTest]
        public IEnumerator DoorRangeIsSymmetricAroundTheMdChildWithNativeHalfWidth()
        {
            Assert.Greater(city.TestHarness_BuildingCount, 0);
            for(int i = 0; i < city.TestHarness_BuildingCount; i++)
            {
                int t = city.TestHarness_BuildingTemplate(i);
                float centre = city.TestHarness_BuildingX(i) + ExpectedCentreOffset[t];
                float half = ExpectedWidth[t] * 0.7f + 5f;
                foreach(var probe in new[]{ (centre - half + 0.5f, true), (centre + half - 0.5f, true), (centre, true),
                                            (centre - half - 0.5f, false), (centre + half + 0.5f, false) })
                {
                    city.TestHarness_SetPlayerX(probe.Item1);
                    yield return null;
                    Assert.AreEqual(probe.Item2, city.TestHarness_DoorInRange(i),
                        "building " + i + " template " + t + " playerX " + probe.Item1 + " centre " + centre + " half " + half);
                }
            }
        }
    }
}
#endif
