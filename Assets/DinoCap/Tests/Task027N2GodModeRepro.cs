#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    /// <summary>
    /// TASK027N.2 Parts T-W live PlayMode verification: God Mode blocks HP
    /// loss/death at the authoritative owner (DamagePlayer) while preserving
    /// the normal hurt-reaction/knockback/invulnerability side effects, and
    /// cleanly restores normal damage the instant it is disabled. Drives the
    /// REAL CombatSandboxV2 through its own Update()/dino-AI bite pipeline -
    /// no gameplay code path is bypassed or reimplemented here.
    /// </summary>
    public class Task027N2GodModeRepro
    {
        const string ScenePath = "Assets/DinoCap/Scenes/CombatSandboxV2.unity";
        static readonly string OutDir =
            System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../Dev/Reports/Task027N_2_BloodAudioBiteEndDebugGodMode/raw"));

        CombatSandboxV2 sandbox;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Directory.CreateDirectory(OutDir);
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            yield return null;
            sandbox = Object.FindFirstObjectByType<CombatSandboxV2>();
            Assert.IsNotNull(sandbox, "CombatSandboxV2 not found after loading " + ScenePath);
            sandbox.TestHarness_SetGodMode(false);
            sandbox.TestHarness_SetShowHPOverlay(true);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (sandbox != null)
                sandbox.TestHarness_SetGodMode(false);
            yield return null;
        }

        class Csv
        {
            readonly List<string> header;
            readonly List<string[]> rows = new List<string[]>();
            public Csv(params string[] cols) { header = cols.ToList(); }
            public void Row(params object[] vals)
            {
                var s = new string[vals.Length];
                for (int i = 0; i < vals.Length; i++)
                {
                    var v = vals[i];
                    s[i] = v switch
                    {
                        null => "",
                        float f => f.ToString("0.####", CultureInfo.InvariantCulture),
                        double d => d.ToString("0.####", CultureInfo.InvariantCulture),
                        bool b => b ? "1" : "0",
                        _ => v.ToString().Replace(",", ";")
                    };
                }
                rows.Add(s);
            }
            public void Save(string path)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", header));
                foreach (var r in rows) sb.AppendLine(string.Join(",", r));
                File.WriteAllText(path, sb.ToString());
            }
        }

        static readonly string[] Cols = {
            "tag","time","godMode","playerHP","playerMaxHP","playerDead",
            "playerHitStun","invulnUntil","playerX","enemyX"
        };

        void Snap(Csv csv,string tag)
        {
            var s=sandbox.CaptureRuntimeDebugSnapshot();
            csv.Row(tag,Time.time,sandbox.TestHarness_GodMode,
                sandbox.TestHarness_PlayerHP,sandbox.TestHarness_PlayerMaxHP,
                sandbox.TestHarness_PlayerDead,s.playerHitStun,s.invulnUntil,
                s.playerX,s.enemyX);
        }

        // ---------------------------------------------------------------
        // Part W: God Mode ON - repeated bites must not reduce HP or kill
        // the player, but hurt-reaction (hitstun) and the invulnerability
        // window must still occur exactly as normal.
        // ---------------------------------------------------------------
        [UnityTest]
        public IEnumerator GodMode_BlocksHPLoss_PreservesHitReaction()
        {
            var csv=new Csv(Cols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true); // Type1 K3Blue - fast, low HP, quick bite cadence
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(220f);
            sandbox.TestHarness_SetGodMode(true);
            Snap(csv,"start_godmode_on");

            float startHP=sandbox.TestHarness_PlayerHP;
            bool sawHitStun=false;
            bool sawInvulnAdvance=false;
            float initialInvuln=sandbox.CaptureRuntimeDebugSnapshot().invulnUntil;

            float start=Time.time;
            int i=0;
            while(Time.time-start<6f && i<60000)
            {
                yield return null; i++;
                var s=sandbox.CaptureRuntimeDebugSnapshot();
                if(s.playerHitStun) sawHitStun=true;
                if(s.invulnUntil>initialInvuln) sawInvulnAdvance=true;
                Snap(csv,"tick_"+i);

                // Force each hitstun window closed quickly by expiring
                // invuln early is NOT done here - we let it run its real
                // 1.5s course so multiple genuine bite/hurt cycles occur
                // within the 6s budget.
                Assert.IsFalse(sandbox.TestHarness_PlayerDead,
                    "Player died while God Mode was ON.");
                Assert.AreEqual(startHP,sandbox.TestHarness_PlayerHP,0.001f,
                    "Player HP changed while God Mode was ON (frame "+i+").");
            }

            csv.Save(Path.Combine(OutDir,"n2godmode_block_and_reactions.csv"));

            Assert.IsTrue(sawHitStun,
                "God Mode ON: no hurt-reaction (playerHitStun) was ever observed across 6s of repeated bites - hit reactions must be preserved, only HP loss/death blocked.");
            Assert.IsTrue(sawInvulnAdvance,
                "God Mode ON: invulnUntil never advanced - the invulnerability window must still be applied on a landed hit.");
        }

        // ---------------------------------------------------------------
        // Part W: HP pinned at 1 stays at 1 under repeated hits while ON.
        // ---------------------------------------------------------------
        [UnityTest]
        public IEnumerator GodMode_HPAtOne_StaysAtOne()
        {
            var csv=new Csv(Cols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(220f);
            sandbox.TestHarness_SetPlayerHP(1f);
            sandbox.TestHarness_SetGodMode(true);
            Snap(csv,"start_hp1");

            float start=Time.time; int i=0;
            while(Time.time-start<4f && i<40000)
            {
                yield return null; i++;
                Snap(csv,"tick_"+i);
                Assert.AreEqual(1f,sandbox.TestHarness_PlayerHP,0.001f,
                    "Player HP at 1 changed under God Mode (frame "+i+").");
                Assert.IsFalse(sandbox.TestHarness_PlayerDead,"Player died at HP=1 under God Mode.");
            }
            csv.Save(Path.Combine(OutDir,"n2godmode_hp1_pinned.csv"));
        }

        // ---------------------------------------------------------------
        // Part W: disabling God Mode restores normal damage immediately -
        // no permanent invulnerability flag leak.
        // ---------------------------------------------------------------
        [UnityTest]
        public IEnumerator GodMode_DisableRestoresNormalDamageImmediately()
        {
            var csv=new Csv(Cols);
            sandbox.TestHarness_ClearDinos();
            yield return null;
            sandbox.TestHarness_SpawnDino(0,true);
            yield return null;
            sandbox.TestHarness_SetPlayerX(0f);
            sandbox.TestHarness_SetEnemyX(220f);
            sandbox.TestHarness_SetGodMode(true);
            Snap(csv,"start_on");

            // Let one bite land under God Mode (HP unchanged), then force
            // the invuln window closed and disable God Mode.
            float startHP=sandbox.TestHarness_PlayerHP;
            bool bitten=false;
            float t0=Time.time; int i=0;
            while(Time.time-t0<6f && i<60000 && !bitten)
            {
                yield return null; i++;
                var s=sandbox.CaptureRuntimeDebugSnapshot();
                if(s.playerHitStun) bitten=true;
                Snap(csv,"pre_"+i);
            }
            Assert.IsTrue(bitten,"No bite landed under God Mode within budget.");
            Assert.AreEqual(startHP,sandbox.TestHarness_PlayerHP,0.001f);

            sandbox.TestHarness_ForceInvulnExpired();
            sandbox.TestHarness_SetGodMode(false);
            Snap(csv,"disabled");

            bool tookDamage=false;
            float t1=Time.time; int i2=0;
            while(Time.time-t1<6f && i2<60000 && !tookDamage)
            {
                yield return null; i2++;
                Snap(csv,"post_"+i2);
                if(sandbox.TestHarness_PlayerHP<startHP-0.001f) tookDamage=true;
            }
            csv.Save(Path.Combine(OutDir,"n2godmode_disable_restores_damage.csv"));

            Assert.IsTrue(tookDamage,
                "Player took no damage within 6s after God Mode was disabled and invulnerability was force-expired - normal damage must resume immediately (no permanent invulnerability flag leak).");
        }
    }
}
#endif
