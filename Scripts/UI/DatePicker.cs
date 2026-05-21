using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Services.Localization;

namespace Code_Game.Scripts.UI;

public class DatePicker
{
    private readonly SpriteFont _font;
    private Vector2 _position;
    private readonly string _label;
    private readonly Texture2D _listBgTexture;
    private Texture2D _inputFrameTexture;
    private Texture2D _arrowIcon;
    private Texture2D _whitePixel;

    // Data
    private readonly List<string> _seasons;
    private readonly List<Color> _seasonColors;
    private readonly List<string> _days;

    // State
    private int _selectedSeasonIndex = 0;
    private bool _isSeasonOpen = false;
    private Rectangle _seasonBounds;
    private int _seasonHoverIndex = -1;
    private int _seasonScrollIndex = 0;

    private int _selectedDayIndex = 0;
    private bool _isDayOpen = false;
    private Rectangle _dayBounds;
    private int _dayHoverIndex = -1;
    private int _dayScrollIndex = 0;

    // Layout
    private int _seasonWidth = 140, _dayWidth = 80, _height = 44;
    private int _seasonOffsetX, _dayOffsetX;
    
    // Style
    private float _seasonArrowOffsetX, _seasonArrowOffsetY, _seasonArrowScale = 0.4f;
    private float _dayArrowOffsetX, _dayArrowOffsetY, _dayArrowScale = 0.4f;
    private float _iconPadding = 0; 

    private MouseState _prevMouseState;
    private const int MaxVisibleItems = 5;

    public int SelectedSeason => _selectedSeasonIndex;
    public int SelectedDay => _selectedDayIndex + 1;
    public int LabelOffsetY { get; set; } = 0;
    public int LabelOffsetX { get; set; } = 0;
    public float FontScale { get; set; } = 1.0f;

    public DatePicker(SpriteFont font, Vector2 position, Texture2D listBg, Texture2D inputFrame, string label, Texture2D unused, List<Texture2D> seasonIcons = null)
    {
        _font = font; _position = position; _listBgTexture = listBg; _inputFrameTexture = inputFrame; _label = label;
        var loc = LocalizationService.Instance;
        _seasons = new List<string> { loc.Get("season.spring").ToLower(), loc.Get("season.summer").ToLower(), loc.Get("season.fall").ToLower(), loc.Get("season.winter").ToLower() };
        _seasonColors = new List<Color> { Color.LightPink, Color.LightGreen, Color.Orange, Color.LightBlue };
        _days = new List<string>(); for (int i = 1; i <= 31; i++) _days.Add(i.ToString());
    }

    public void SetDaySelectorStyle(Texture2D background, Texture2D arrowIcon) { _inputFrameTexture = background; _arrowIcon = arrowIcon; }
    public void SetSeasonSelectorStyle(Texture2D background, Texture2D arrowIcon) { _inputFrameTexture = background; _arrowIcon = arrowIcon; }

    public void SetSelectorArrowStyle(bool isSeason, float? offsetX, float? offsetY, float? scale)
    {
        if (isSeason) { if (offsetX.HasValue) _seasonArrowOffsetX = offsetX.Value; if (offsetY.HasValue) _seasonArrowOffsetY = offsetY.Value; if (scale.HasValue) _seasonArrowScale = scale.Value; }
        else { if (offsetX.HasValue) _dayArrowOffsetX = offsetX.Value; if (offsetY.HasValue) _dayArrowOffsetY = offsetY.Value; if (scale.HasValue) _dayArrowScale = scale.Value; }
    }

    public void SetSelectorIconStyle(bool isSeason, float? offsetX, float? offsetY, float? scale, float? padding = null, bool? unusedStretch = null)
    {
        // This method was redundant and overwriting arrow styles. 
        // We'll keep it for compatibility but only update padding if provided.
        if (padding.HasValue) _iconPadding = padding.Value;
    }

    public void UpdateLayout(Vector2 position, int seasonWidth, int dayWidth, int seasonOffsetX, int dayOffsetX)
    {
        _position = position; _seasonWidth = seasonWidth; _dayWidth = dayWidth; _seasonOffsetX = seasonOffsetX; _dayOffsetX = dayOffsetX;
        _seasonBounds = new Rectangle((int)(_position.X + _seasonOffsetX), (int)_position.Y, _seasonWidth, _height);
        _dayBounds = new Rectangle((int)(_position.X + _dayOffsetX), (int)_position.Y, _dayWidth, _height);
    }

