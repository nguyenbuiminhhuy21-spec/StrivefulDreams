using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Screens.CharacterCreation;

namespace Code_Game.Scripts.UI;

public class ColorPicker
{
    private readonly GraphicsDevice _graphicsDevice;
    private Texture2D _colorWheelTexture;
    private Texture2D _brightnessBarTexture;
    private Texture2D _handleTexture;
    private Texture2D _whitePixel;
    private Texture2D _panelTexture;
    
    private float _hue = 0f;
    private float _saturation = 0f;
    private float _brightness = 0f;
    
    private Rectangle _wheelRect;
    private Rectangle _barRect;
    private Rectangle _bgRect;
    private bool _isDraggingWheel;
    private bool _isDraggingBar;

    public event Action<Color> OnColorChanged;
    public Color SelectedColor { get; private set; } = Color.White;

    public Rectangle BoundingBox => _bgRect;

    private float _lastWheelBorderWidth = -1;
    private float _lastBarBorderWidth = -1;
    private float _lastBarRadius = -1;

    public ColorPicker(GraphicsDevice graphicsDevice, int size)
    {
        _graphicsDevice = graphicsDevice;
        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        _handleTexture = CreateCircleTexture(64);
        _panelTexture = CreateRoundedRectTexture(128, 128, 16);
        UpdateFinalColor();
    }

    public void Update(GameTime gameTime, Rectangle iconRect, CharacterCreationLayoutData.UIElementConfig config)
    {
        var mouseState = Mouse.GetState();
        var barCfg = config.Bar;
        var wheelCfg = config.Wheel;
        var ctrl = config.Controls;

        if (iconRect.Width <= 0 || iconRect.Height <= 0) return;

        float res = _graphicsDevice.Viewport.Height / 720f;
        float wheelScale = wheelCfg.GetScale();
        int wheelSize = (int)(iconRect.Width * wheelScale);
        
        _wheelRect = new Rectangle(
            iconRect.X + (int)(ctrl.GetModalOffsetX() * res),
            iconRect.Y + (int)(ctrl.GetModalOffsetY() * res),
            wheelSize,
            wheelSize
        );

        float s = config.GetScale() * res;
        int barH = (int)(barCfg.GetWidth() * s);
        if (barH <= 0) barH = 1;

        _barRect = new Rectangle(
            _wheelRect.X + (int)(barCfg.GetOffsetX() * res),
            _wheelRect.Bottom + (int)(barCfg.GetOffsetY() * res) + (int)(10 * res),
            _wheelRect.Width,
            barH
        );

        int padding = (int)(15 * res);
        int minX = Math.Min(_wheelRect.X, _barRect.X);
        int minY = Math.Min(_wheelRect.Y, _barRect.Y);
        int maxX = Math.Max(_wheelRect.Right, _barRect.Right);
        int maxY = Math.Max(_wheelRect.Bottom, _barRect.Bottom);
        _bgRect = new Rectangle(minX - padding, minY - padding, (maxX - minX) + padding * 2, (maxY - minY) + padding * 2);

        if (_lastWheelBorderWidth != config.Wheel.GetBorderWidth() || _lastBarBorderWidth != barCfg.GetBorderWidth() || _lastBarRadius != barCfg.GetRadius()) RegenerateTextures(config);
        HandleInput(mouseState);
    }

    private void RegenerateTextures(CharacterCreationLayoutData.UIElementConfig config)
    {
        if (_wheelRect.Width <= 0 || _barRect.Width <= 0 || _barRect.Height <= 0) return;
        _colorWheelTexture?.Dispose(); _brightnessBarTexture?.Dispose();
        _colorWheelTexture = CreateColorWheel(_wheelRect.Width, config.Wheel.GetBorderWidth(), ParseColor(config.Wheel.BorderColor, Color.White));
        _brightnessBarTexture = CreateBrightnessBar(_barRect.Width, _barRect.Height, config.Bar.GetRadius(), config.Bar.GetBorderWidth(), ParseColor(config.Bar.BorderColor, Color.White));
        _lastWheelBorderWidth = config.Wheel.GetBorderWidth(); _lastBarBorderWidth = config.Bar.GetBorderWidth(); _lastBarRadius = config.Bar.GetRadius();
    }

