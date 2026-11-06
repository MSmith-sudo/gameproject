using UnityEditor;
using UnityEngine;
namespace AstralPuzzles.Editor {
public sealed class GardenTextureImport : AssetPostprocessor {
    void OnPreprocessTexture() {
        if(assetPath!="Assets/Resources/GardenBackdrop.png")return;
        var texture=(TextureImporter)assetImporter;
        texture.filterMode=FilterMode.Point;
        texture.mipmapEnabled=false;
        texture.npotScale=TextureImporterNPOTScale.None;
        texture.textureCompression=TextureImporterCompression.Uncompressed;
    }
}
}
