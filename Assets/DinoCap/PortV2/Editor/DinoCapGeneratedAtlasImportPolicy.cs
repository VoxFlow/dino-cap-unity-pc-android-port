using UnityEditor;
using UnityEngine;

namespace DinoCap.PortV2.Editor
{
    /// <summary>
    /// Task025 source-truth import policy for generated Power2D atlas pages.
    ///
    /// Native atlas state is page-specific: dino1Image is GL_LINEAR,
    /// while dino2Image and the existing other pages use GL_NEAREST.
    ///   no mipmaps
    ///   uncompressed RGBA8888
    ///   straight alpha
    ///
    /// Preserve original transparent RGB on the linear page; Unity's alpha
    /// dilation changes the colors that native linear interpolation samples.
    /// </summary>
    public sealed class DinoCapGeneratedAtlasImportPolicy : AssetPostprocessor
    {
        const string GeneratedTextureRoot =
            "Assets/DinoCap/Generated/Resources/DinoCapGenerated/Textures/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(GeneratedTextureRoot,
                    System.StringComparison.OrdinalIgnoreCase))
                return;

            TextureImporter ti = (TextureImporter)assetImporter;
            bool nativeLinearDinoPage=string.Equals(assetPath,
                GeneratedTextureRoot+"ipad_dino1.png",System.StringComparison.OrdinalIgnoreCase);

            ti.textureType = TextureImporterType.Default;
            ti.mipmapEnabled = false;
            ti.sRGBTexture = true;
            ti.alphaIsTransparency = !nativeLinearDinoPage;
            ti.isReadable = false;

            ti.filterMode = nativeLinearDinoPage?FilterMode.Bilinear:FilterMode.Point;
            ti.wrapMode = nativeLinearDinoPage?TextureWrapMode.Repeat:TextureWrapMode.Clamp;

            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 2048;

            // Do not allow stale per-platform overrides to restore BC/DXT/ETC
            // compression behind the global source-truth setting.
            ClearOverride(ti, "Standalone");
            ClearOverride(ti, "Android");
        }

        static void ClearOverride(TextureImporter ti, string platform)
        {
            TextureImporterPlatformSettings s =
                ti.GetPlatformTextureSettings(platform);

            s.overridden = false;
            ti.SetPlatformTextureSettings(s);
        }
    }
}
