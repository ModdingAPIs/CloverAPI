# 🎵 Audio Overrides

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for overriding in-game audio clips with custom audio files.
</link-summary>

**Code Reference**  
`namespace: CloverAPI.Content.Audio`  
`class: AudioManager`

You can override in-game audio clips with your own custom audio files using the `AudioManager` class.  

## Supported Formats
CloverAPI supports all Unity-supported audio formats: `OGG`, `WAV`, `MP3`, `AIF(F)`, `MOD`, `XM`, `IT`, and `S3M`.  
`WAV` and `OGG` are recommended for best compatibility.

## Registering Audio Overrides
To register an audio override, use the `AudioManager.RegisterSoundOverride(string name, AudioClip clip, ModGuid guid)` for sound effects or its `RegisterMusicOverride` counterpart for music.  
The `name` parameter is the name of the audio clip to override. The easiest way to find the name is to look at the audio clip's filename in the game's files using a tool like [AssetStudio](https://github.com/aelurum/AssetStudio).  
The `clip` parameter is the `AudioClip` instance to use as the override. You can use the `AudioUtils.LoadAudioFromFile(string filePath)` method to load an audio clip from a file.  
The `guid` parameter is the `ModGuid` of your mod. Pass either your plugin instance or a string. It'll be converted automatically.

```C#
using CloverAPI;
using CloverAPI.Content.Audio;
using CloverAPI.Utils;

public class ExampleMod : BaseUnityPlugin
{
    internal const string MainContentFolder = "ExampleMod_Content";
    internal static string PluginPath;
    public static string DataPath { get; private set; }

    private void Awake()
    {
        PluginPath = Path.GetDirectoryName(this.Info.Location);
        DataPath = Path.Combine(PluginPath, MainContentFolder, "Data");
        RegisterAudioOverrides();
    }
    
    private void RegisterAudioOverrides()
    {
        var soundPath = Path.Combine(DataPath, "example_sound.ogg");
        var soundClip = AudioUtils.LoadAudioFromFile(soundPath);
        if (soundClip != null)
        {
            AudioManager.RegisterSoundOverride("example_sound", soundClip, this);
        }
    }
}
```