using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Services;
using Code_Game.Scripts.Services.CharacterCreation;
using Code_Game.Scripts.Services.Localization;
using Code_Game.Scripts.UI;
using Code_Game.Scripts.Screens.Beginning;
using Code_Game.Scripts.Contracts.CharacterCreation;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Code_Game.Scripts.Screens.CharacterCreation;

public class CharacterCreationScreen : Screen
{
    private SpriteFont _font;
    private Texture2D _panelTexture;
    private Texture2D _bgTexture;
    private Texture2D _frameTexture;
    private Texture2D _selectTexture;
    private Texture2D _selectMainTexture;
    private Texture2D _selectItemTexture;
    private Texture2D _descFrameTexture;
    private Texture2D _inputTexture;
    private List<Texture2D> _farmTypeTextures = new();
    private Dictionary<string, Texture2D> _advantageIcons = new();
    private List<List<BackgroundFrame>> _farmTypeFramesList = new();
    private int _currentFarmTypeFrame = 0;
    private int _selectedFarmTypeIndex = 0;
    private int _hoveredFarmTypeIndex = -1;
    private double _farmTypeFrameTimer = 0;
    private Texture2D _colorPickerTexture;
    private List<Rectangle> _farmTypeRects = new();
    private static Texture2D _squareTexture;
    
    private Rectangle _panelRect;
    private Rectangle _frameRect;
    private Matrix _uiScaleMatrix;
    
    private Button _okButton;
    private Button _backButton;
    private TextInput _nameInput;
    private TextInput _farmNameInput;
    private Select _advantageSelect;
    private DatePicker _datePicker;
    private ColorPicker _colorPicker;
    private Button _framePrevBtn;
    private Button _frameNextBtn;
    
    private CharacterCreationLayoutData _layout;
    private readonly ICharacterCreationService _creationService;
    private int _lastViewportWidth, _lastViewportHeight;
    private int _virtualWidth = 1280;
    private MouseState _prevMouse;
    
    private readonly List<CategoryUI> _categories = new();
    private int _activeCategoryIndex = -1;

    private List<BackgroundFrame> _bgFrames = new();
    private int _currentBgFrame = 0;
    private float _bgFrameTimer = 0;

    public CharacterCreationScreen(Game game, ScreenManager screenManager) : base(game, screenManager)
    {
        _creationService = CharacterCreationService.Instance;
        LoadLayout();
    }

    private void LoadLayout()
    {
        try {
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Content/Data/Screens/CharacterCreationLayout.json");
            
            if (!File.Exists(jsonPath))
            {
                jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/Data/Screens/CharacterCreationLayout.json");
                Console.WriteLine($"[Debug] Source JSON not found, using build path: {jsonPath}");
            }
            else
            {
                Console.WriteLine($"[Debug] Loading layout from: {jsonPath}");
            }

            if (File.Exists(jsonPath)) {
                string json = File.ReadAllText(jsonPath);
                _layout = JsonConvert.DeserializeObject<CharacterCreationLayoutData>(json);
                _layout?.AppearanceSelectors?.ApplyCommon();
                _layout?.IdentitySelectors?.ApplyCommon();
                Console.WriteLine("[Debug] Layout loaded successfully.");
            }
            else
            {
                Console.WriteLine("[Error] Layout file NOT found in any known path.");
            }
        } catch (Exception ex) { 
            Console.WriteLine($"[Error] Failed to load layout: {ex.Message}");
            _layout = new CharacterCreationLayoutData(); 
        }
    }

