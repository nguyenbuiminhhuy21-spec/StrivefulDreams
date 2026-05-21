using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code_Game.Scripts.UI;

public class Select
{
    private readonly SpriteFont _font;
    private readonly List<SelectOption> _options;
    private Vector2 _position;
    private int _width;
    private int _height;
    
    private readonly Texture2D _listBgTexture;
    private Texture2D _mainBoxTexture;
    private readonly Texture2D _itemHoverTexture;
    private readonly Texture2D _descFrameTexture;
    private Texture2D _whitePixel;
    
    private readonly string _label;

    private int _selectedIndex = 0;
    private bool _isOpen = false;
    private Rectangle _bounds;
    private MouseState _prevMouseState;
    private int _hoverIndex = -1;
    private Rectangle _descRect;

    // Scrolling logic
    private int _scrollIndex = 0;
    private const int MaxVisibleItems = 4;

    public int SelectedIndex => _selectedIndex;
    public SelectOption SelectedOption => _options[_selectedIndex];
    public int LabelOffsetY { get; set; } = -35;
    public float FontScale { get; set; } = 1.0f;

    public bool ShowText { get; set; } = true;
    public bool ShowBackground { get; set; } = true;

    public event Action<string> OnSelectionChanged;

    public float DescIconX, DescIconY, DescIconScale;
    public float DescTitleX, DescTitleY, DescTitleScale;
    public float DescContentX, DescContentY, DescContentScale;
    public float DescFrameWidth, DescFrameHeight;

    public class SelectOption
    {
        public string Text { get; }
        public string Description { get; }
        public Texture2D MainImage { get; }
        public Texture2D Icon { get; }
        public string Key { get; }
        public SelectOption(string text, string description = "", Texture2D mainImage = null, Texture2D icon = null, string key = "") { Text = text; Description = description; MainImage = mainImage; Icon = icon; Key = key ?? text; }
    }

    public Select(SpriteFont font, List<SelectOption> options, Vector2 position, int width, int height, Texture2D listBgTexture, Texture2D mainBoxTexture, string label = "", Texture2D descFrameTexture = null, Texture2D itemHoverTexture = null)
    {
        _font = font; _options = options; _position = position; _width = width; _height = height; _listBgTexture = listBgTexture; _mainBoxTexture = mainBoxTexture; _label = label; _descFrameTexture = descFrameTexture; _itemHoverTexture = itemHoverTexture;
        UpdateBounds();
    }

    public void UpdateLayout(Vector2 position, int width, int height) { _position = position; _width = width; _height = height; UpdateBounds(); }
    private void UpdateBounds() { _bounds = new Rectangle((int)_position.X, (int)_position.Y, _width, _height); }

    public Rectangle GetBounds() => _bounds;
    public void SetMainBoxTexture(Texture2D texture) { _mainBoxTexture = texture; }

    public void Update(GameTime gameTime)
    {
        var ms = Mouse.GetState();
        Update(gameTime, new Point(ms.X, ms.Y));
    }

