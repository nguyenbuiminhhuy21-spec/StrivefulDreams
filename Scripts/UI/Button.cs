using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code_Game.Scripts.UI;

public class Button
{
    private readonly SpriteFont _font;
    private readonly string _text;
    private readonly Texture2D _texture;
    private readonly Vector2 _position;
    private readonly Color _normalColor;
    private readonly Color _hoverColor;
    private readonly Color _pressedColor;

    private Rectangle _bounds;
    private bool _isHovered;
    private bool _isPressed;
    private static Texture2D _whitePixel;
    public event Action OnClick;

    private float _textScale = 1.0f;
    private string _wrappedText;

    public Button(SpriteFont font, string text, Vector2 position, Color? normalColor = null, Color? hoverColor = null, Color? pressedColor = null, int? width = null, int? height = null)
    {
        _font = font;
        _text = text;
        _position = position;
        _normalColor = normalColor ?? Color.Gray;
        _hoverColor = hoverColor ?? Color.LightGray;
        _pressedColor = pressedColor ?? Color.DarkGray;

        float manualSpaceWidth = 8;
        float availableWidth = (width ?? 200) - 20;
        _wrappedText = WrapTextWithManualSpaces(_font, _text, availableWidth, manualSpaceWidth);

        float textW = MeasureStringWithManualSpaces(_font, _wrappedText, 1.0f, manualSpaceWidth);
        float textH = _font.MeasureString("A").Y;

        int initialWidth = width ?? (int)textW + 40;
        int initialHeight = height ?? (int)textH + 20;
        _bounds = new Rectangle((int)_position.X, (int)_position.Y, initialWidth, initialHeight);

        availableWidth = _bounds.Width - 20;
        float availableHeight = _bounds.Height - 10;
        
        float scaleX = availableWidth / textW;
        float scaleY = availableHeight / textH;
        _textScale = Math.Clamp(Math.Min(scaleX, scaleY), 0.1f, 1.0f);
    }

    public Button(Texture2D texture, Vector2 position, int? width = null, int? height = null)
    {
        _texture = texture;
        _position = position;
        _normalColor = Color.White;
        _hoverColor = Color.LightGray;
        _pressedColor = Color.Gray;

        int initialWidth = width ?? _texture.Width;
        int initialHeight = height ?? _texture.Height;
        _bounds = new Rectangle((int)_position.X, (int)_position.Y, initialWidth, initialHeight);
    }

    private string WrapTextWithManualSpaces(SpriteFont font, string text, float maxWidth, float spaceWidth)
    {
        if (string.IsNullOrEmpty(text)) return "";
        StringBuilder sb = new StringBuilder();
        string[] words = text.Split(' ');
        float currentLineLength = 0;
        foreach (string word in words)
        {
            Vector2 wordSize = font.MeasureString(word);
            if (currentLineLength + wordSize.X > maxWidth && currentLineLength > 0)
            {
                sb.Append("\n");
                currentLineLength = 0;
            }
            sb.Append(word + " ");
            currentLineLength += wordSize.X + spaceWidth;
        }
        return sb.ToString().TrimEnd();
    }

    private float MeasureStringWithManualSpaces(SpriteFont font, string text, float scale, float spaceWidth)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        float totalWidth = 0;
        string[] words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            totalWidth += font.MeasureString(words[i]).X * scale;
            if (i < words.Length - 1) totalWidth += spaceWidth * scale;
        }
        return totalWidth;
    }

    private void DrawStringWithManualSpaces(SpriteBatch sb, string text, Vector2 position, Color color, float scale, float spaceWidth)
    {
        if (string.IsNullOrEmpty(text)) return;
        Vector2 currentPos = position;
        string[] words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            sb.DrawString(_font, words[i], currentPos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            currentPos.X += _font.MeasureString(words[i]).X * scale + spaceWidth * scale;
        }
    }

    public Rectangle Bounds => _bounds;

    public void UpdateLayout(Vector2 position, int width, int height)
    {
        _bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
        if (_font != null && !string.IsNullOrEmpty(_text))
        {
            float manualSpaceWidth = 8;
            float availableWidth = width - 20;
            _wrappedText = WrapTextWithManualSpaces(_font, _text, availableWidth, manualSpaceWidth);
            float textW = MeasureStringWithManualSpaces(_font, _wrappedText, 1.0f, manualSpaceWidth);
            float textH = _font.MeasureString("A").Y;
            float scaleX = availableWidth / textW;
            float scaleY = (height - 10) / textH;
            _textScale = Math.Clamp(Math.Min(scaleX, scaleY), 0.1f, 1.0f);
        }
    }

    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        Update(gameTime, new Point(mouseState.X, mouseState.Y));
    }

    public void Update(GameTime gameTime, Point mousePoint)
    {
        _isHovered = _bounds.Contains(mousePoint);
        var mouseState = Mouse.GetState();
        if (_isHovered && mouseState.LeftButton == ButtonState.Pressed) _isPressed = true;
        else if (_isPressed && mouseState.LeftButton == ButtonState.Released && _isHovered) { OnClick?.Invoke(); _isPressed = false; }
        else _isPressed = false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = _isPressed ? _pressedColor : (_isHovered ? _hoverColor : _normalColor);
        if (_texture != null) spriteBatch.Draw(_texture, _bounds, color);
        else
        {
            if (_whitePixel == null) { _whitePixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1); _whitePixel.SetData(new[] { Color.White }); }
            spriteBatch.Draw(_whitePixel, _bounds, color);
            if (_font != null && !string.IsNullOrEmpty(_wrappedText))
            {
                float manualSpaceWidth = 8;
                float textW = MeasureStringWithManualSpaces(_font, _wrappedText, _textScale, manualSpaceWidth);
                float textH = _font.MeasureString("A").Y * _textScale;
                Vector2 textPos = new Vector2(_bounds.X + (_bounds.Width - textW) / 2, _bounds.Y + (_bounds.Height - textH) / 2);
                DrawStringWithManualSpaces(spriteBatch, _wrappedText, textPos, Color.Black, _textScale, manualSpaceWidth);
            }
        }
    }
}