    public override void LoadContent()
    {
        LoadLayout();
        _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");
        _panelTexture = Game.Content.Load<Texture2D>("Graphics/Screens/CharacterCreation/screen-character-creation-400");
        _bgTexture = Game.Content.Load<Texture2D>("Graphics/Screens/Common/no_title_bg");
        _frameTexture = Game.Content.Load<Texture2D>("Graphics/Screens/CharacterCreation/frame-create-new-character-x4");
        _selectTexture = Game.Content.Load<Texture2D>("Graphics/UI/Input/frame-dropdow-x84-x4");
        _selectMainTexture = Game.Content.Load<Texture2D>("Graphics/UI/Input/frame-select-x84-22-x4");
        _selectItemTexture = Game.Content.Load<Texture2D>("Graphics/UI/Input/frame-select-item");
        _descFrameTexture = Game.Content.Load<Texture2D>("Graphics/UI/Input/frame-description");
        _inputTexture = Game.Content.Load<Texture2D>("Graphics/UI/Input/frame-input");
        _colorPickerTexture = Game.Content.Load<Texture2D>("Graphics/Screens/CharacterCreation/frame-change-color/fram-change-colo-x4r");
        
        try { 
            LoadAllFarmTypes();
        } catch { }
        
        string[] advKeys = { "normal", "farmer", "tamer", "cooker", "strenger", "woodcutter", "fisher", "communicator", "gatherers" };
        foreach (var k in advKeys) {
            try { _advantageIcons[k] = Game.Content.Load<Texture2D>($"Graphics/Icons/Advantages/{k}"); } catch { }
        }

        if (_squareTexture == null) { _squareTexture = new Texture2D(Game.GraphicsDevice, 1, 1); _squareTexture.SetData(new[] { Color.White }); }

        LoadBackgroundMetadata();
        InitializeUI();
    }

    private void LoadAllFarmTypes()
    {
        var cfg = _layout?.FarmTypeSelector;
        if (cfg?.Options == null) return;

        _farmTypeTextures.Clear();
        _farmTypeFramesList.Clear();

        foreach (var opt in cfg.Options) {
            try {
                _farmTypeTextures.Add(Game.Content.Load<Texture2D>(opt.Texture));
                _farmTypeFramesList.Add(LoadFarmTypeMetadata(opt.Texture));
            } catch (Exception ex) {
                Console.WriteLine($"[Error] Failed to load farm type texture: {opt.Texture}. {ex.Message}");
                _farmTypeTextures.Add(null);
                _farmTypeFramesList.Add(new List<BackgroundFrame>());
            }
        }
    }

    private List<BackgroundFrame> LoadFarmTypeMetadata(string texturePath)
    {
        var frames = new List<BackgroundFrame>();
        try {
            string jsonPath = Path.Combine(Game.Content.RootDirectory, texturePath + ".json");
            if (File.Exists(jsonPath)) {
                string json = File.ReadAllText(jsonPath);
                var data = JsonConvert.DeserializeObject<dynamic>(json);
                foreach (var frame in data.frames) {
                    var f = frame.Value.frame;
                    frames.Add(new BackgroundFrame { SourceRect = new Rectangle((int)f.x, (int)f.y, (int)f.w, (int)f.h), Duration = (int)frame.Value.duration });
                }
            }
        } catch { }
        return frames;
    }

