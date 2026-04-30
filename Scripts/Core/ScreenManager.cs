using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Code_Game.Scripts.Core;

public class ScreenManager
{
    private readonly Game _game;
    private readonly List<Screen> _screens = new();
    private readonly List<Screen> _screensToUpdate = new();
    public static bool ReloadRequested { get; set; }

    public SpriteBatch SpriteBatch { get; private set; }

    public ScreenManager(Game game)
    {
        _game = game;
    }

    public void LoadContent()
    {
        SpriteBatch = new SpriteBatch(_game.GraphicsDevice);
        foreach (var screen in _screens)
        {
            screen.LoadContent();
        }
    }

    public void UnloadContent()
    {
        foreach (var screen in _screens)
        {
            screen.UnloadContent();
        }
    }

    public void Update(GameTime gameTime)
    {
        _screensToUpdate.Clear();
        _screensToUpdate.AddRange(_screens);

        // Update screens from top to bottom
        for (int i = _screensToUpdate.Count - 1; i >= 0; i--)
        {
            var screen = _screensToUpdate[i];
            screen.Update(gameTime);

            // If the screen is active and not a popup, we might want to stop 
            // updating screens below it, but for now we'll update all in the list.
        }
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var screen in _screens)
        {
            if (screen.IsActive)
            {
                screen.Draw(gameTime, SpriteBatch);
            }
        }
    }

    public void ReloadCurrentScreen()
    {
        ReloadRequested = false;
        var currentScreen = _screens.LastOrDefault();
        if (currentScreen == null) return;

        currentScreen.UnloadContent();
        currentScreen.LoadContent();

        System.Console.WriteLine($"[Debug] Screen reloaded: {currentScreen.GetType().Name}");
    }

    public void AddScreen(Screen screen)
    {
        screen.LoadContent();
        _screens.Add(screen);
    }

    public void RemoveScreen(Screen screen)
    {
        screen.UnloadContent();
        _screens.Remove(screen);
    }

    public static void RequestReload()
    {
        ReloadRequested = true;
    }
}