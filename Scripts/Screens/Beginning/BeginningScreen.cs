using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Screens.CharacterCreation;
using Code_Game.Scripts.Screens.Gameplay;
using Code_Game.Scripts.Screens.LoadGame;
using Code_Game.Scripts.UI;
using Code_Game.Scripts.Repositories.Storage;
using Code_Game.Domain;
using Code_Game.Scripts.Services.Localization;
using System.Linq;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Code_Game.Scripts.Screens.Beginning;

public class BeginningScreen : Screen
{
    private SpriteFont _font;
    private Button _startButton;
    private Button _loadButton;
    private Button _coopButton;
    private Button _exitButton;
    private Texture2D _backgroundTexture;
    private Point _lastViewportSize; // Track window size
    private IPlayerProfileRepository _profileRepository;
    private string _title = "";
    private string _subtitle = "";

    private struct FrameData
    {
        public Rectangle SourceRect;
        public int Duration;
    }

    private List<FrameData> _bgFrames = new List<FrameData>();
    private int _currentBgFrame = 0;
    private float _bgFrameTimer = 0f;

    public BeginningScreen(Game game, ScreenManager screenManager) : base(game, screenManager)
    {
        _profileRepository = new PlayerProfileRepository();
    }

    private void StartGame()
    {
        ScreenManager.AddScreen(new CharacterCreationScreen(Game, ScreenManager));
        ScreenManager.RemoveScreen(this);
    }

    private void LoadGame()
    {
        try
        {
            ScreenManager.AddScreen(new LoadGameScreen(Game, ScreenManager));
            ScreenManager.RemoveScreen(this);
        }
        catch
        {
            Console.WriteLine("Error opening load game screen.");
        }
    }

    private void CoopGame()
    {
        // TODO: Implement multiplayer/coop mode
        Console.WriteLine(LocalizationService.Instance.Get("Beginning.CoopComingSoon"));
    }

    private void ExitGame()
    {
        Game.Exit();
    }

    private Button CreateButton(string text, Vector2 position, Action onClick, int width, int? height = null)
    {
        var button = new Button(_font, text, position, width: width, height: height);
        button.OnClick += onClick;
        return button;
    }

