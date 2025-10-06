using UnityEngine;

namespace CloverAPI.Classes;

public class TextureSourceFromTexture : TextureSource
{
	private Texture2D _texture;

	public TextureSourceFromTexture(Texture2D texture)
	{
		_texture = texture;
	}

	public override Texture2D GetTexture()
	{
		return _texture;
	}

	public static implicit operator TextureSourceFromTexture(Texture2D texture) => new(texture);
	public static implicit operator Texture2D(TextureSourceFromTexture source) => source._texture;
}