    // Now accepts the correctly transformed mouse point
    public void Update(GameTime gameTime, Point mousePoint)
    {
        var ms = Mouse.GetState();
        bool leftPressed = ms.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released;
        int wheelDelta = ms.ScrollWheelValue - _prevMouseState.ScrollWheelValue;

        if (_isSeasonOpen) {
            Rectangle listBounds = new Rectangle(_seasonBounds.X, _seasonBounds.Bottom, _seasonBounds.Width, _height * Math.Min(_seasons.Count, MaxVisibleItems));
            if (leftPressed) {
                if (listBounds.Contains(mousePoint)) {
                    int clickedIdx = _seasonScrollIndex + (mousePoint.Y - _seasonBounds.Bottom) / _height;
                    if (clickedIdx >= 0 && clickedIdx < _seasons.Count) { _selectedSeasonIndex = clickedIdx; _isSeasonOpen = false; }
                } else { _isSeasonOpen = false; }
            } else {
                if (listBounds.Contains(mousePoint)) _seasonHoverIndex = _seasonScrollIndex + (mousePoint.Y - _seasonBounds.Bottom) / _height;
                else _seasonHoverIndex = -1;
                if (wheelDelta > 0) _seasonScrollIndex = Math.Max(0, _seasonScrollIndex - 1);
                else if (wheelDelta < 0) _seasonScrollIndex = Math.Min(Math.Max(0, _seasons.Count - MaxVisibleItems), _seasonScrollIndex + 1);
            }
        } else if (_isDayOpen) {
            Rectangle listBounds = new Rectangle(_dayBounds.X, _dayBounds.Bottom, _dayBounds.Width, _height * Math.Min(_days.Count, MaxVisibleItems));
            if (leftPressed) {
                if (listBounds.Contains(mousePoint)) {
                    int clickedIdx = _dayScrollIndex + (mousePoint.Y - _dayBounds.Bottom) / _height;
                    if (clickedIdx >= 0 && clickedIdx < _days.Count) { _selectedDayIndex = clickedIdx; _isDayOpen = false; }
                } else { _isDayOpen = false; }
            } else {
                if (listBounds.Contains(mousePoint)) _dayHoverIndex = _dayScrollIndex + (mousePoint.Y - _dayBounds.Bottom) / _height;
                else _dayHoverIndex = -1;
                if (wheelDelta > 0) _dayScrollIndex = Math.Max(0, _dayScrollIndex - 1);
                else if (wheelDelta < 0) _dayScrollIndex = Math.Min(Math.Max(0, _days.Count - MaxVisibleItems), _dayScrollIndex + 1);
            }
        } else if (leftPressed) {
            if (_seasonBounds.Contains(mousePoint)) { _isSeasonOpen = true; _isDayOpen = false; }
            else if (_dayBounds.Contains(mousePoint)) { _isDayOpen = true; _isSeasonOpen = false; }
        }
        _prevMouseState = ms;
    }

    public void Draw(SpriteBatch sb)
    {
        if (_whitePixel == null) { _whitePixel = new Texture2D(sb.GraphicsDevice, 1, 1); _whitePixel.SetData(new[] { Color.White }); }
        if (!string.IsNullOrEmpty(_label)) {
            float labelY = _position.Y + (_height - _font.MeasureString("A").Y * FontScale) / 2 + LabelOffsetY;
            UIUtils.DrawStringWithManualSpaces(sb, _font, _label, new Vector2(_position.X + LabelOffsetX, labelY), Color.Black, FontScale);
        }
        DrawSelector(sb, _seasonBounds, _selectedSeasonIndex, _seasons, _isSeasonOpen, _seasonHoverIndex, _seasonScrollIndex, _seasonArrowOffsetX, _seasonArrowOffsetY, _seasonArrowScale, true);
        DrawSelector(sb, _dayBounds, _selectedDayIndex, _days, _isDayOpen, _dayHoverIndex, _dayScrollIndex, _dayArrowOffsetX, _dayArrowOffsetY, _dayArrowScale, false);
    }

    private void DrawSelector(SpriteBatch sb, Rectangle bounds, int selectedIdx, List<string> items, bool isOpen, int hoverIdx, int scrollIdx, float arrowX, float arrowY, float arrowScale, bool isSeason)
    {
        if (_inputFrameTexture != null) sb.Draw(_inputFrameTexture, bounds, Color.White);
        if (isSeason && selectedIdx < _seasonColors.Count) {
            // Draw a slightly smaller color box inside the frame
            Rectangle colorBox = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);
            sb.Draw(_whitePixel, colorBox, _seasonColors[selectedIdx]);
        }
        if (!isSeason) { Vector2 textPos = new Vector2(bounds.X + (bounds.Width - _font.MeasureString(items[selectedIdx]).X * FontScale) / 2, bounds.Y + (_height - _font.MeasureString("A").Y * FontScale) / 2); UIUtils.DrawStringWithManualSpaces(sb, _font, items[selectedIdx], textPos, Color.Black, FontScale); }
        if (_arrowIcon != null) { int arrowSize = (int)(bounds.Height * arrowScale); Rectangle arrowRect = new Rectangle((int)(bounds.Right + arrowX - arrowSize), (int)(bounds.Y + (bounds.Height - arrowSize) / 2 + arrowY), arrowSize, arrowSize); sb.Draw(_arrowIcon, arrowRect, Color.White); }
        if (isOpen) {
            int visibleCount = Math.Min(items.Count, MaxVisibleItems);
            Rectangle listBounds = new Rectangle(bounds.X, bounds.Bottom, bounds.Width, _height * visibleCount);
            if (_listBgTexture != null) sb.Draw(_listBgTexture, listBounds, Color.White);
            for (int i = 0; i < visibleCount; i++) {
                int itemIdx = scrollIdx + i; if (itemIdx >= items.Count) break;
                Rectangle itemRect = new Rectangle(bounds.X, bounds.Bottom + i * _height, bounds.Width, _height);
                if (itemIdx == hoverIdx) {
                    Color hCol = new Color(255, 165, 0); int bT = 2;
                    sb.Draw(_whitePixel, new Rectangle(itemRect.X + 4, itemRect.Y, itemRect.Width - 8, bT), hCol);
                    sb.Draw(_whitePixel, new Rectangle(itemRect.X + 4, itemRect.Bottom - bT, itemRect.Width - 8, bT), hCol);
                }
                if (isSeason) { Rectangle colorBox = new Rectangle(itemRect.X + 2, itemRect.Y + 2, itemRect.Width - 4, itemRect.Height - 4); sb.Draw(_whitePixel, colorBox, _seasonColors[itemIdx]); }
                else { Vector2 textPos = new Vector2(itemRect.X + (itemRect.Width - _font.MeasureString(items[itemIdx]).X * FontScale) / 2, itemRect.Y + (_height - _font.MeasureString("A").Y * FontScale) / 2); UIUtils.DrawStringWithManualSpaces(sb, _font, items[itemIdx], textPos, Color.Black, FontScale); }
            }
        }
    }
}