    private void LoadBackgroundMetadata()
    {
        try {
            string jsonPath = Path.Combine(Game.Content.RootDirectory, "Graphics/Screens/Common/no_title_bg.json");
            if (File.Exists(jsonPath)) {
                string jsonString = File.ReadAllText(jsonPath);
                using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                    JsonElement frames = doc.RootElement.GetProperty("frames");
                    if (frames.ValueKind == JsonValueKind.Array) foreach (JsonElement f in frames.EnumerateArray()) _bgFrames.Add(ParseFrame(f));
                    else if (frames.ValueKind == JsonValueKind.Object) foreach (JsonProperty p in frames.EnumerateObject()) _bgFrames.Add(ParseFrame(p.Value));
                }
            }
        } catch { }
    }

    private BackgroundFrame ParseFrame(JsonElement f) { return new BackgroundFrame { SourceRect = new Rectangle(f.GetProperty("frame").GetProperty("x").GetInt32(), f.GetProperty("frame").GetProperty("y").GetInt32(), f.GetProperty("frame").GetProperty("w").GetInt32(), f.GetProperty("frame").GetProperty("h").GetInt32()), Duration = f.GetProperty("duration").GetInt32() }; }

    private void InitializeUI()
    {
        var loc = LocalizationService.Instance;
        _okButton = CreateButton(loc.Get("common.ok").ToLower(), _layout.Buttons["Ok"], Color.SaddleBrown);
        _backButton = CreateButton(loc.Get("common.back").ToLower(), _layout.Buttons["Back"], Color.SaddleBrown);
        
        _nameInput = new TextInput(_font, loc.Get("charactercreation.name"), Vector2.Zero, 260, 44, _inputTexture);
        _farmNameInput = new TextInput(_font, loc.Get("charactercreation.farmname"), Vector2.Zero, 260, 44, _inputTexture);
        
        InitializeAdvantageSelect();
        InitializeDatePicker();
        
        _framePrevBtn = new Button(_font, "<", Vector2.Zero, Color.White);
        _frameNextBtn = new Button(_font, ">", Vector2.Zero, Color.White);
        
        _framePrevBtn.OnClick += () => {
            if (_categories.Count == 0) return;
            if (_activeCategoryIndex < 0) _activeCategoryIndex = _categories.Count - 1;
            else _activeCategoryIndex = (_activeCategoryIndex - 1 + _categories.Count) % _categories.Count;
            RefreshUI();
        };
        
        _frameNextBtn.OnClick += () => {
            if (_categories.Count == 0) return;
            if (_activeCategoryIndex < 0) _activeCategoryIndex = 0;
            else _activeCategoryIndex = (_activeCategoryIndex + 1) % _categories.Count;
            RefreshUI();
        };

        _colorPicker = new ColorPicker(Game.GraphicsDevice, 180);
        _colorPicker.OnColorChanged += (c) => {
            if (_activeCategoryIndex == 0) _creationService.HairColor = c;
            else if (_activeCategoryIndex == 1) _creationService.ShirtColor = c;
            else if (_activeCategoryIndex == 2) _creationService.PantsColor = c;
        };
        RefreshUI();
    }

    private void InitializeAdvantageSelect()
    {
        var loc = LocalizationService.Instance;
        var options = new List<Select.SelectOption>();
        var selectCfg = _layout?.IdentitySelectors?.Select1;
        if (selectCfg?.Options != null) {
            foreach (var optKey in selectCfg.Options) {
                string shortKey = optKey.Replace("charactercreation.advantage.", "");
                _advantageIcons.TryGetValue(shortKey, out Texture2D icon);
                options.Add(new Select.SelectOption(loc.Get(optKey).ToLower(), loc.Get(optKey + ".desc").ToLower(), null, icon, shortKey));
            }
        }
        _advantageSelect = new Select(_font, options, Vector2.Zero, 200, 44, _selectTexture, _selectMainTexture, loc.Get("charactercreation.advantages").ToLower(), _descFrameTexture, _selectItemTexture);
    }

    private void InitializeDatePicker()
    {
        var loc = LocalizationService.Instance;
        _datePicker = new DatePicker(_font, Vector2.Zero, _selectTexture, _selectMainTexture, loc.Get("charactercreation.birthday").ToLower(), null);
    }

    private void RefreshUI()
    {
        int vph = Game.GraphicsDevice.Viewport.Height;
        float res = vph / 720f;
        _uiScaleMatrix = Matrix.CreateScale(res, res, 1.0f);
        
        // We work in a fixed 720p height space for UI definition
        // vpw / res would give us the width in 720p space
        _virtualWidth = (int)(Game.GraphicsDevice.Viewport.Width / res);
        
        if (_layout?.Panel != null) {
            float rawW = _layout.Panel.GetWidth();
            float rawH = _layout.Panel.GetHeight();
            int pw = (int)(rawW <= 1.0f && rawW > 0 ? rawW * _virtualWidth : (rawW > 1.0f ? rawW : 800));
            int ph = (int)(rawH <= 1.0f && rawH > 0 ? rawH * 720 : (rawH > 1.0f ? rawH : 600));
            
            Vector2 pPos = CalculateElementPosition(_layout.Panel, pw, ph, Rectangle.Empty, Rectangle.Empty);
            _panelRect = new Rectangle((int)pPos.X, (int)pPos.Y, pw, ph);
        } else {
            _panelRect = new Rectangle((_virtualWidth - 800) / 2, (720 - 600) / 2, 800, 600);
        }
        
        if (_layout?.Frame != null && _frameTexture != null) {
            float s = _layout.Frame.GetScale();
            int fw = (int)(_frameTexture.Width * s), fh = (int)(_frameTexture.Height * s);
            Vector2 fPos = CalculateElementPosition(_layout.Frame, fw, fh, _panelRect, _panelRect);
            _frameRect = new Rectangle((int)fPos.X, (int)fPos.Y, fw, fh);
        } else {
            _frameRect = new Rectangle(_panelRect.X + (_panelRect.Width - 760) / 2, _panelRect.Y + (_panelRect.Height - 560) / 2, 760, 560);
        }

        UpdateIdentityUI();
        RefreshAppearanceUI();
        RefreshDatePickerUI();
        UpdateFarmTypeLayout();
        
        // Position frame navigation buttons below the frame
        if (_layout?.Frame != null) {
            float scale = _layout.Frame.GetScale();
            int btnSize = (int)(40 * scale);
            int spacing = (int)(20 * scale);
            
            Vector2 prevPos = new Vector2(_frameRect.Center.X - btnSize - spacing / 2, _frameRect.Bottom + 20 * scale);
            Vector2 nextPos = new Vector2(_frameRect.Center.X + spacing / 2, _frameRect.Bottom + 20 * scale);
            
            _framePrevBtn?.UpdateLayout(prevPos, btnSize, btnSize);
            _frameNextBtn?.UpdateLayout(nextPos, btnSize, btnSize);
        }
    }

    private void UpdateFarmTypeLayout()
    {
        var cfg = _layout?.FarmTypeSelector;
        if (cfg == null || cfg.Options == null) return;
        
        float scale = cfg.GetScale();
        var grid = cfg.Grid;
        float itemSize = (grid?.ItemSize ?? 128) * scale;
        float spacingX = (grid?.SpacingX ?? 10) * scale;
        float spacingY = (grid?.SpacingY ?? 10) * scale;
        int cols = grid?.Columns ?? 1;
        
        int rows = (int)Math.Ceiling(cfg.Options.Count / (float)cols);
        int totalW = (int)(cols * itemSize + (cols - 1) * spacingX);
        int totalH = (int)(rows * itemSize + (rows - 1) * spacingY);
        
        Vector2 basePos = CalculateElementPosition(cfg, totalW, totalH, _panelRect, _frameRect);
        
        _farmTypeRects.Clear();
        for (int i = 0; i < cfg.Options.Count; i++) {
            int r = i / cols, c = i % cols;
            _farmTypeRects.Add(new Rectangle((int)(basePos.X + c * (itemSize + spacingX)), (int)(basePos.Y + r * (itemSize + spacingY)), (int)itemSize, (int)itemSize));
        }
    }

    private void RefreshDatePickerUI()
    {
        if (_datePicker == null || _layout?.DatePicker == null) return;
        var cfg = _layout.DatePicker;
        Vector2 pos = CalculateElementPosition(cfg, (int)cfg.GetWidth(), (int)cfg.GetHeight(), _panelRect, _frameRect);
        
        if (cfg.LabelSettings != null) {
            _datePicker.FontScale = cfg.LabelSettings.GetFontScale();
            _datePicker.LabelOffsetY = (int)cfg.LabelSettings.GetOffsetY();
            _datePicker.LabelOffsetX = (int)cfg.LabelSettings.GetOffsetX();
        }

        int sW = (int)(cfg.SeasonSelector?.Width ?? 140), dW = (int)(cfg.DaySelector?.Width ?? 80);
        int sO = (int)(cfg.SeasonSelector?.OffsetX ?? 100), dO = (int)(cfg.DaySelector?.OffsetX ?? 220);
        _datePicker.UpdateLayout(pos, sW, dW, sO, dO);
        _datePicker.SetSeasonSelectorStyle(_selectMainTexture, null);
        _datePicker.SetDaySelectorStyle(_selectMainTexture, null);
        if (cfg.SeasonSelector != null) {
            _datePicker.SetSelectorIconStyle(true, cfg.SeasonSelector.IconOffsetX, cfg.SeasonSelector.IconOffsetY, cfg.SeasonSelector.IconScale, cfg.SeasonSelector.IconPadding);
            _datePicker.SetSelectorArrowStyle(true, cfg.SeasonSelector.ArrowOffsetX, cfg.SeasonSelector.ArrowOffsetY, cfg.SeasonSelector.ArrowScale);
        }
        if (cfg.DaySelector != null) {
            _datePicker.SetSelectorIconStyle(false, cfg.DaySelector.IconOffsetX, cfg.DaySelector.IconOffsetY, cfg.DaySelector.IconScale, cfg.DaySelector.IconPadding);
            _datePicker.SetSelectorArrowStyle(false, cfg.DaySelector.ArrowOffsetX, cfg.DaySelector.ArrowOffsetY, cfg.DaySelector.ArrowScale);
        }
    }

    private void UpdateIdentityUI()
    {
        var id = _layout.IdentitySelectors;
        UpdateSelectorUI(_nameInput, id.Input1);
        UpdateSelectorUI(_farmNameInput, id.Input2);
        UpdateDropdownUI(_advantageSelect, id.Select1);
    }

    private void UpdateSelectorUI(TextInput input, CharacterCreationLayoutData.UIElementConfig cfg) { if (input == null || cfg == null) return; Vector2 pos = CalculateElementPosition(cfg, (int)cfg.GetWidth(), (int)cfg.GetHeight(), _panelRect, _frameRect); input.UpdateLayout(pos, (int)cfg.GetWidth(), (int)cfg.GetHeight()); }
    private void UpdateDropdownUI(Select dropdown, CharacterCreationLayoutData.UIElementConfig cfg) { 
        if (dropdown == null || cfg == null) return; 
        Vector2 pos = CalculateElementPosition(cfg, (int)cfg.GetWidth(), (int)cfg.GetHeight(), _panelRect, _frameRect); 
        dropdown.UpdateLayout(pos, (int)cfg.GetWidth(), (int)cfg.GetHeight()); 
        if (cfg.DescriptionSettings != null) {
            var ds = cfg.DescriptionSettings;
            dropdown.DescIconX = ds.IconX; dropdown.DescIconY = ds.IconY; dropdown.DescIconScale = ds.IconScale;
            dropdown.DescTitleX = ds.TitleX; dropdown.DescTitleY = ds.TitleY; dropdown.DescTitleScale = ds.TitleScale;
            dropdown.DescContentX = ds.ContentX; dropdown.DescContentY = ds.ContentY; dropdown.DescContentScale = ds.ContentScale;
            dropdown.DescFrameWidth = ds.FrameWidth; dropdown.DescFrameHeight = ds.FrameHeight;
        }
    }

    private void RefreshAppearanceUI() {
        var loc = LocalizationService.Instance;
        _categories.Clear();
        _categories.Add(new CategoryUI(loc.Get("charactercreation.hair").ToLower(), _layout.AppearanceSelectors.Hair, _font, _squareTexture, "Hair"));
        _categories.Add(new CategoryUI(loc.Get("charactercreation.shirt").ToLower(), _layout.AppearanceSelectors.Shirt, _font, _squareTexture, "Shirt"));
        _categories.Add(new CategoryUI(loc.Get("charactercreation.pants").ToLower(), _layout.AppearanceSelectors.Pants, _font, _squareTexture, "Pants"));
        for (int i = 0; i < _categories.Count; i++) {
            var cat = _categories[i]; int index = i; float s = cat.Config.GetScale();
            Vector2 basePos = CalculateElementPosition(cat.Config, (int)(200 * s), (int)(80 * s), _panelRect, _frameRect);
            cat.UpdateLayout(basePos, s);
            cat.PrevBtn.OnClick += () => { if (cat.InternalKey == "Hair") _creationService.HairIndex = (_creationService.HairIndex + _creationService.MaxHairStyles - 1) % _creationService.MaxHairStyles; else if (cat.InternalKey == "Shirt") _creationService.ShirtIndex = (_creationService.ShirtIndex + _creationService.MaxShirtStyles - 1) % _creationService.MaxShirtStyles; else _creationService.PantsIndex = (_creationService.PantsIndex + _creationService.MaxPantsStyles - 1) % _creationService.MaxPantsStyles; };
            cat.NextBtn.OnClick += () => { if (cat.InternalKey == "Hair") _creationService.HairIndex = (_creationService.HairIndex + 1) % _creationService.MaxHairStyles; else if (cat.InternalKey == "Shirt") _creationService.ShirtIndex = (_creationService.ShirtIndex + 1) % _creationService.MaxShirtStyles; else _creationService.PantsIndex = (_creationService.PantsIndex + 1) % _creationService.MaxPantsStyles; };
            cat.OnIconClick += () => { _activeCategoryIndex = (_activeCategoryIndex == index) ? -1 : index; };
        }
    }

    private Button CreateButton(string text, CharacterCreationLayoutData.ElementConfig cfg, Color bc) {
        Vector2 size = _font.MeasureString(text); int w = (int)size.X + 40, h = (int)size.Y + 20;
        Vector2 pos = CalculateElementPosition(cfg, w, h, _panelRect, _frameRect);
        var b = new Button(_font, text, pos, bc, Color.LightGray, Color.DarkGray, w, h);
        if (text.Contains("ok")) b.OnClick += ConfirmCharacter;
        else if (text.Contains("back")) b.OnClick += () => { ScreenManager.RemoveScreen(this); ScreenManager.AddScreen(new BeginningScreen(Game, ScreenManager)); };
        return b;
    }

    private Vector2 CalculateElementPosition(CharacterCreationLayoutData.ElementConfig cfg, int w, int h, Rectangle p, Rectangle f) {
        float bx = cfg.RelativeTo == "Panel" ? p.X + p.Width * cfg.GetX() : (cfg.RelativeTo == "Frame" ? f.X + f.Width * cfg.GetX() : _virtualWidth * cfg.GetX());
        float by = cfg.RelativeTo == "Panel" ? p.Y + p.Height * cfg.GetY() : (cfg.RelativeTo == "Frame" ? f.Y + f.Height * cfg.GetY() : 720 * cfg.GetY());
        return new Vector2(bx - w * cfg.GetOriginX() + cfg.GetPaddingX(), by - h * cfg.GetOriginY() + cfg.GetPaddingY());
    }

    public override void Update(GameTime gameTime) {
        MouseState mouse = Mouse.GetState();
        int vpW = Game.GraphicsDevice.Viewport.Width, vpH = Game.GraphicsDevice.Viewport.Height;
        if (vpW != _lastViewportWidth || vpH != _lastViewportHeight) { _lastViewportWidth = vpW; _lastViewportHeight = vpH; RefreshUI(); }

        var mousePos = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(_uiScaleMatrix));
        var mousePoint = new Point((int)mousePos.X, (int)mousePos.Y);

        if (_activeCategoryIndex >= 0 && _activeCategoryIndex < _categories.Count) {
            var activeCat = _categories[_activeCategoryIndex];
            _colorPicker.Update(gameTime, activeCat.IconRect, activeCat.Config);
            if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released) { if (!_colorPicker.BoundingBox.Contains(mousePoint) && !activeCat.IconRect.Contains(mousePoint)) _activeCategoryIndex = -1; }
        }

        if (_bgFrames.Count > 0)
        {
            _bgFrameTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_bgFrameTimer >= _bgFrames[_currentBgFrame].Duration)
            {
                _bgFrameTimer -= _bgFrames[_currentBgFrame].Duration;
                _currentBgFrame = (_currentBgFrame + 1) % _bgFrames.Count;
            }
        }

        if (_farmTypeFramesList.Count > 0 && _selectedFarmTypeIndex < _farmTypeFramesList.Count)
        {
            var frames = _farmTypeFramesList[_selectedFarmTypeIndex];
            if (frames.Count > 0) {
                _farmTypeFrameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_farmTypeFrameTimer >= frames[_currentFarmTypeFrame].Duration)
                {
                    _farmTypeFrameTimer -= frames[_currentFarmTypeFrame].Duration;
                    _currentFarmTypeFrame = (_currentFarmTypeFrame + 1) % frames.Count;
                }
            }
        }
        
        _hoveredFarmTypeIndex = -1;
        for (int i = 0; i < _farmTypeRects.Count; i++) {
            if (_farmTypeRects[i].Contains(mousePoint)) {
                _hoveredFarmTypeIndex = i;
                if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released) {
                    _selectedFarmTypeIndex = i;
                    _currentFarmTypeFrame = 0;
                    _farmTypeFrameTimer = 0;
                }
                break;
            }
        }

        bool isModalActive = _activeCategoryIndex >= 0 && _activeCategoryIndex < _categories.Count;
        if (!isModalActive || !_colorPicker.BoundingBox.Contains(mousePoint)) { 
            _okButton?.Update(gameTime, mousePoint); _backButton?.Update(gameTime, mousePoint); _nameInput?.Update(gameTime, mousePoint); _farmNameInput?.Update(gameTime, mousePoint); _advantageSelect?.Update(gameTime, mousePoint); _datePicker?.Update(gameTime, mousePoint);
            _framePrevBtn?.Update(gameTime, mousePoint); _frameNextBtn?.Update(gameTime, mousePoint);
            foreach (var cat in _categories) cat.Update(mousePoint, mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released); 
        }
        _prevMouse = mouse;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _uiScaleMatrix);
        if (_bgTexture != null) {
            var fullRect = new Rectangle(0, 0, _virtualWidth, 720);
            if (_bgFrames.Count > 0) spriteBatch.Draw(_bgTexture, fullRect, _bgFrames[_currentBgFrame].SourceRect, Color.White);
            else spriteBatch.Draw(_bgTexture, fullRect, Color.White);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _uiScaleMatrix);
        spriteBatch.Draw(_panelTexture, _panelRect, Color.White);
        spriteBatch.Draw(_frameTexture, _frameRect, Color.White);
        _okButton?.Draw(spriteBatch); _backButton?.Draw(spriteBatch); 
        _nameInput?.Draw(spriteBatch, _uiScaleMatrix); 
        _farmNameInput?.Draw(spriteBatch, _uiScaleMatrix); 
        _advantageSelect?.Draw(spriteBatch, _uiScaleMatrix, _virtualWidth); 
        _datePicker?.Draw(spriteBatch);
        _framePrevBtn?.Draw(spriteBatch);
        _frameNextBtn?.Draw(spriteBatch);
        DrawFarmTypeSelector(spriteBatch);
        foreach (var cat in _categories) cat.Draw(spriteBatch, _creationService);
        if (_activeCategoryIndex >= 0 && _activeCategoryIndex < _categories.Count) _colorPicker.Draw(spriteBatch, _categories[_activeCategoryIndex].Config);
        spriteBatch.End();
    }

    private void DrawFarmTypeSelector(SpriteBatch sb)
    {
        var cfg = _layout?.FarmTypeSelector;
        if (cfg == null || _descFrameTexture == null || cfg.Options == null) return;

        var loc = LocalizationService.Instance;
        float scale = cfg.GetScale();
        var ds = cfg.DescriptionSettings;

        for (int i = 0; i < _farmTypeRects.Count; i++) {
            var rect = _farmTypeRects[i];
            var texture = i < _farmTypeTextures.Count ? _farmTypeTextures[i] : null;
            var frames = i < _farmTypeFramesList.Count ? _farmTypeFramesList[i] : null;
            
            if (texture != null) {
                if (frames != null && frames.Count > 0) {
                    int frameIdx = (i == _selectedFarmTypeIndex) ? _currentFarmTypeFrame : 0;
                    sb.Draw(texture, rect, frames[frameIdx].SourceRect, Color.White);
                } else sb.Draw(texture, rect, Color.White);
            }

            // Draw selection border
            if (i == _selectedFarmTypeIndex) UIUtils.DrawRectangle(sb, _squareTexture, rect, Color.SaddleBrown, 2);
            else if (i == _hoveredFarmTypeIndex) UIUtils.DrawRectangle(sb, _squareTexture, rect, Color.Gray, 1);

            // Draw type name under each icon
            string typeName = loc.Get(cfg.Options[i].TypeName).ToLower();
            float typeNameScale = 0.8f * scale;
            Vector2 typeNameSize = _font.MeasureString(typeName) * typeNameScale;
            sb.DrawString(_font, typeName, new Vector2(rect.Center.X - typeNameSize.X / 2, rect.Bottom + 5 * scale), Color.Black, 0f, Vector2.Zero, typeNameScale, SpriteEffects.None, 0f);
        }

        if (_hoveredFarmTypeIndex >= 0 && _hoveredFarmTypeIndex < cfg.Options.Count)
        {
            var opt = cfg.Options[_hoveredFarmTypeIndex];
            var hoveredRect = _farmTypeRects[_hoveredFarmTypeIndex];
            
            int x = hoveredRect.Left - 10 - (int)(ds.FrameWidth * scale);
            Rectangle bounds = new Rectangle(x, hoveredRect.Y, (int)(ds.FrameWidth * scale), (int)(ds.FrameHeight * scale));
            sb.Draw(_descFrameTexture, bounds, Color.White);

            string title = loc.Get(opt.TypeName);
            string desc = loc.Get(opt.Description);

            UIUtils.DrawStringWithManualSpaces(sb, _font, title, new Vector2(bounds.X + ds.TitleX * scale, bounds.Y + ds.TitleY * scale), Color.SaddleBrown, ds.TitleScale * scale);
            
            if (!string.IsNullOrEmpty(desc))
            {
                float wrapWidth = bounds.Width - (ds.ContentX * 2 * scale);
                DrawWrappedString(sb, _font, desc, new Vector2(bounds.X + ds.ContentX * scale, bounds.Y + ds.ContentY * scale), Color.Black, ds.ContentScale * scale, wrapWidth);
            }
        }
    }

    private void DrawWrappedString(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, float scale, float maxWidth)
    {
        string[] words = text.Split(' '); Vector2 currentPos = pos; float spaceWidth = UIUtils.DefaultSpaceWidth * scale;
        foreach (var word in words) { float wordWidth = font.MeasureString(word).X * scale; if (currentPos.X + wordWidth > pos.X + maxWidth) { currentPos.X = pos.X; currentPos.Y += font.LineSpacing * scale; } UIUtils.DrawStringWithManualSpaces(sb, font, word, currentPos, color, scale); currentPos.X += wordWidth + spaceWidth; }
    }

    private void ConfirmCharacter() {
        _creationService.Name = _nameInput.Text; _creationService.FarmName = _farmNameInput.Text;
        _creationService.BirthdaySeason = _datePicker.SelectedSeason; _creationService.BirthdayDay = _datePicker.SelectedDay;
        Console.WriteLine($"Character Created: {_creationService.Name} from {_creationService.FarmName} farm.");
    }

    private class CategoryUI {
        public string Name;
        public CharacterCreationLayoutData.UIElementConfig Config;
        public Rectangle IconRect;
        public Button PrevBtn, NextBtn;
        public Action OnIconClick;
        private SpriteFont _font;
        private Texture2D _sq;
        private Vector2 _basePos;
        public string InternalKey;

        public CategoryUI(string name, CharacterCreationLayoutData.UIElementConfig config, SpriteFont font, Texture2D sq, string internalKey) { Name = name; Config = config; _font = font; _sq = sq; InternalKey = internalKey; PrevBtn = new Button(font, "<", Vector2.Zero, Color.White); NextBtn = new Button(font, ">", Vector2.Zero, Color.White); }
        public void UpdateLayout(Vector2 pos, float scale) {
            _basePos = pos; float s = scale; var ctrl = Config.Controls;
            float btnSize = ctrl.GetButtonSize() * s, spacing = ctrl.GetSpacingX();
            IconRect = new Rectangle((int)pos.X, (int)(pos.Y + ctrl.GetRowOffsetY()), (int)btnSize, (int)btnSize);
            PrevBtn.UpdateLayout(new Vector2(IconRect.Right + ctrl.GetColorIconOffsetX(), IconRect.Y), (int)btnSize, (int)btnSize);
            NextBtn.UpdateLayout(new Vector2(PrevBtn.Bounds.Right + spacing, IconRect.Y), (int)btnSize, (int)btnSize);
        }
        public void Update(Point mousePt, bool leftPressed) { PrevBtn.Update(default, mousePt); NextBtn.Update(default, mousePt); if (leftPressed && IconRect.Contains(mousePt)) OnIconClick?.Invoke(); }
        public void Draw(SpriteBatch sb, ICharacterCreationService svc) {
            sb.DrawString(_font, Name, _basePos, Color.Black);
            Color c = InternalKey == "Hair" ? svc.HairColor : (InternalKey == "Shirt" ? svc.ShirtColor : svc.PantsColor);
            sb.Draw(_sq, IconRect, c); PrevBtn.Draw(sb); NextBtn.Draw(sb);
        }
    }

    private class BackgroundFrame { public Rectangle SourceRect; public int Duration; }
}
