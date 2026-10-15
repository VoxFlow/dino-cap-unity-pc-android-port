#if UNITY_EDITOR
using NUnit.Framework;
using DinoCap.PortV2;

[SetUpFixture]
public sealed class DC1FilePreferencesScope
{
    System.IDisposable storage;
    int master,savePolicy;
    bool hadMaster,hadSavePolicy;
    [OneTimeSetUp] public void EnableExistingDiagnosticFixtures()
    {
        storage=DC1Preferences.TestHarness_UseDirectory(System.IO.Path.GetFullPath(System.IO.Path.Combine(
            UnityEngine.Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",System.Guid.NewGuid().ToString("N"))));
        hadMaster=DC1Preferences.HasKey(DC1DebugSettings.EnabledKey);
        hadSavePolicy=DC1Preferences.HasKey(DC1DebugSettings.AffectsSavesKey);
        master=DC1Preferences.GetInt(DC1DebugSettings.EnabledKey);
        savePolicy=DC1Preferences.GetInt(DC1DebugSettings.AffectsSavesKey);
        DC1DebugSettings.Enabled=true;
        DC1DebugSettings.AffectsSaves=false;
    }
    [OneTimeTearDown] public void RestoreDiagnosticPolicy()
    {
        if(hadMaster)DC1Preferences.SetInt(DC1DebugSettings.EnabledKey,master);else DC1Preferences.DeleteKey(DC1DebugSettings.EnabledKey);
        if(hadSavePolicy)DC1Preferences.SetInt(DC1DebugSettings.AffectsSavesKey,savePolicy);else DC1Preferences.DeleteKey(DC1DebugSettings.AffectsSavesKey);
        DC1Preferences.Save();
        storage.Dispose();
    }
}
#endif