    public void Update(GameTime gameTime, Point mousePoint)
    {
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released) {
            if (_bounds.Contains(mousePoint)) _isOpen = !_isOpen;
            else if (_isOpen) {
                Rectangle listBounds = new Rectangle(_bounds.X, _bounds.Bottom, _width, _height * _options.Count);
                if (listBounds.Contains(mousePoint)) {
                    int clickedRelativeIndex = (mousePoint.Y - _bounds.Bottom) / _height;
                    if (clickedRelativeIndex >= 0 && clickedRelativeIndex < _options.Count) { 
                        _selectedIndex = clickedRelativeIndex; 
                        _isOpen = false; 
                        OnSelectionChanged?.Invoke(_options[_selectedIndex].Key); 
                        _scrollIndex = 0; 
                    }
                }
                else _isOpen = false;
            }
        }
        if (_isOpen) {
            Rectangle listBounds = new Rectangle(_bounds.X, _bounds.Bottom, _width, _height * Math.Min(_options.Count, MaxVisibleItems));
            int wheelDelta = mouseState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
            if (wheelDelta > 0) _scrollIndex = Math.Max(0, _scrollIndex - 1);
            else if (wheelDelta < 0) _scrollIndex = Math.Min(Math.Max(0, _options.Count - MaxVisibleItems), _scrollIndex + 1);
            if (listBounds.Contains(mousePoint)) { 
                _hoverIndex = _scrollIndex + (mousePoint.Y - _bounds.Bottom) / _height; 
            }
            else if (_hoverIndex >= 0 && _descRect.Contains(mousePoint)) {
                // Keep hover index if mouse is over description
            }
            else _hoverIndex = -1;
        }
        else _hoverIndex = -1;
        _prevMouseState = mouseState;
    }

    private int GetActualIndexFromDropdown(int relativeInView) {
        int totalToSkip = _scrollIndex + relativeInView; int count = 0;
        for (int i = 0; i < _options.Count; i++) { if (i == _selectedIndex) continue; if (count == totalToSkip) return i; count++; }
        return -1;
    }

    public void Draw(SpriteBatch spriteBatch, Matrix? transformMatrix = null, int virtualWidth = 1280)
    {
        if (_whitePixel == null) { _whitePixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1); _whitePixel.SetData(new[] { Color.White }); }
        if (!string.IsNullOrEmpty(_label)) UIUtils.DrawStringWithManualSpaces(spriteBatch, _font, _label, new Vector2(_position.X, _position.Y + LabelOffsetY), Color.Black, FontScale);
        
        if (ShowBackground && _mainBoxTexture != null) spriteBatch.Draw(_mainBoxTexture, _bounds, Color.White);
        
        var opt = _options[_selectedIndex];
        if (ShowText) {
            Vector2 textPos = new Vector2(_bounds.X + 12, _bounds.Y + (_height - _font.MeasureString("A").Y * FontScale) / 2);
            UIUtils.DrawStringWithManualSpaces(spriteBatch, _font, opt.Text, textPos, Color.Black, FontScale);
        }

        if (_isOpen) {
            int visibleCount = Math.Min(_options.Count, MaxVisibleItems);
            Rectangle listBounds = new Rectangle(_bounds.X, _bounds.Bottom, _width, _height * visibleCount);
            if (_listBgTexture != null) spriteBatch.Draw(_listBgTexture, listBounds, Color.White);
            
            var oldScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
            var oldRasterizer = spriteBatch.GraphicsDevice.RasterizerState;
            spriteBatch.End();
            RasterizerState rs = new RasterizerState { ScissorTestEnable = true };
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rs, null, transformMatrix);
            spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(oldScissor, listBounds);

            int dropdownIndex = 0;
            for (int i = 0; i < visibleCount; i++) {
                int itemIdx = _scrollIndex + i;
                if (itemIdx >= _options.Count) break;
                Rectangle itemRect = new Rectangle(_bounds.X, _bounds.Bottom + (i * _height), _width, _height);
                if (itemIdx == _hoverIndex) {
                    Color hoverColor = new Color(255, 165, 0); int borderT = 2;
                    spriteBatch.Draw(_whitePixel, new Rectangle(itemRect.X + 4, itemRect.Y, itemRect.Width - 8, borderT), hoverColor);
                    spriteBatch.Draw(_whitePixel, new Rectangle(itemRect.X + 4, itemRect.Bottom - borderT, itemRect.Width - 8, borderT), hoverColor);
                }
                if (ShowText) UIUtils.DrawStringWithManualSpaces(spriteBatch, _font, _options[itemIdx].Text, new Vector2(itemRect.X + 12, itemRect.Y + (_height - _font.MeasureString("A").Y * FontScale) / 2), Color.Black, FontScale);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, oldRasterizer, null, transformMatrix);
            spriteBatch.GraphicsDevice.ScissorRectangle = oldScissor;

            if (_options.Count > MaxVisibleItems) {
                int scrollTrackX = listBounds.Right - 8, scrollTrackY = listBounds.Y + 4, scrollTrackH = listBounds.Height - 8, scrollTrackW = 4;
                spriteBatch.Draw(_whitePixel, new Rectangle(scrollTrackX, scrollTrackY, scrollTrackW, scrollTrackH), new Color(0, 0, 0, 50));
                float thumbHeightRatio = (float)MaxVisibleItems / _options.Count;
                int thumbH = (int)(scrollTrackH * thumbHeightRatio);
                float scrollRatio = (float)_scrollIndex / (_options.Count - MaxVisibleItems);
                int thumbY = scrollTrackY + (int)((scrollTrackH - thumbH) * scrollRatio);
                spriteBatch.Draw(_whitePixel, new Rectangle(scrollTrackX, thumbY, scrollTrackW, thumbH), Color.SaddleBrown);
            }
            if (_hoverIndex >= 0 && _hoverIndex < _options.Count && !string.IsNullOrEmpty(_options[_hoverIndex].Description)) DrawDescription(spriteBatch, _options[_hoverIndex], virtualWidth);
        }
    }

    private void DrawDescription(SpriteBatch sb, SelectOption option, int virtualWidth) {
        if (_descFrameTexture == null) return;
        int x = _bounds.Right + 10;
        if (x + DescFrameWidth > virtualWidth) x = _bounds.Left - 10 - (int)DescFrameWidth;
        _descRect = new Rectangle(x, _bounds.Y, (int)DescFrameWidth, (int)DescFrameHeight);
        sb.Draw(_descFrameTexture, _descRect, Color.White);
        if (option.Icon != null) sb.Draw(option.Icon, new Vector2(_descRect.X + DescIconX, _descRect.Y + DescIconY), null, Color.White, 0f, Vector2.Zero, DescIconScale, SpriteEffects.None, 0f);
        UIUtils.DrawStringWithManualSpaces(sb, _font, option.Text, new Vector2(_descRect.X + DescTitleX, _descRect.Y + DescTitleY), Color.SaddleBrown, DescTitleScale);
        if (!string.IsNullOrEmpty(option.Description)) { float contentScale = DescContentScale, wrapWidth = _descRect.Width - (DescContentX * 2); DrawWrappedString(sb, _font, option.Description, new Vector2(_descRect.X + DescContentX, _descRect.Y + DescContentY), Color.Black, contentScale, wrapWidth); }
    }

    private void DrawWrappedString(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, float scale, float maxWidth) {
        string[] words = text.Split(' '); Vector2 currentPos = pos; float spaceWidth = UIUtils.DefaultSpaceWidth * scale;
        foreach (var word in words) { float wordWidth = font.MeasureString(word).X * scale; if (currentPos.X + wordWidth > pos.X + maxWidth) { currentPos.X = pos.X; currentPos.Y += font.LineSpacing * scale; } UIUtils.DrawStringWithManualSpaces(sb, font, word, currentPos, color, scale); currentPos.X += wordWidth + spaceWidth; }
    }
}
