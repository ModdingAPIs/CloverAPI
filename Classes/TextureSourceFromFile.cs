using CloverAPI.Utils;
using UnityEngine;

namespace CloverAPI.Classes;

public class TextureSourceFromFile : TextureSource
{
	private string _filePath;

	private Texture2D _texture;

	public TextureSourceFromFile(string filePath)
	{
		_filePath = filePath;
		LoadTexture();
	}

	private void LoadTexture()
	{
		_texture = TextureUtils.LoadTextureFromFile(_filePath);
	}

	public override Texture2D GetTexture()
	{
		return _texture;
	}

	public static implicit operator TextureSourceFromFile(string filePath) => new TextureSourceFromFile(filePath);
	public static implicit operator string(TextureSourceFromFile source) => source._filePath;
}
