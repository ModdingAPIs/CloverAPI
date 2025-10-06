using UnityEngine;

namespace CloverAPI.Classes;

public abstract class TextureSource
{
	public abstract Texture2D GetTexture();

    public static implicit operator TextureSource(string filePath) => new TextureSourceFromFile(filePath);
	public static implicit operator TextureSource(Texture2D texture) => new TextureSourceFromTexture(texture);
	public static implicit operator Texture2D(TextureSource source) => source.GetTexture();
}
