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
    private readonly Vector2 _position;
    private readonly Color _normalColor;
    private readonly Color _hoverColor;
    private readonly Color _pressedColor;

    private Rectangle _bounds;
    private bool _isHovered;
    private bool _isPressed;
    private static Texture2D _whitePixel;
    public event Action OnClick;

    private float _textScale = 0.1f;
    private string _wrappedText;

    public Button(SpriteFont font, string text, Vector2 position, Color? normalColor = null, Color? hoverColor = null, Color? pressedColor = null, int? width = null, int? height = null)
    {
        _font = font;
        _text = text;
        _position = position;
        _normalColor = normalColor ?? Color.Gray;
        _hoverColor = hoverColor ?? Color.LightGray;
        _pressedColor = pressedColor ?? Color.DarkGray;

        // 1. Determine base bounds
        int initialWidth = width ?? (int)_font.MeasureString(_text).X + 20;
        int initialHeight = height ?? (int)_font.MeasureString(_text).Y + 10;
        _bounds = new Rectangle((int)_position.X, (int)_position.Y, initialWidth, initialHeight);

        // 2. First pass: Try to wrap the text based on button width
        float availableWidth = _bounds.Width - 20;
        float availableHeight = _bounds.Height - 10;
        _wrappedText = WrapText(_font, _text, availableWidth);

        // 3. Second pass: Calculate scale if the wrapped text still overflows height-wise
        Vector2 wrappedSize = _font.MeasureString(_wrappedText);

        float scaleX = availableWidth / wrappedSize.X;
        float scaleY = availableHeight / wrappedSize.Y;

        // Use the smaller scale to fit. 
        // Using Clamp to keep text size between 0.1 and 1.0
        float calculatedScale = Math.Min(scaleX, scaleY);
        _textScale = Math.Clamp(calculatedScale, 0.1f, 1.0f);
    }

    private string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return "";

        StringBuilder sb = new StringBuilder();
        string[] words = text.Split(' ');
        float spaceWidth = font.MeasureString(" ").X;
        float currentLineLength = 0;

        foreach (string word in words)
        {
            Vector2 wordSize = font.MeasureString(word);

            // If a single word is longer than the maxWidth, we keep it but it will be scaled later
            if (currentLineLength + wordSize.X > maxWidth)
            {
                sb.Append("\n");
                currentLineLength = 0;
            }

            sb.Append(word + " ");
            currentLineLength += wordSize.X + spaceWidth;
        }

        return sb.ToString().TrimEnd();
    }

    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        _isHovered = _bounds.Contains(mousePoint);

        if (_isHovered && mouseState.LeftButton == ButtonState.Pressed)
        {
            _isPressed = true;
        }
        else if (_isPressed && mouseState.LeftButton == ButtonState.Released && _isHovered)
        {
            OnClick?.Invoke();
            _isPressed = false;
        }
        else
        {
            _isPressed = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = _normalColor;
        if (_isPressed)
        {
            color = _pressedColor;
        }
        else if (_isHovered)
        {
            color = _hoverColor;
        }

        // Draw button background
        if (_whitePixel == null)
        {
            _whitePixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }
        spriteBatch.Draw(_whitePixel, _bounds, color);

        // Draw wrapped and potentially scaled text
        Vector2 wrappedSize = _font.MeasureString(_wrappedText) * _textScale;
        Vector2 textPosition = new Vector2(
            _bounds.X + (_bounds.Width - wrappedSize.X) / 2,
            _bounds.Y + (_bounds.Height - wrappedSize.Y) / 2
        );

        spriteBatch.DrawString(_font, _wrappedText, textPosition, Color.Black, 0, Vector2.Zero, _textScale, SpriteEffects.None, 0);
    }

}