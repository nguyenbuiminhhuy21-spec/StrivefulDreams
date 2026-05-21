using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Code_Game.Scripts.Constants;
using Code_Game.Scripts.Constants.LocalizationKeys;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Services.Localization;
using Code_Game.Scripts.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code_Game.Scripts.Screens.Beginning;

public class BeginningScreen : Screen
{
    private Texture2D _backgroundTexture;
    private Texture2D _newGameTexture;
    private Texture2D _loadGameTexture;
    private Texture2D _coopTexture;
    private Texture2D _exitTexture;
    private Texture2D _settingTexture;
    private Texture2D _titleTexture;
    private SpriteFont _font;

    private Button _newGameButton;
    private Button _loadGameButton;
    private Button _coopButton;
    private Button _exitButton;
    private Button _settingButton;

    private List<BackgroundFrame> _bgFrames = new();
    private int _currentBgFrame = 0;
    private float _bgFrameTimer = 0;

    private Point _lastViewportSize;
    private Matrix _uiScaleMatrix = Matrix.Identity;
    private BeginningLayoutData _layout;

    public BeginningScreen(Game game, ScreenManager screenManager) : base(game, screenManager)
    {
    }

    public override void LoadContent()
    {
        try {
            _backgroundTexture = Game.Content.Load<Texture2D>("Graphics/Screens/Beginning/beginning_notitle_bg");
            _titleTexture = Game.Content.Load<Texture2D>("Graphics/Screens/Beginning/title_cloudy");
            _settingTexture = Game.Content.Load<Texture2D>("Graphics/Buttons/setting");
            _font = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");
            
            LoadLocalizedButtons();
        } catch (Exception ex) {
            Console.WriteLine($"[Error] Failed to load BeginningScreen content: {ex.Message}");
        }

        LoadBackgroundMetadata();
        LoadLayout();
        InitializeButtons();
    }

    private void LoadLocalizedButtons()
    {
        string locale = LocalizationService.Instance.CurrentLocale;
        _newGameTexture = Game.Content.Load<Texture2D>($"Graphics/Buttons/{locale}/new");
        _loadGameTexture = Game.Content.Load<Texture2D>($"Graphics/Buttons/{locale}/load");
        _coopTexture = Game.Content.Load<Texture2D>($"Graphics/Buttons/{locale}/coop");
        _exitTexture = Game.Content.Load<Texture2D>($"Graphics/Buttons/{locale}/exit");
    }

    private void LoadLayout()
    {
        try {
            string path = Path.Combine(Game.Content.RootDirectory, "Data/Screens/BeginningLayout.json");
            if (File.Exists(path)) {
                string json = File.ReadAllText(path);
                _layout = JsonSerializer.Deserialize<BeginningLayoutData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        } catch { }
        if (_layout == null) _layout = new BeginningLayoutData();
    }

    private void InitializeButtons()
    {
        var viewport = Game.GraphicsDevice.Viewport;
        float res = viewport.Height / 720f;
        _uiScaleMatrix = Matrix.CreateScale(res, res, 1.0f);
        _lastViewportSize = new Point(viewport.Width, viewport.Height);
        
        int virtualWidth = (int)(viewport.Width / res);

        var menuCfg = _layout.MainMenu;
        float btnScale = (menuCfg?.Scale ?? 1.2f);
        int uniformWidth = (int)(70 * btnScale);
        int uniformHeight = (int)(52 * btnScale);
        
        var buttonY = (int)(720 * (menuCfg?.Y ?? 0.85f) - (uniformHeight / 2));
        var buttonGap = (int)(menuCfg?.Gap ?? 20);
        var totalWidth = (uniformWidth * 4) + (buttonGap * 3);
        var groupStartX = (virtualWidth - totalWidth) / 2;

        _newGameButton = CreateButton(_newGameTexture, new Vector2(groupStartX, buttonY), NewGame, uniformWidth, uniformHeight);
        _loadGameButton = CreateButton(_loadGameTexture, new Vector2(groupStartX + uniformWidth + buttonGap, buttonY), LoadGame, uniformWidth, uniformHeight);
        _coopButton = CreateButton(_coopTexture, new Vector2(groupStartX + (uniformWidth + buttonGap) * 2, buttonY), CoopGame, uniformWidth, uniformHeight);
        _exitButton = CreateButton(_exitTexture, new Vector2(groupStartX + (uniformWidth + buttonGap) * 3, buttonY), ExitGame, uniformWidth, uniformHeight);

        var settingCfg = _layout.SettingsButton;
        int settingSize = (int)(40 * (settingCfg?.Scale ?? 1.0f));
        var settingX = virtualWidth * (settingCfg?.X ?? 0.95f);
        var settingY = 720 * (settingCfg?.Y ?? 0.05f);
        _settingButton = new Button(_settingTexture, new Vector2(settingX, settingY), settingSize, settingSize);
        _settingButton.OnClick += OpenSettings;

    }

    private Button CreateButton(Texture2D texture, Vector2 position, Action onClick, int w, int h)
    {
        if (texture == null) return null;
        var button = new Button(texture, position, w, h);
        button.OnClick += onClick;
        return button;
    }

    private void NewGame() => ScreenManager.AddScreen(new Code_Game.Scripts.Screens.CharacterCreation.CharacterCreationScreen(Game, ScreenManager));
    private void LoadGame() => Console.WriteLine("[BeginningScreen] Load Game Clicked");
    private void CoopGame() => Console.WriteLine("[BeginningScreen] Coop Game Clicked");
    private void ExitGame() => Game.Exit();
    private void OpenSettings() => ScreenManager.AddScreen(
        new Code_Game.Scripts.Screens.Settings.SettingScreen(Game, ScreenManager, () => {
            LoadLocalizedButtons();
            InitializeButtons();
        })
    );

    public override void Update(GameTime gameTime)
    {
        var viewport = Game.GraphicsDevice.Viewport;
        if (viewport.Width != _lastViewportSize.X || viewport.Height != _lastViewportSize.Y) InitializeButtons();

        float res = viewport.Height / 720f;
        var mouseState = Mouse.GetState();
        var mousePos = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(Matrix.CreateScale(res, res, 1.0f)));
        var mousePoint = new Point((int)mousePos.X, (int)mousePos.Y);

        if (_bgFrames.Count > 0)
        {
            _bgFrameTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_bgFrameTimer >= _bgFrames[_currentBgFrame].Duration)
            {
                _bgFrameTimer -= _bgFrames[_currentBgFrame].Duration;
                _currentBgFrame = (_currentBgFrame + 1) % _bgFrames.Count;
            }
        }

