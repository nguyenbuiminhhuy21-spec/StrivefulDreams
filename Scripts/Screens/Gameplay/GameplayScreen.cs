using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.UI;
using Code_Game.Domain;
using Code_Game.Scripts.Services.Localization;
using Code_Game.Scripts.Repositories.Storage;

namespace Code_Game.Scripts.Screens.Gameplay;

public class GameplayScreen : Screen
{
    private SpriteFont _font;
    private Button _backButton;
    private PlayerProfile _playerProfile;
    private IPlayerProfileRepository _profileRepository;

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
        // Load a default font if available
        try
        {
            _font = Game.Content.Load<SpriteFont>(ContentPaths.DefaultFont);
        }
        catch
        {
            // Fallback: no text if font fails
            _font = null;
        }

        if (_font != null)
        {
            var centerX = Game.GraphicsDevice.Viewport.Width / 2;
            var centerY = Game.GraphicsDevice.Viewport.Height / 2;
            _backButton = new Button(_font, LocalizationService.Instance.Get("Gameplay.BackToMenu"), new Vector2(centerX - 100, centerY + 100));
            _backButton.OnClick += BackToMenu;
        }
    }

    public override void Update(GameTime gameTime)
    {
        _backButton?.Update(gameTime);

        // Handle escape key to go back
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            BackToMenu();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Clear to a nice game background color
        Game.GraphicsDevice.Clear(new Color(135, 206, 235)); // Sky blue

        spriteBatch.Begin();

        // Draw game content
        if (_font != null && _playerProfile != null)
        {
            var centerX = Game.GraphicsDevice.Viewport.Width / 2;
            var centerY = Game.GraphicsDevice.Viewport.Height / 2;
            var lang = LocalizationService.Instance;

            // Draw welcome message
            string welcomeText = string.Format(lang.Get("Gameplay.Welcome"), _playerProfile.FarmName, _playerProfile.Name);
            var welcomeSize = _font.MeasureString(welcomeText);
            spriteBatch.DrawString(_font, welcomeText, new Vector2(centerX - welcomeSize.X / 2, centerY - 150), Color.White);

            // Draw player info
            string infoText = string.Format(lang.Get("Gameplay.Info"), _playerProfile.FavoriteThing, _playerProfile.AnimalPreference);
            var infoSize = _font.MeasureString(infoText);
            spriteBatch.DrawString(_font, infoText, new Vector2(centerX - infoSize.X / 2, centerY - 100), Color.Yellow);

            // Draw placeholder game elements
            string gameText = lang.Get("Gameplay.ComingSoon");
            var gameSize = _font.MeasureString(gameText);
            spriteBatch.DrawString(_font, gameText, new Vector2(centerX - gameSize.X / 2, centerY - 50), Color.Green);

            // Draw back button
            _backButton?.Draw(spriteBatch);
        }

        spriteBatch.End();
    }
}