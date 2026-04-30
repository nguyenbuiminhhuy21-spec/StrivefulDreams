using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Screens.Beginning;
using Code_Game.Scripts.Contracts.CharacterCreation;
using Code_Game.Scripts.Services.CharacterCreation;
using Code_Game.Scripts.UI;
using Code_Game.Scripts.Services.Localization;

namespace Code_Game.Scripts.Screens.CharacterCreation;

public class CharacterCreationScreen : Screen
{
    private SpriteFont _font;
    private TextInput _nameInput;
    private TextInput _farmInput;
    private TextInput _favoriteInput;
    private Button _okButton;
    private Button _backButton;
    private Button _animalLeftButton;
    private Button _animalRightButton;
    private ICharacterCreationService _creationService;
    private string _statusMessage = string.Empty;
    private static Texture2D _whitePixel;

    public CharacterCreationScreen(Game game, ScreenManager screenManager) : base(game, screenManager)
    {
        _creationService = new CharacterCreationService();
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

        if (_font == null)
            return;

        var centerX = Game.GraphicsDevice.Viewport.Width / 2;
        var centerY = Game.GraphicsDevice.Viewport.Height / 2;
        var lang = LocalizationService.Instance;

        _nameInput = new TextInput(_font, lang.Get("CharacterCreation.NameLabel"), new Vector2(centerX - 280, centerY - 180));
        _farmInput = new TextInput(_font, lang.Get("CharacterCreation.FarmLabel"), new Vector2(centerX - 280, centerY - 100));
        _favoriteInput = new TextInput(_font, lang.Get("CharacterCreation.FavoriteLabel"), new Vector2(centerX - 280, centerY - 20));

        _animalLeftButton = new Button(_font, "<", new Vector2(centerX + 90, centerY + 60), Color.Goldenrod, Color.Yellow, Color.Orange);
        _animalRightButton = new Button(_font, ">", new Vector2(centerX + 180, centerY + 60), Color.Goldenrod, Color.Yellow, Color.Orange);
        _animalLeftButton.OnClick += _creationService.SelectPreviousAnimal;
        _animalRightButton.OnClick += _creationService.SelectNextAnimal;

        _okButton = new Button(_font, lang.Get("Common.OK"), new Vector2(centerX - 90, centerY + 160), Color.Green, Color.LightGreen, Color.DarkGreen);
        _okButton.OnClick += ConfirmCharacter;

        _backButton = new Button(_font, lang.Get("Common.Back"), new Vector2(centerX + 40, centerY + 160), Color.SaddleBrown, Color.Peru, Color.Brown);
        _backButton.OnClick += BackToBeginning;
    }

    public override void Update(GameTime gameTime)
    {
        if (_font == null)
            return;

        _nameInput?.Update(gameTime);
        _farmInput?.Update(gameTime);
        _favoriteInput?.Update(gameTime);
        _animalLeftButton?.Update(gameTime);
        _animalRightButton?.Update(gameTime);
        _okButton?.Update(gameTime);
        _backButton?.Update(gameTime);

        _creationService.Name = _nameInput.Text;
        _creationService.FarmName = _farmInput.Text;
        _creationService.FavoriteThing = _favoriteInput.Text;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_font == null)
        {
            Game.GraphicsDevice.Clear(Color.Black);
            return;
        }

        Game.GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin();

        var panelRect = new Rectangle(80, 60, Game.GraphicsDevice.Viewport.Width - 160, Game.GraphicsDevice.Viewport.Height - 120);
        
        if (_whitePixel == null)
        {
            _whitePixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }
        var lang = LocalizationService.Instance;
        spriteBatch.Draw(_whitePixel, panelRect, new Color(0, 0, 0, 180));

        spriteBatch.DrawString(_font, lang.Get("CharacterCreation.Title"), new Vector2(120, 80), Color.White);
        spriteBatch.DrawString(_font, lang.Get("CharacterCreation.Subtitle"), new Vector2(120, 110), Color.LightGray);

        _nameInput?.Draw(spriteBatch);
        _farmInput?.Draw(spriteBatch);
        _favoriteInput?.Draw(spriteBatch);

        spriteBatch.DrawString(_font, lang.Get("CharacterCreation.AnimalLabel"), new Vector2(120, Game.GraphicsDevice.Viewport.Height / 2 + 80), Color.White);
        spriteBatch.DrawString(_font, _creationService.SelectedAnimal, new Vector2(160, Game.GraphicsDevice.Viewport.Height / 2 + 100), Color.Yellow);

        _animalLeftButton?.Draw(spriteBatch);
        _animalRightButton?.Draw(spriteBatch);
        _okButton?.Draw(spriteBatch);
        _backButton?.Draw(spriteBatch);

        if (!string.IsNullOrEmpty(_statusMessage))
        {
            spriteBatch.DrawString(_font, _statusMessage, new Vector2(120, Game.GraphicsDevice.Viewport.Height - 50), Color.DarkGreen);
        }

        spriteBatch.End();
    }

    private void ConfirmCharacter()
    {
        _statusMessage = _creationService.Confirm();

        // In a real project, transition to gameplay here.
    }

    private void BackToBeginning()
    {
        ScreenManager.AddScreen(new BeginningScreen(Game, ScreenManager));
        ScreenManager.RemoveScreen(this);
    }

}
