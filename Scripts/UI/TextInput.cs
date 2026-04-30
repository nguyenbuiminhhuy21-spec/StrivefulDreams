using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code_Game.Scripts.UI;

public class TextInput
{
    private readonly SpriteFont _font;
    private readonly Vector2 _position;
    private readonly int _width;
    private readonly Color _backgroundColor;
    private readonly Color _borderColor;
    private readonly Color _textColor;
    private readonly string _label;

    private string _text;
    private bool _isFocused;
    private KeyboardState _previousKeyboardState;

    public string Text
    {
        get => _text;
        set => _text = value ?? string.Empty;
    }

    public Rectangle Bounds { get; private set; }
    public bool IsFocused => _isFocused;

    public TextInput(SpriteFont font, string label, Vector2 position, int width = 260, string initialText = "")
    {
        _font = font;
        _label = label;
        _position = position;
        _width = width;
        _text = initialText;
        _backgroundColor = new Color(245, 245, 245);
        _borderColor = Color.Black;
        _textColor = Color.Black;
        var textSize = _font.MeasureString(_label);
        Bounds = new Rectangle((int)_position.X, (int)_position.Y + (int)textSize.Y + 8, _width, (int)_font.MeasureString("A").Y + 12);
    }

    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        if (mouseState.LeftButton == ButtonState.Pressed && Bounds.Contains(mousePoint))
        {
            _isFocused = true;
        }
        else if (mouseState.LeftButton == ButtonState.Pressed && !Bounds.Contains(mousePoint))
        {
            _isFocused = false;
        }

        var keyboardState = Keyboard.GetState();
        if (_isFocused)
        {
            foreach (Keys key in keyboardState.GetPressedKeys())
            {
                if (_previousKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Back && _text.Length > 0)
                    {
                        _text = _text.Substring(0, _text.Length - 1);
                    }
                    else if (key == Keys.Space)
                    {
                        _text += " ";
                    }
                    else
                    {
                        var character = MapKeyToChar(key, keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift));
                        if (!string.IsNullOrEmpty(character))
                            _text += character;
                    }
                }
            }
        }

        _previousKeyboardState = keyboardState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CreateWhitePixel(spriteBatch.GraphicsDevice), Bounds, _backgroundColor);
        spriteBatch.DrawRectangle(Bounds, _borderColor, 2);

        var labelSize = _font.MeasureString(_label);
        spriteBatch.DrawString(_font, _label, new Vector2(_position.X, _position.Y), Color.White);

        var textToRender = string.IsNullOrEmpty(_text) ? "..." : _text;
        var textPosition = new Vector2(Bounds.X + 8, Bounds.Y + 6);
        spriteBatch.DrawString(_font, textToRender, textPosition, _textColor);
    }

    private Texture2D CreateWhitePixel(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        return texture;
    }

    private static string MapKeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            var letter = (char)('a' + (key - Keys.A));
            return shift ? letter.ToString().ToUpperInvariant() : letter.ToString();
        }

        if (key >= Keys.D0 && key <= Keys.D9)
        {
            var number = (char)('0' + (key - Keys.D0));
            return number.ToString();
        }

        if (key == Keys.OemPeriod) return ".";
        if (key == Keys.OemMinus) return "-";
        if (key == Keys.OemComma) return ",";
        if (key == Keys.OemQuestion) return "?";
        if (key == Keys.OemSemicolon) return ";";
        if (key == Keys.OemQuotes) return "'";
        if (key == Keys.OemOpenBrackets) return "[";
        if (key == Keys.OemCloseBrackets) return "]";
        if (key == Keys.OemPlus) return shift ? "+" : "=";
        if (key == Keys.OemPipe) return "|";
        if (key == Keys.OemTilde) return shift ? "~" : "`";

        return null;
    }
}

public static class SpriteBatchExtensions
{
    public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
    {
        var pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { color });
        spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
    }
}