        _newGameButton?.Update(gameTime, mousePoint);
        _loadGameButton?.Update(gameTime, mousePoint);
        _coopButton?.Update(gameTime, mousePoint);
        _exitButton?.Update(gameTime, mousePoint);
        _settingButton?.Update(gameTime, mousePoint);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _uiScaleMatrix);
        var viewport = Game.GraphicsDevice.Viewport;
        float res = viewport.Height / 720f;
        int virtualWidth = (int)(viewport.Width / res);
        var fullRect = new Rectangle(0, 0, virtualWidth, 720);

        if (_backgroundTexture != null)
        {
            if (_bgFrames.Count > 0) spriteBatch.Draw(_backgroundTexture, fullRect, _bgFrames[_currentBgFrame].SourceRect, Color.White);
            else spriteBatch.Draw(_backgroundTexture, fullRect, Color.White);
        }

        if (_titleTexture != null)
        {
            var titleCfg = _layout.TitleImage;
            int titleW = (int)(_titleTexture.Width * titleCfg.Scale);
            int titleH = (int)(_titleTexture.Height * titleCfg.Scale);
            int titleX = (int)(virtualWidth * titleCfg.X) - titleW / 2;
            int titleY = (int)(720 * titleCfg.Y);
            spriteBatch.Draw(_titleTexture, new Rectangle(titleX, titleY, titleW, titleH), Color.White);
        }

        _newGameButton?.Draw(spriteBatch);
        _loadGameButton?.Draw(spriteBatch);
        _coopButton?.Draw(spriteBatch);
        _exitButton?.Draw(spriteBatch);
        _settingButton?.Draw(spriteBatch);

        spriteBatch.End();
    }

    private void LoadBackgroundMetadata()
    {
        try {
            string jsonPath = Path.Combine(Game.Content.RootDirectory, "Graphics/Screens/Beginning/beginning_notitle_bg.json");
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

    private class BackgroundFrame { public Rectangle SourceRect; public int Duration; }
    public class BeginningLayoutData {
        public ElementConfig SettingsButton { get; set; } = new ElementConfig();
        public MenuConfig MainMenu { get; set; } = new MenuConfig();
        public ElementConfig TitleImage { get; set; } = new ElementConfig { X = 0.5f, Y = 0.08f, Scale = 2.0f };

        public class ElementConfig { public float X { get; set; } = 0.88f; public float Y { get; set; } = 0.05f; public float Scale { get; set; } = 1.0f; }
        public class MenuConfig { public float Y { get; set; } = 0.85f; public float Scale { get; set; } = 1.2f; public float Gap { get; set; } = 20f; }
    }
}