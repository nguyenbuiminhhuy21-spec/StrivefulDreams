using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Screens.Beginning;
using Code_Game.Scripts.UI;
using Code_Game.Scripts.Services.Localization;
using Code_Game.Scripts.Constants.LocalizationKeys;
using Code_Game.Scripts.Constants.Paths;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System;

namespace Code_Game.Scripts.Screens.Coop;

public class CoopScreen : Screen
{
    private SpriteFont _font;
    private Button _backButton;
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

    public CoopScreen(Game game, ScreenManager screenManager) : base(game, screenManager)
    {
    }

    private void BackToMenu()
    {
        ScreenManager.AddScreen(new BeginningScreen(Game, ScreenManager));
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

        // Load shared background texture (no title version)
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
            _title = LocalizationService.Instance.Get(BeginningKeys.Coop);
            _subtitle = LocalizationService.Instance.Get(BeginningKeys.CoopComingSoon);
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
        int centerY = 360;

        _backButton = new Button(_font, LocalizationService.Instance.Get(CommonKeys.Back), new Vector2(centerX - 100, centerY + 100), width: 200, height: 44);
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
            Game.GraphicsDevice.Clear(new Color(30, 30, 60));
        }

        if (_font != null)
        {
            var centerX = virtualWidth / 2;
            var titleSize = _font.MeasureString(_title);
            var subtitleSize = _font.MeasureString(_subtitle);

            spriteBatch.DrawString(_font, _title, new Vector2(centerX - titleSize.X / 2, 100), Color.White);
            spriteBatch.DrawString(_font, _subtitle, new Vector2(centerX - subtitleSize.X / 2, 160), Color.Yellow);

            _backButton?.Draw(spriteBatch);
        }

        spriteBatch.End();
    }
}