    public override void LoadContent()
    {
        try
        {
            _font = Game.Content.Load<SpriteFont>(ContentPaths.DefaultFont);
        }
        catch
        {
            _font = null;
        }

        // Load background texture
        try
        {
            _backgroundTexture = Game.Content.Load<Texture2D>(ContentPaths.BeginningBackground);
            
            var jsonPath = Path.Combine(Game.Content.RootDirectory, ContentPaths.BeginningBackgroundJson);
            if (File.Exists(jsonPath))
            {
                var jsonString = File.ReadAllText(jsonPath);
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var framesElement = doc.RootElement.GetProperty("frames");
                    foreach (var frameProp in framesElement.EnumerateObject())
                    {
                        var frameObj = frameProp.Value;
                        var rectObj = frameObj.GetProperty("frame");
                        int x = rectObj.GetProperty("x").GetInt32();
                        int y = rectObj.GetProperty("y").GetInt32();
                        int w = rectObj.GetProperty("w").GetInt32();
                        int h = rectObj.GetProperty("h").GetInt32();
                        int duration = frameObj.GetProperty("duration").GetInt32();

                        _bgFrames.Add(new FrameData { SourceRect = new Rectangle(x, y, w, h), Duration = duration });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading background: {ex.Message}");
            _backgroundTexture = null;
        }

        if (_font != null)
        {
            _title = LocalizationService.Instance.Get("Beginning.Title");
            _subtitle = LocalizationService.Instance.Get("Beginning.Subtitle");
            InitializeButtons();
        }
    }

    private void InitializeButtons()
    {
        var viewport = Game.GraphicsDevice.Viewport;
        var centerX = viewport.Width / 2;

        // --- RESPONSIVE SETTINGS (Percentages relative to screen size) ---
        var buttonWidthPercent = 0.16f;
        var buttonHeightPercent = 0.14f;
        var gapPercent = 0.022f;
        var marginBottomPercent = 0.12f; // 8% margin from the bottom

        // Calculate actual pixel dimensions
        var uniformWidth = (int)(viewport.Width * buttonWidthPercent);
        var uniformHeight = (int)(viewport.Height * buttonHeightPercent);
        var buttonGap = (int)(viewport.Width * gapPercent);

        // --- PREVENT TEXT OVERFLOW --- 
        var lang = LocalizationService.Instance;
        var buttonTexts = new[] {
            lang.Get("Beginning.NewGame"),
            lang.Get("Beginning.LoadGame"),
            lang.Get("Beginning.Coop"),
            lang.Get("Beginning.Exit")
        };
        var pdBg = 30;
        var maxTextWidth = (int)buttonTexts.Max(text => _font.MeasureString(text).X) + pdBg;

        // --- CONSTRAINTS ---
        var minWidth = 200;
        var minHeight = 120;

        // Button width is the maximum of (screen percentage), (text length), and (minWidth)
        uniformWidth = Math.Max(uniformWidth, Math.Max(maxTextWidth, minWidth));
        uniformHeight = Math.Max(uniformHeight, minHeight);

        // --- ANCHOR TO BOTTOM ---
        // Calculate Y position AFTER final height is determined
        var buttonY = viewport.Height - uniformHeight - (int)(viewport.Height * marginBottomPercent);

        Console.WriteLine($"[Debug] Button Size: {uniformWidth}x{uniformHeight} (Screen: {viewport.Width}x{viewport.Height})");

        // Calculate layout to center the entire button group horizontally
        var totalGroupWidth = 4 * uniformWidth + 3 * buttonGap;
        var groupStartX = centerX - totalGroupWidth / 2;

        // Initialize Button objects
        _startButton = CreateButton(lang.Get("Beginning.NewGame"), new Vector2(groupStartX, buttonY), StartGame, uniformWidth, uniformHeight);
        _loadButton = CreateButton(lang.Get("Beginning.LoadGame"), new Vector2(groupStartX + (uniformWidth + buttonGap), buttonY), LoadGame, uniformWidth, uniformHeight);
        _coopButton = CreateButton(lang.Get("Beginning.Coop"), new Vector2(groupStartX + (uniformWidth + buttonGap) * 2, buttonY), CoopGame, uniformWidth, uniformHeight);
        _exitButton = CreateButton(lang.Get("Beginning.Exit"), new Vector2(groupStartX + (uniformWidth + buttonGap) * 3, buttonY), ExitGame, uniformWidth, uniformHeight);
    }

    public override void Update(GameTime gameTime)
    {

        // Check if window was resized
        var currentSize = new Point(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        if (currentSize != _lastViewportSize)
        {
            Console.WriteLine($"[Debug] Window Resized: {_lastViewportSize.X}x{_lastViewportSize.Y} -> {currentSize.X}x{currentSize.Y}");
            _lastViewportSize = currentSize;
            if (_font != null) InitializeButtons();
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

        _startButton?.Update(gameTime);
        _loadButton?.Update(gameTime);
        _coopButton?.Update(gameTime);
        _exitButton?.Update(gameTime);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_backgroundTexture == null)
        {
            Game.GraphicsDevice.Clear(Color.White);
        }

        spriteBatch.Begin();

        // 2. Draw background
        if (_backgroundTexture != null)
        {
            if (_bgFrames.Count > 0)
            {
                var sourceRect = _bgFrames[_currentBgFrame].SourceRect;
                spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), sourceRect, Color.White);
            }
            else
            {
                spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            }
        }

        // Draw title
        if (_font != null)
        {
            var titleSize = _font.MeasureString(_title);
            var centerX = Game.GraphicsDevice.Viewport.Width / 2;
            var centerY = Game.GraphicsDevice.Viewport.Height / 2;

            // Draw title at 10% above center
            spriteBatch.DrawString(_font, _title, new Vector2(centerX - titleSize.X / 2, centerY - (Game.GraphicsDevice.Viewport.Height * 0.15f)), Color.White);

            var subtitleSize = _font.MeasureString(_subtitle);
            // Draw subtitle at 5% above center
            spriteBatch.DrawString(_font, _subtitle, new Vector2(centerX - subtitleSize.X / 2, centerY - (Game.GraphicsDevice.Viewport.Height * 0.08f)), Color.Yellow);

            // Draw button
            _startButton?.Draw(spriteBatch);
            _loadButton?.Draw(spriteBatch);
            _coopButton?.Draw(spriteBatch);
            _exitButton?.Draw(spriteBatch);
        }

        spriteBatch.End();
    }
}