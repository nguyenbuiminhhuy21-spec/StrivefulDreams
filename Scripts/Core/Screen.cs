using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Code_Game.Scripts.Core;

public abstract class Screen
{
    protected Game Game { get; private set; }
    protected ScreenManager ScreenManager { get; private set; }

    public bool IsActive { get; set; } = true;

    public Screen(Game game, ScreenManager screenManager)
    {
        Game = game;
        ScreenManager = screenManager;
    }

    public virtual void LoadContent() { }
    public virtual void UnloadContent() { }
    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
}