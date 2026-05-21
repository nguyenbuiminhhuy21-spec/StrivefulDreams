using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.UI;
using Code_Game.Domain;
using Code_Game.Scripts.Services.Localization;
using Code_Game.Scripts.Repositories.Storage;
using Code_Game.Scripts.Constants.LocalizationKeys;
using Code_Game.Scripts.Constants.Paths;

namespace Code_Game.Scripts.Screens.Gameplay;

public class GameplayScreen : Screen
{
    private SpriteFont _font;
    private Button _backButton;
    private PlayerProfile _playerProfile;
    private IPlayerProfileRepository _profileRepository;
    private Matrix _uiScaleMatrix = Matrix.Identity;
    private Point _lastViewportSize;
    private Texture2D _backgroundTexture;

    public GameplayScreen(Game game, ScreenManager screenManager, PlayerProfile playerProfile) : base(game, screenManager)
    {
        _playerProfile = playerProfile;
        _profileRepository = new PlayerProfileRepository();
    }

    private void BackToMenu()
    {
        // For now, just go back to beginning screen
        // In a real game, this would save progress first
        ScreenManager.AddScreen(new Scripts.Screens.Beginning.BeginningScreen(Game, ScreenManager));
        ScreenManager.RemoveScreen(this);
    }

    public override void LoadContent()
    {
        try {
            _font = Game.Content.Load<SpriteFont>(ContentPaths.Fonts.DefaultFont);
        } catch { _font = null; }

        try {
            _backgroundTexture = Game.Content.Load<Texture2D>(FolderNames.CommonBgPath + ContentPaths.Screens.NoTitleBackground);
        } catch { }

        if (_font != null)
        {
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

        _backButton = new Button(_font, LocalizationService.Instance.Get(GameplayKeys.BackToMenu), new Vector2(centerX - 100, centerY + 100), width: 200, height: 44);
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

        // Handle escape key to go back
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

        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, fullRect, Color.White);
        }
        else
        {
            Game.GraphicsDevice.Clear(new Color(135, 206, 235));
        }

        // Draw game content
        if (_font != null && _playerProfile != null)
        {
            var centerX = virtualWidth / 2;
            var centerY = 360;
            var lang = LocalizationService.Instance;

            // Draw welcome message
            string welcomeText = string.Format(lang.Get(GameplayKeys.Welcome), _playerProfile.FarmName, _playerProfile.Name);
            var welcomeSize = _font.MeasureString(welcomeText);
            spriteBatch.DrawString(_font, welcomeText, new Vector2(centerX - welcomeSize.X / 2, centerY - 150), Color.White);

            // Draw player info
            string infoText = string.Format(lang.Get(GameplayKeys.Info), _playerProfile.FavoriteThing, _playerProfile.AnimalPreference);
            var infoSize = _font.MeasureString(infoText);
            spriteBatch.DrawString(_font, infoText, new Vector2(centerX - infoSize.X / 2, centerY - 100), Color.Yellow);

            // Draw placeholder game elements
            string gameText = lang.Get(GameplayKeys.ComingSoon);
            var gameSize = _font.MeasureString(gameText);
            spriteBatch.DrawString(_font, gameText, new Vector2(centerX - gameSize.X / 2, centerY - 50), Color.Green);

            // Draw back button
            _backButton?.Draw(spriteBatch);
        }

        spriteBatch.End();
    }
}