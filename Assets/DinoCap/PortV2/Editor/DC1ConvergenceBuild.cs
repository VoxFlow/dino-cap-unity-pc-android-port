#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace DinoCap.PortV2.EditorTools
{
    // Validation/export tool only. Does not rebuild scenes.
    public static class DC1ConvergenceBuild
    {
        public static void BuildWindowsTestPlayer()
        {
            ApplyIdentity(false);
            BuildWindowsPlayer("Build");
        }

        // Highest-resolution original icon: the IPA root "iTunesArtwork" (600x600 JPEG);
        // the best icon inside the .app bundle is only 152x152. A byte-identical copy of
        // that entry lives at the path below (original IPA untouched). PLATFORM_ADAPTER:
        // presentation identity of the shipped player, no gameplay semantics.
        const string AppIconAssetPath = "Assets/DinoCap/AppIcon/DC1_AppIcon_iTunesArtwork_600.jpg";

        static void ApplyAppIcon(bool android)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(AppIconAssetPath);
            if (importer == null)
                throw new InvalidOperationException("App icon asset missing: " + AppIconAssetPath);
            if (importer.textureType != TextureImporterType.Default || importer.mipmapEnabled ||
                importer.npotScale != TextureImporterNPOTScale.None || !importer.isReadable ||
                importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconAssetPath);
            if (icon == null)
                throw new InvalidOperationException("App icon failed to import: " + AppIconAssetPath);

            // Default icon: used by the Windows standalone executable.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

            if (!android) return;
            foreach (var kind in new PlatformIconKind[] { UnityEditor.Android.AndroidPlatformIconKind.Legacy,
                                                          UnityEditor.Android.AndroidPlatformIconKind.Round })
            {
                var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                foreach (var platformIcon in icons)
                    platformIcon.SetTexture(icon, 0);
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            }
        }

        // User-authorized build policy (2026-10-13): Windows x64 Mono (left
        // at the project's own default scripting backend, unchanged here);
        // Android IL2CPP, ARMv7 + ARM64, one universal APK (no AAB, no
        // per-architecture splitting). Explicitly sets project settings
        // (unlike the Windows path above, which builds from whatever is
        // already configured) because the live project's own
        // applicationIdentifier/scripting backend for Android were empty
        // per the prior read-only preflight - this is the one authorized
        // place those get set. Per AGENTS.md, the identifier is a neutral
        // project-derived string, not personal/company-derived.
        const string AndroidApplicationIdentifier = "com.dc1port.convergence";

        // Shipped product identity (user-specified 2026-10-13): app label "Dino Cap";
        // Windows executable "Dino Cap.exe"; Android package "Dino Cap.apk".
        const string ShippedProductName = "Dino Cap";

        static void ApplyIdentity(bool android)
        {
            PlayerSettings.productName = ShippedProductName;
            // User decision 2026-10-13: no "Made with Unity" boot splash in any build (PLATFORM_ADAPTER; the
            // original game's own boot logo scene is unaffected).
            PlayerSettings.resizableWindow = true;     // PC QoL: resizable window (size/position remembered at runtime)
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            // Colour Unity draws while it loads the first scene (before any game code runs). Black matches the boot logo's own background, so
            // launch goes black -> logo instead of grey -> black -> logo.
            PlayerSettings.SplashScreen.backgroundColor = UnityEngine.Color.black;
            ApplyAppIcon(android);
        }

        // Dev/test builds keep their historical file names and output folders, which
        // existing capture/test scripts reference. Shipped builds are non-development
        // (the DEVELOPMENT_BUILD-only diagnostics, capture hooks and smoke harness are
        // not compiled in) and use the shipped names under Operator/DC1-MASTER/Release.
        public static void BuildShippedWindows()
        {
            ApplyIdentity(false);
            BuildWindowsPlayer("Release/Windows", ShippedProductName + ".exe", BuildOptions.None);
        }

        public static void BuildShippedAndroid()
        {
            BuildAndroidPlayer("Release/Android", ShippedProductName + ".apk", BuildOptions.None);
        }

        public static void BuildAndroidTestPlayer()
        {
            BuildAndroidPlayer("BuildAndroid", "DC1-CG-Convergence.apk", BuildOptions.Development);
        }

        static void BuildAndroidPlayer(string folder, string fileName, BuildOptions options)
        {
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Operator/DC1-MASTER/" + folder));
            Directory.CreateDirectory(directory);

            // GPU/CPU frame timing is for the dev smoke harness's perf mode only.
            PlayerSettings.enableFrameTimingStats = (options & BuildOptions.Development) != 0;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
            ApplyIdentity(true);
            // Forced landscape on Android (the phone launched portrait under the
            // previous auto-rotate-all setting). AutoRotation with only the two
            // landscape orientations allowed maps to Android "sensorLandscape".
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            // Unity reports a placeholder default (com.DefaultCompany.*) when the
            // saved mapping is empty, so "only if empty" never fires; set explicitly.
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidApplicationIdentifier);

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(directory, fileName),
                target = BuildTarget.Android,
                options = options
            });
            File.WriteAllText(Path.Combine(directory, "build_result.json"),
                "{\"result\":\"" + report.summary.result + "\",\"errors\":" +
                report.summary.totalErrors + ",\"warnings\":" + report.summary.totalWarnings +
                ",\"bytes\":" + report.summary.totalSize +
                ",\"scriptingBackend\":\"" + PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) + "\"" +
                ",\"targetArchitectures\":\"" + PlayerSettings.Android.targetArchitectures + "\"" +
                ",\"buildAppBundle\":" + EditorUserBuildSettings.buildAppBundle.ToString().ToLowerInvariant() +
                ",\"applicationIdentifier\":\"" + PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) + "\"}");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Convergence Android player build failed: " + report.summary.result);
            Debug.Log("[DC1-MASTER] Android test player built: " + report.summary.outputPath);
        }

        // Separate renderer diagnostic; restore production settings even
        // when the build fails. The PowerShell runner also checks/restores
        // the exact serialized settings file after this editor exits.
        public static void BuildWindowsOpenGLDiagnosticPlayer()
        {
            const BuildTarget target=BuildTarget.StandaloneWindows64;
            bool automatic=PlayerSettings.GetUseDefaultGraphicsAPIs(target);
            var apis=PlayerSettings.GetGraphicsAPIs(target);
            string settings=Path.GetFullPath(Path.Combine(Application.dataPath,"../ProjectSettings/ProjectSettings.asset"));
            byte[] original=File.ReadAllBytes(settings);
            try {
                PlayerSettings.SetUseDefaultGraphicsAPIs(target,false);
                PlayerSettings.SetGraphicsAPIs(target,new[]{GraphicsDeviceType.OpenGLCore});
                BuildWindowsPlayer("BuildOpenGLDiagnostic");
            } finally {
                PlayerSettings.SetGraphicsAPIs(target,apis);
                PlayerSettings.SetUseDefaultGraphicsAPIs(target,automatic);
                File.WriteAllBytes(settings,original);
            }
        }

        static void BuildWindowsPlayer(string folder)
        {
            BuildWindowsPlayer(folder,"DC1-CG-Convergence.exe",BuildOptions.Development);
        }

        static void BuildWindowsPlayer(string folder,string fileName,BuildOptions options)
        {
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../Dev/Operator/DC1-MASTER/"+folder));
            Directory.CreateDirectory(directory);
            PlayerSettings.enableFrameTimingStats=(options&BuildOptions.Development)!=0;
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=Path.Combine(directory,fileName),
                target=BuildTarget.StandaloneWindows64,
                options=options
            });
            File.WriteAllText(Path.Combine(directory,"build_result.json"),
                "{\"result\":\""+report.summary.result+"\",\"errors\":"+
                report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+
                ",\"bytes\":"+report.summary.totalSize+"}");
            if(report.summary.result!=BuildResult.Succeeded)
                throw new InvalidOperationException("Convergence player build failed: "+report.summary.result);
            Debug.Log("[DC1-MASTER] Test player built: "+report.summary.outputPath);
        }
    }
}
#endif
