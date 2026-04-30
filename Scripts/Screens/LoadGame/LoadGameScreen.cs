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
            _font = Game.Content.Load<SpriteFont>(ContentPaths.DefaultFont);
        }
        catch
        {
            _font = null;
        }

        if (_font != null)
        {
            _title = LocalizationService.Instance.Get("LoadGame.Title");
            _subtitle = LocalizationService.Instance.Get("LoadGame.Subtitle");

            var centerX = Game.GraphicsDevice.Viewport.Width / 2;
            var startY = Game.GraphicsDevice.Viewport.Height / 2 - 120;

            for (int i = 0; i < _profiles.Count; i++)
            {
                var profile = _profiles[i];
                var buttonText = $"{profile.FarmName} - {profile.Name}";
                var button = new Button(_font, buttonText, new Vector2(centerX - 150, startY + i * 60));
                button.OnClick += () => LoadProfile(profile);
                _saveButtons.Add(button);
            }

            var backY = startY + Math.Max(_profiles.Count, 1) * 60 + 40;
            _backButton = new Button(_font, LocalizationService.Instance.Get("Common.Back"), new Vector2(centerX - 75, backY));
            _backButton.OnClick += BackToMenu;
        }
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var button in _saveButtons)
        {
            button.Update(gameTime);
        }

        _backButton?.Update(gameTime);

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            BackToMenu();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(new Color(30, 30, 60));
        spriteBatch.Begin();

        if (_font != null)
        {
            var centerX = Game.GraphicsDevice.Viewport.Width / 2;
            var titleSize = _font.MeasureString(_title);
            var subtitleSize = _font.MeasureString(_subtitle);
            spriteBatch.DrawString(_font, _title, new Vector2(centerX - titleSize.X / 2, 80), Color.White);
            spriteBatch.DrawString(_font, _subtitle, new Vector2(centerX - subtitleSize.X / 2, 120), Color.LightGray);

            if (_profiles.Any())
            {
                for (int i = 0; i < _profiles.Count; i++)
                {
                    var profile = _profiles[i];
                    var infoText = $"{i + 1}. {profile.FarmName} ({profile.Name})";
                    var infoPosition = new Vector2(centerX - 150, 220 + i * 60);
                    spriteBatch.DrawString(_font, infoText, infoPosition, Color.White);
                }
            }
            else
            {
                var noSavesText = LocalizationService.Instance.Get("LoadGame.NoSaves");
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
}
