using CloverAPI.Utils;

namespace CloverAPI.Classes;

public class TextureSourceFromFile : TextureSource
{
    private readonly string _filePath;

    private Texture2D _texture;

    public TextureSourceFromFile(string filePath)
    {
        this._filePath = filePath;
        LoadTexture();
    }

    private void LoadTexture()
    {
        this._texture = TextureUtils.LoadTextureFromFile(this._filePath);
    }

    public override Texture2D GetTexture()
    {
        return this._texture;
    }

    public static implicit operator TextureSourceFromFile(string filePath)
    {
        return new TextureSourceFromFile(filePath);
    }

    public static implicit operator string(TextureSourceFromFile source)
    {
        return source._filePath;
    }
}