using System;
using System.IO;
using UnityEngine;

namespace CloverAPI.Utils;

public static class TextureUtils
{
	public static Texture2D LoadTextureFromFile(string filePath, FilterMode filterMode = FilterMode.Point, string onNotFound = "error", string onMultipleFound = "warn")
	{
		var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
		byte[] bytes = FileUtils.LoadFileBytes(filePath, onNotFound, onMultipleFound);
		if (!ImageConversion.LoadImage(tex, bytes))
		{
			throw new Exception("Failed to load texture from file '" + filePath + "'. The file may not be a valid image.");
		}
		tex.filterMode = filterMode;
		tex.name = Path.GetFileNameWithoutExtension(filePath);
		return tex;
	}

	public static Texture2D ResizeTexture(Texture2D original, int width, int height)
	{
		RenderTexture rt = RenderTexture.active = RenderTexture.GetTemporary(width, height);
		Graphics.Blit(original, rt);
		var resized = new Texture2D(width, height);
		resized.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		resized.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(rt);
		return resized;
	}

	public static Texture2D CopyTexture(Texture2D original)
	{
        var copy = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1);
		if (original.isReadable)
		{
			copy.SetPixels(original.GetPixels());
			copy.Apply();
        }
        else
        {
            RenderTexture rt = (RenderTexture.active = RenderTexture.GetTemporary(original.width, original.height));
            Graphics.Blit(original, rt);
            copy.ReadPixels(new Rect(0f, 0f, original.width, original.height), 0, 0);
            copy.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return copy;
        }
        return copy;
    }
}
