using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Domain;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Repositories.Storage;
using Code_Game.Scripts.UI;
using Code_Game.Scripts.Screens.Beginning;
using Code_Game.Scripts.Screens.Gameplay;
using Code_Game.Scripts.Services.Localization;
using Code_Game.Scripts.Constants.LocalizationKeys;
using Code_Game.Scripts.Constants.Paths;
using System.IO;
using System.Text.Json;

namespace Code_Game.Scripts.Screens.LoadGame;

public class LoadGameScreen : Screen
{
    private SpriteFont _font;
    private readonly List<PlayerProfile> _profiles;
    private readonly List<Button> _saveButtons = new();
    private Button _backButton;
    private readonly IPlayerProfileRepository _profileRepository;
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
    private Texture2D _backgroundTexture;
    private Matrix _uiScaleMatrix = Matrix.Identity;
    private Point _lastViewportSize;

    public LoadGameScreen(Game game, ScreenManager screenManager) : base(game, screenManager)
    {
        _profileRepository = new PlayerProfileRepository();
        _profiles = _profileRepository.LoadAll().ToList();
    }

    private void BackToMenu()
    {
        ScreenManager.AddScreen(new BeginningScreen(Game, ScreenManager));
        ScreenManager.RemoveScreen(this);
    }

    private void LoadProfile(PlayerProfile profile)
    {
        ScreenManager.AddScreen(new GameplayScreen(Game, ScreenManager, profile));
        ScreenManager.RemoveScreen(this);
    }

    public override void LoadContent()
    {
        try
        {
            _font = Game.Content.Load<SpriteFont>(ContentPaths.Fonts.DefaultFont);
        }
        catch
        {
            _font = null;
        }

        // Load background texture
        try
        {
            _backgroundTexture = Game.Content.Load<Texture2D>(FolderNames.CommonBgPath + ContentPaths.Screens.NoTitleBackground);
            
            var jsonPath = Path.Combine(Game.Content.RootDirectory, FolderNames.CommonBgPath + ContentPaths.Screens.NoTitleBackgroundJson);
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
            Console.WriteLine($"Error loading shared background: {ex.Message}");
        }

        if (_font != null)
        {
            _title = LocalizationService.Instance.Get(LoadGameKeys.Title);
            _subtitle = LocalizationService.Instance.Get(LoadGameKeys.Subtitle);
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        var viewport = Game.GraphicsDevice.Viewport;
        float res = viewport.Height / 720f;
        _uiScaleMatrix = Matrix.CreateScale(res, res, 1.0f);
        _lastViewportSize = new Point(viewport.Width, viewport.Height);
        int virtualWidth = (int)(viewport.Width / res);
        int centerX = virtualWidth / 2;
        int startY = 250;

        _saveButtons.Clear();
        for (int i = 0; i < _profiles.Count; i++)
        {
            var profile = _profiles[i];
            var buttonText = $"{profile.FarmName} - {profile.Name}";
            var button = new Button(_font, buttonText, new Vector2(centerX - 150, startY + i * 60), width: 300, height: 50);
            button.OnClick += () => LoadProfile(profile);
            _saveButtons.Add(button);
        }

        var backY = 650;
        _backButton = new Button(_font, LocalizationService.Instance.Get(CommonKeys.Back), new Vector2(centerX - 75, backY), width: 150, height: 44);
        _backButton.OnClick += BackToMenu;
    }

    public override void Update(GameTime gameTime)
    {
        var viewport = Game.GraphicsDevice.Viewport;
        if (viewport.Width != _lastViewportSize.X || viewport.Height != _lastViewportSize.Y) InitializeUI();

        float res = viewport.Height / 720f;
        var mouseState = Mouse.GetState();
        var mousePos = Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(Matrix.CreateScale(res, res, 1.0f)));
        var mousePoint = new Point((int)mousePos.X, (int)mousePos.Y);

        foreach (var button in _saveButtons)
        {
            button.Update(gameTime, mousePoint);
        }

        _backButton?.Update(gameTime, mousePoint);

        // Update background animation
        if (_bgFrames.Count > 0)
        {
            _bgFrameTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_bgFrameTimer >= _bgFrames[_currentBgFrame].Duration)
            {
                _bgFrameTimer = 0;
                _currentBgFrame = (_currentBgFrame + 1) % _bgFrames.Count;
            }
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            BackToMenu();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _uiScaleMatrix);

        var viewport = Game.GraphicsDevice.Viewport;
        float res = viewport.Height / 720f;
        int virtualWidth = (int)(viewport.Width / res);
        var fullRect = new Rectangle(0, 0, virtualWidth, 720);

        // Draw background
        if (_backgroundTexture != null && _bgFrames.Count > 0)
        {
            spriteBatch.Draw(_backgroundTexture, 
                fullRect, 
                _bgFrames[_currentBgFrame].SourceRect, 
                Color.White);
        }
        else if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, fullRect, Color.White);
        }
        else
        {
            // Just a fallback color
            sbDrawWhite(spriteBatch, fullRect, new Color(30, 30, 60));
        }

        if (_font != null)
        {
            var centerX = virtualWidth / 2;
            var titleSize = _font.MeasureString(_title);
            var subtitleSize = _font.MeasureString(_subtitle);
            spriteBatch.DrawString(_font, _title, new Vector2(centerX - titleSize.X / 2, 80), Color.White);
            spriteBatch.DrawString(_font, _subtitle, new Vector2(centerX - subtitleSize.X / 2, 120), Color.LightGray);

            if (_profiles.Any())
            {
                // Profile list is now handled by buttons mostly, but we can draw some text if we want
            }
            else
            {
                var noSavesText = LocalizationService.Instance.Get(LoadGameKeys.NoSaves);
                var noSavesSize = _font.MeasureString(noSavesText);
                spriteBatch.DrawString(_font, noSavesText, new Vector2(centerX - noSavesSize.X / 2, 220), Color.Orange);
            }

            foreach (var button in _saveButtons)
            {
                button.Draw(spriteBatch);
            }

            _backButton?.Draw(spriteBatch);
        }

        spriteBatch.End();
    }

    private void sbDrawWhite(SpriteBatch sb, Rectangle rect, Color color)
    {
        Texture2D pixel = new Texture2D(sb.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        sb.Draw(pixel, rect, color);
    }
}