    private void HandleInput(MouseState mouse)
    {
        Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            if (!_isDraggingBar && (_wheelRect.Contains(mousePos) || _isDraggingWheel)) { _isDraggingWheel = true; UpdateWheelColor(mousePos); }
            else if (!_isDraggingWheel && (_barRect.Contains(mousePos) || _isDraggingBar)) { _isDraggingBar = true; _brightness = MathHelper.Clamp((mousePos.X - _barRect.X) / (float)_barRect.Width, 0, 1); UpdateFinalColor(); }
        }
        else { _isDraggingWheel = false; _isDraggingBar = false; }
    }

    private void UpdateWheelColor(Vector2 mousePos)
    {
        Vector2 center = new Vector2(_wheelRect.X + _wheelRect.Width / 2f, _wheelRect.Y + _wheelRect.Height / 2f);
        Vector2 diff = mousePos - center;
        float dist = diff.Length(), maxR = _wheelRect.Width / 2f;
        if (dist > 0) { float angle = (float)Math.Atan2(diff.Y, diff.X); _hue = (angle / MathHelper.TwoPi); if (_hue < 0) _hue += 1f; _saturation = MathHelper.Clamp(dist / maxR, 0, 1); UpdateFinalColor(); }
    }

    private void UpdateFinalColor() { SelectedColor = HSVToRGB(_hue, _saturation, 1f - _brightness); OnColorChanged?.Invoke(SelectedColor); }

    private Color HSVToRGB(float h, float s, float v)
    {
        int hi = Convert.ToInt32(Math.Floor(h * 6)) % 6;
        float f = h * 6 - (float)Math.Floor(h * 6), p = v * (1 - s), q = v * (1 - f * s), t = v * (1 - (1 - f) * s);
        return hi switch { 0 => new Color(v, t, p), 1 => new Color(q, v, p), 2 => new Color(p, v, t), 3 => new Color(p, q, v), 4 => new Color(t, p, v), _ => new Color(v, p, q) };
    }

    public void Draw(SpriteBatch spriteBatch, CharacterCreationLayoutData.UIElementConfig config)
    {
        if (_colorWheelTexture == null || _brightnessBarTexture == null) return;
        spriteBatch.Draw(_panelTexture, _bgRect, new Color(40, 40, 40, 240));
        spriteBatch.Draw(_colorWheelTexture, _wheelRect, Color.White);
        spriteBatch.Draw(_brightnessBarTexture, _barRect, Color.White);
        var wHdl = config.Wheel.Handle;
        Vector2 hPos = new Vector2(_wheelRect.X + _wheelRect.Width / 2f, _wheelRect.Y + _wheelRect.Height / 2f) + new Vector2((float)Math.Cos(_hue * MathHelper.TwoPi), (float)Math.Sin(_hue * MathHelper.TwoPi)) * (_saturation * (_wheelRect.Width / 2f));
        DrawCircularHandle(spriteBatch, hPos, wHdl.GetSize(), HSVToRGB(_hue, _saturation, 1f), ParseColor(wHdl.BorderColor, Color.Black), wHdl.GetBorderWidth());
        var bHdl = config.Bar.Handle;
        Vector2 bHPos = new Vector2(_barRect.X + (_brightness * _barRect.Width), _barRect.Y + (_barRect.Height / 2f));
        DrawCircularHandle(spriteBatch, bHPos, bHdl.GetSize(), new Color(1f - _brightness, 1f - _brightness, 1f - _brightness), ParseColor(bHdl.BorderColor, Color.Black), bHdl.GetBorderWidth());
    }

    private void DrawCircularHandle(SpriteBatch sb, Vector2 pos, float size, Color color, Color borderColor, float borderWidth)
    {
        if (_handleTexture == null || size <= 0) return;
        Vector2 origin = new Vector2(_handleTexture.Width / 2f, _handleTexture.Height / 2f);
        sb.Draw(_handleTexture, pos, null, borderColor, 0f, origin, size / _handleTexture.Width, SpriteEffects.None, 0f);
        float innerS = (size - borderWidth * 2) / _handleTexture.Width;
        if (innerS > 0) sb.Draw(_handleTexture, pos, null, color, 0f, origin, innerS, SpriteEffects.None, 0f);
    }

    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(_graphicsDevice, size, size);
        Color[] data = new Color[size * size];
        float r = size / 2f;
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) { float dx = x - r + 0.5f, dy = y - r + 0.5f; data[y * size + x] = (float)Math.Sqrt(dx * dx + dy * dy) <= r ? Color.White : Color.Transparent; }
        tex.SetData(data); return tex;
    }

    private Texture2D CreateRoundedRectTexture(int w, int h, float r)
    {
        Texture2D tex = new Texture2D(_graphicsDevice, w, h);
        Color[] data = new Color[w * h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) data[y * w + x] = IsInsideRoundedRect(x, y, w, h, r) ? Color.White : Color.Transparent;
        tex.SetData(data); return tex;
    }

    private Texture2D CreateColorWheel(int size, float borderWidth, Color borderColor)
    {
        Texture2D tex = new Texture2D(_graphicsDevice, size, size);
        Color[] data = new Color[size * size];
        float r = size / 2f, innerR = r - borderWidth;
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) { float dx = x - r, dy = y - r, dist = (float)Math.Sqrt(dx * dx + dy * dy); if (dist <= r) { if (dist > innerR && borderWidth > 0) data[y * size + x] = borderColor; else { float h = ((float)Math.Atan2(dy, dx) / MathHelper.TwoPi); if (h < 0) h += 1f; data[y * size + x] = HSVToRGB(h, dist / innerR, 1f); } } else data[y * size + x] = Color.Transparent; }
        tex.SetData(data); return tex;
    }

    private Texture2D CreateBrightnessBar(int w, int h, float cornerRadius, float borderWidth, Color borderColor)
    {
        Texture2D tex = new Texture2D(_graphicsDevice, w, h);
        Color[] data = new Color[w * h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) {
            float val = 1f - (float)x / w; Color color = new Color(val, val, val);
            if (IsInsideRoundedRect(x, y, w, h, cornerRadius)) { if (borderWidth > 0 && IsOnBorder(x, y, w, h, cornerRadius, borderWidth)) data[y * w + x] = borderColor; else data[y * w + x] = color; } else data[y * w + x] = Color.Transparent;
        }
        tex.SetData(data); return tex;
    }

    private bool IsInsideRoundedRect(int x, int y, int w, int h, float r)
    {
        if (x >= r && x <= w - r) return y >= 0 && y <= h; if (y >= r && y <= h - r) return x >= 0 && x <= w;
        if (x < r && y < r) return Vector2.Distance(new Vector2(x, y), new Vector2(r, r)) <= r;
        if (x > w - r && y < r) return Vector2.Distance(new Vector2(x, y), new Vector2(w - r, r)) <= r;
        if (x < r && y > h - r) return Vector2.Distance(new Vector2(x, y), new Vector2(r, h - r)) <= r;
        if (x > w - r && y > h - r) return Vector2.Distance(new Vector2(w - r, h - r), new Vector2(x, y)) <= r;
        return false;
    }
    private bool IsOnBorder(int x, int y, int w, int h, float r, float b) => !IsInsideRoundedRect(x, (int)(y + b), w, (int)(h - b * 2), r - b) || x < b || x >= w - b || y < b || y >= h - b;
    private Color ParseColor(string s, Color d) { try { var p = s.Split(','); if (p.Length == 3) return new Color(byte.Parse(p[0]), byte.Parse(p[1]), byte.Parse(p[2])); } catch { } return d; }
}
