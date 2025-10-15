# TextureOverrides

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for overriding in-game textures with custom image files.
</link-summary>

**Code Reference**  
`namespace: CloverAPI.Content.Textures`
`class: TextureManager`

You can override in-game textures with your own custom image files using the `TextureManager` class.

## Supported Formats
CloverAPI supports all Unity-supported image formats: `PNG`, `JP(E)G`, `BMP`, `TGA`, and `GIF` (static images only).
`PNG` is recommended for best compatibility.

## Registering Texture Overrides
To register a texture override, use the `TextureManager.RegisterTextureOverride(string name, Texture2D texture, ModGuid guid)` method.  
The `name` parameter is the name of the texture to override. The easiest way to find the name is to look at the texture's or sprite's filename in the game's files using a tool like [AssetStudio](https://github.com/aelurum/AssetStudio).
The `texture` parameter is the `Texture2D` instance to use as the override. You can use the `TextureUtils.LoadTextureFromFile(string filePath, FilterMode filterMode = FilterMode.Point)` method to load a texture from a file. The `filterMode` parameter determines how the texture is filtered when scaled. `Point` is recommended for pixel art, while `Bilinear` or `Trilinear` may be better for high-resolution textures in some cases.  
The `guid` parameter is the `ModGuid` of your mod. Pass either your plugin instance or a string. It'll be converted automatically.

```C#
using CloverAPI;
using CloverAPI.Content.Textures;
using CloverAPI.Utils;

public class ExampleMod : BaseUnityPlugin
{
    internal const string MainContentFolder = "ExampleMod_Content";
    internal static string PluginPath;
    public static string ImagePath { get; private set; }

    private void Awake()
    {
        PluginPath = Path.GetDirectoryName(this.Info.Location);
        ImagePath = Path.Combine(PluginPath, MainContentFolder, "Images");
        RegisterTextureOverrides();
    }
    
    private void RegisterTextureOverrides()
    {
        var texturePath = Path.Combine(ImagePath, "example_texture.png");
        var texture = TextureUtils.LoadTextureFromFile(texturePath);
        if (texture != null)
        {
            TextureManager.RegisterTextureOverride("example_texture", texture, this);
        }
    }
}
```