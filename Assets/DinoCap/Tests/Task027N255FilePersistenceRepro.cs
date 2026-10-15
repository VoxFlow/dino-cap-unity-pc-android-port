#if UNITY_EDITOR
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    public sealed class Task027N255FilePersistenceRepro
    {
        IDisposable storage;
        [SetUp] public void Isolate()
        {
            storage=DC1Preferences.TestHarness_UseDirectory(Path.GetFullPath(Path.Combine(
                Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",Guid.NewGuid().ToString("N"))));
        }
        [TearDown] public void Restore()=>storage.Dispose();

        [Test] public void UncommittedEditsDisappearOnRestartAndCommittedValuesSurvive()
        {
            DC1Preferences.SetInt("cash",140);DC1Preferences.SetFloat("hp",75);DC1Preferences.SetString("mode","City");
            DC1Preferences.Save();
            DC1Preferences.SetInt("cash",999);DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.GetInt("cash"),Is.EqualTo(140));
            Assert.That(DC1Preferences.GetFloat("hp"),Is.EqualTo(75));
            Assert.That(DC1Preferences.GetString("mode"),Is.EqualTo("City"));
        }
        [Test] public void CorruptPrimaryRecoversLastCommittedBackup()
        {
            DC1Preferences.SetInt("cash",140);DC1Preferences.Save();
            DC1Preferences.SetInt("cash",300);DC1Preferences.Save();
            string file=DC1Preferences.SaveFilePath;
            File.WriteAllText(file,"broken JSON");DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.GetInt("cash"),Is.EqualTo(140));
            DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.GetInt("cash"),Is.EqualTo(140));
        }
        [Test] public void ResetRemovesCampaignSettingsAndRecoveryBackupAcrossRestart()
        {
            DC1Preferences.SetInt("campaign",999999);DC1Preferences.Save();
            DC1DebugSettings.Enabled=true;DC1DebugSettings.AffectsSaves=false;
            string file=DC1Preferences.SaveFilePath;
            DC1Preferences.ResetAll();DC1Preferences.TestHarness_Reload();
            Assert.That(DC1Preferences.HasKey("campaign"),Is.False);
            Assert.That(DC1DebugSettings.Enabled,Is.False);
            Assert.That(DC1DebugSettings.AffectsSaves,Is.True);
            Assert.That(File.Exists(file+".bak"),Is.False);
        }
        [Test] public void DebugPoliciesPersistIndependentlyAcrossRestart()
        {
            DC1DebugSettings.Enabled=true;DC1DebugSettings.AffectsSaves=false;
            DC1Preferences.TestHarness_Reload();
            Assert.That(DC1DebugSettings.Enabled,Is.True);Assert.That(DC1DebugSettings.AffectsSaves,Is.False);
            DC1DebugSettings.Enabled=false;DC1Preferences.TestHarness_Reload();
            Assert.That(DC1DebugSettings.Enabled,Is.False);Assert.That(DC1DebugSettings.AffectsSaves,Is.False);
        }
        [Test] public void UnreadablePrimaryAndBackupArePreservedInsteadOfSilentlyResetting()
        {
            string file=DC1Preferences.SaveFilePath;
            File.WriteAllText(file,"bad primary");File.WriteAllText(file+".bak","bad backup");
            DC1Preferences.TestHarness_Reload();
            Assert.Throws<InvalidDataException>(()=>DC1Preferences.GetInt("cash"));
            Assert.That(File.ReadAllText(file),Is.EqualTo("bad primary"));
            Assert.That(File.ReadAllText(file+".bak"),Is.EqualTo("bad backup"));
        }
        [TestCase(1024,768)] [TestCase(1920,1080)] [TestCase(2560,1440)]
        [TestCase(3840,2160)] [TestCase(3440,1440)] [TestCase(5120,1440)] [TestCase(5760,1080)]
        public void DebugViewportIsBoundedAndDoesNotShrinkText(int width,int height)
        {
            Rect rect=DC1DebugPanel.Viewport(width,height,460);
            Assert.That(rect.xMax,Is.LessThanOrEqualTo(width));Assert.That(rect.yMax,Is.LessThanOrEqualTo(height));
            Assert.That(rect.width,Is.LessThanOrEqualTo(width*.55f));
            Assert.That(rect.height,Is.LessThanOrEqualTo(height*.55f));
            Assert.That(DC1DebugPanel.ScaleForHeight(height),Is.GreaterThanOrEqualTo(1));
        }
    }
}
#endif
