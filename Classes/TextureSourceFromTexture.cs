namespace CloverAPI.Classes;

public class TextureSourceFromTexture : TextureSource
{
    private readonly Texture2D _texture;

    public TextureSourceFromTexture(Texture2D texture)
    {
        this._texture = texture;
    }

    public override Texture2D GetTexture()
    {
        return this._texture;
    }

    public static implicit operator TextureSourceFromTexture(Texture2D texture)
    {
        return new TextureSourceFromTexture(texture);
    }

    public static implicit operator Texture2D(TextureSourceFromTexture source)
    {
        return source._texture;
    }
}