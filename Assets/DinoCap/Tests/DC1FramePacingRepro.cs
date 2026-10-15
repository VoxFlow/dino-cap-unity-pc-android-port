#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    // User-approved FPS setting 2026-10-13: default 60, persisted, desktop adds 120/240/UNLIMITED + V-Sync.
    public class DC1FramePacingRepro
    {
        // The rest of the suite runs uncapped in batch mode (see DC1FramePacing.Startup); undo what these tests applied.
        [OneTimeTearDown] public void RestoreBatchUncapped()
        {
            DC1Preferences.DeleteKey(DC1FramePacing.LimitKey); DC1Preferences.DeleteKey(DC1FramePacing.VSyncKey);
            if (Application.isBatchMode) { QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; }
        }

        [TearDown] public void Reset()
        {
            DC1Preferences.DeleteKey(DC1FramePacing.LimitKey); DC1Preferences.DeleteKey(DC1FramePacing.VSyncKey);
            DC1FramePacing.Apply();
        }

        [Test] public void DefaultsToSixtyWithVSyncOn()
        {
            Reset();
            Assert.AreEqual(60, DC1FramePacing.Limit);
            Assert.IsTrue(DC1FramePacing.VSync);
            Assert.AreEqual(1, QualitySettings.vSyncCount);
            Assert.AreEqual(60, Application.targetFrameRate);
        }

        [Test] public void CycleVisitsAllDesktopLimitsAndPersists()
        {
            Reset();
            var seen = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 4; i++) { DC1FramePacing.CycleLimit(); seen.Add(DC1FramePacing.Limit); }
            CollectionAssert.AreEqual(new[] { 120, 240, 0, 60 }, seen);
            DC1FramePacing.CycleLimit();                       // 120
            DC1Preferences.TestHarness_Reload();
            Assert.AreEqual(120, DC1FramePacing.Limit, "The chosen limit survives a reload.");
            Assert.AreEqual("120", DC1FramePacing.LimitName);
        }

        [Test] public void VSyncOffAppliesTheLimitAndUnlimitedUncaps()
        {
            Reset();
            DC1FramePacing.ToggleVSync();
            Assert.AreEqual(0, QualitySettings.vSyncCount);
            Assert.AreEqual(60, Application.targetFrameRate);
            for (int i = 0; i < 3; i++) DC1FramePacing.CycleLimit();   // 120,240,unlimited
            Assert.AreEqual("UNLIMITED", DC1FramePacing.LimitName);
            Assert.AreEqual(-1, Application.targetFrameRate);
        }

        [Test] public void InvalidSavedLimitFallsBackToSixty()
        {
            DC1Preferences.SetInt(DC1FramePacing.LimitKey, 77);
            Assert.AreEqual(60, DC1FramePacing.Limit);
        }
    }
}
#endif
