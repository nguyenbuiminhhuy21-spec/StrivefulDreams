using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code_Game.Scripts.UI;

public class TextInput
{
    private readonly SpriteFont _font;
    private Vector2 _position;
    private int _width;
    private int _height;
    private readonly Color _textColor;
    private readonly string _label;
    private readonly Texture2D _backgroundTexture;
    private int _labelOffsetY = -35;
    private float _fontScale = 1.0f;

    private string _text;
    private bool _isFocused;
    private KeyboardState _previousKeyboardState;

    // Key Repeat logic
    private double _backspaceTimer;
    private const double InitialRepeatDelay = 500; // ms
    private const double RepeatInterval = 50; // ms
    private bool _isInitialDelayPassed;

    public string Text
    {
        get => _text;
        set => _text = value ?? string.Empty;
    }

    public Rectangle Bounds { get; private set; }
    public bool IsFocused => _isFocused;
    public int LabelOffsetY { get => _labelOffsetY; set => _labelOffsetY = value; }
    public float FontScale { get => _fontScale; set => _fontScale = value; }

    public TextInput(SpriteFont font, string label, Vector2 position, int width, int height, Texture2D backgroundTexture = null, string placeholder = "", string initialText = "")
    {
        _font = font;
        _label = label;
        _position = position;
        _width = width;
        _height = height;
        _text = initialText;
        _backgroundTexture = backgroundTexture;
        _textColor = Color.Black;

        UpdateBounds();
    }

    public void UpdateLayout(Vector2 position, int width, int height)
    {
        _position = position;
        _width = width;
        _height = height;
        UpdateBounds();
    }

    private void UpdateBounds()
    {
        Bounds = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
    }

    public void Update(GameTime gameTime)
    {
        var ms = Mouse.GetState();
        Update(gameTime, new Point(ms.X, ms.Y));
    }

    public void Update(GameTime gameTime, Point mousePoint)
    {
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            _isFocused = Bounds.Contains(mousePoint);
        }

        var keyboardState = Keyboard.GetState();
        if (_isFocused)
        {
            // Special handling for Backspace repeat
            if (keyboardState.IsKeyDown(Keys.Back))
            {
                if (_previousKeyboardState.IsKeyUp(Keys.Back))
                {
                    DeleteLastChar();
                    _backspaceTimer = gameTime.TotalGameTime.TotalMilliseconds;
                    _isInitialDelayPassed = false;
                }
                else
                {
                    double elapsed = gameTime.TotalGameTime.TotalMilliseconds - _backspaceTimer;
                    if (!_isInitialDelayPassed)
                    {
                        if (elapsed >= InitialRepeatDelay)
                        {
                            DeleteLastChar();
                            _backspaceTimer = gameTime.TotalGameTime.TotalMilliseconds;
                            _isInitialDelayPassed = true;
                        }
                    }
                    else
                    {
                        if (elapsed >= RepeatInterval)
                        {
                            DeleteLastChar();
                            _backspaceTimer = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
            }

            foreach (Keys key in keyboardState.GetPressedKeys())
            {
                if (_previousKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Back) { /* Handled above */ }
                    else if (key == Keys.Space) _text += " ";
                    else if (key == Keys.Enter) _isFocused = false;
                    else
                    {
                        var character = MapKeyToChar(key, keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift));
                        if (!string.IsNullOrEmpty(character)) _text += character;
                    }
                }
            }
        }

        _previousKeyboardState = keyboardState;
    }

    private void DeleteLastChar()
    {
        if (_text.Length > 0)
        {
            _text = _text.Substring(0, _text.Length - 1);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Matrix? transformMatrix = null)
    {
        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, Bounds, Color.White);
        }
        else
        {
            var pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, Bounds, Color.DarkGray);
        }

        if (!string.IsNullOrEmpty(_label))
        {
            UIUtils.DrawStringWithManualSpaces(spriteBatch, _font, _label, new Vector2(_position.X, _position.Y + _labelOffsetY), Color.Black, _fontScale);
        }

        // Setup Clipping (ScissorRectangle)
        var oldScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
        var oldRasterizer = spriteBatch.GraphicsDevice.RasterizerState;
        Rectangle clipRect = new Rectangle(Bounds.X + 10, Bounds.Y, Bounds.Width - 20, Bounds.Height);
        
        spriteBatch.End();
        RasterizerState rs = new RasterizerState { ScissorTestEnable = true };
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rs, null, transformMatrix);
        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(oldScissor, clipRect);

        string textToRender = _text;
        bool showCursor = _isFocused && (DateTime.Now.Millisecond / 500) % 2 == 0;
        
        float totalTextWidth = UIUtils.CalculateTextWidth(_font, _text, _fontScale);
        float maxVisibleWidth = clipRect.Width - 10;
        float scrollOffset = Math.Max(0, totalTextWidth - maxVisibleWidth);
        
        Vector2 startPos = new Vector2(clipRect.X - scrollOffset, Bounds.Y + (Bounds.Height - _font.MeasureString("A").Y * _fontScale) / 2);
        
        UIUtils.DrawStringWithManualSpaces(spriteBatch, _font, textToRender, startPos, _textColor, _fontScale);
        
        if (showCursor)
        {
            Vector2 cursorPos = new Vector2(startPos.X + totalTextWidth, startPos.Y);
            spriteBatch.DrawString(_font, "|", cursorPos, _textColor, 0f, Vector2.Zero, _fontScale, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, oldRasterizer, null, transformMatrix);
        spriteBatch.GraphicsDevice.ScissorRectangle = oldScissor;
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
        return null;
    }
}
