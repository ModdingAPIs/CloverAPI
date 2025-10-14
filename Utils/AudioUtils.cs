using System.IO;
using UnityEngine.Networking;

namespace CloverAPI.Utils;

public static class AudioUtils
{
    public static AudioClip LoadAudioFromFile(string filePath, string onNotFound = "error", string onMultipleFound = "warn")
    {
        string foundPath = FileUtils.FindFile(filePath, onNotFound, onMultipleFound);
        if (!File.Exists(foundPath))
        {
            throw new FileNotFoundException("File '" + filePath + "' not found.");
        }
        var wr = UnityWebRequestMultimedia.GetAudioClip("file://" + foundPath, AudioType.UNKNOWN);
        wr.SendWebRequest();
        while (!wr.isDone) { } // Wait for the request to complete (should be minimal delay for local files)
        if (wr.result == UnityWebRequest.Result.Success)
        {
            var clip = DownloadHandlerAudioClip.GetContent(wr);
            wr.Dispose();
            return clip;
        }
        LogError("Failed to load audio clip from file '" + filePath + "': " + wr.error);
        wr.Dispose();
        return null;
    }
}