using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Screens.Beginning;
using CodeGame.Scripts.Services.Multiplayer;
using Code_Game.Scripts.Services.Localization;
using System;
using System.IO;

namespace Code_Game;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private ScreenManager _screenManager;
    private ISteamMultiplayerService _steamMultiplayerService;
    private FileSystemWatcher _fileWatcher;
    private bool _reloadPending = false;
    private KeyboardState _lastKeyboardState;

    public ISteamMultiplayerService SteamMultiplayerService => _steamMultiplayerService;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            IsFullScreen = false,
            HardwareModeSwitch = false
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override async void Initialize()
    {
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;

        SetupFileWatcher();

        _graphics.ApplyChanges();

        // Initialize Steam
        // _steamMultiplayerService = new SteamMultiplayerService();
        /*
        try
        {
            var steamInitialized = await _steamMultiplayerService.InitializeAsync();
            if (steamInitialized)
            {
                Console.WriteLine("Steam multiplayer initialized successfully");
            }
            else
            {
                Console.WriteLine("Failed to initialize Steam multiplayer - continuing with single-player only");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Steam initialization failed: {ex.Message} - continuing with single-player only");
            _steamMultiplayerService = null;
        }
        */

        _screenManager = new ScreenManager(this);
        _screenManager.AddScreen(new BeginningScreen(this, _screenManager));

        base.Initialize();
    }

    private void OnClientSizeChanged(object sender, System.EventArgs e)
    {
        if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
        {
            // Use Window.ClientBounds to get the actual window size after a resize event
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        }
    }

    private void SetupFileWatcher()
    {
        string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        
        // Fallback if Data directory doesn't exist in base directory (dev environment)
        if (!Directory.Exists(dataPath))
        {
            dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        }

        if (Directory.Exists(dataPath))
        {
            _fileWatcher = new FileSystemWatcher
            {
                Path = dataPath,
                Filter = "*.json",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            _fileWatcher.Changed += (s, e) =>
            {
                Console.WriteLine($"[HotReload] File changed: {e.FullPath}");
                _reloadPending = true;
            };
        }
    }

    protected override void LoadContent()
    {
        _screenManager.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState currentKeyState = Keyboard.GetState();

        // Global Debug Refresh: Press F5 to reload current screen
        if (currentKeyState.IsKeyDown(Keys.F5) && _lastKeyboardState.IsKeyUp(Keys.F5))
        {
            ScreenManager.RequestReload();
        }

        // Language Switch: Press L to switch language
        if (currentKeyState.IsKeyDown(Keys.L) && _lastKeyboardState.IsKeyUp(Keys.L))
        {
            LocalizationService.Instance.SwitchLanguage();
            ScreenManager.RequestReload();
        }
        _lastKeyboardState = currentKeyState;

        if (_reloadPending || ScreenManager.ReloadRequested)
        {
            if (_reloadPending)
            {
                // Reload localization if a locale file changed
                LocalizationService.Instance.LoadLocale(LocalizationService.Instance.CurrentLocale);
            }
            _reloadPending = false;
            _screenManager.ReloadCurrentScreen();
        }

        _steamMultiplayerService?.Update();
        _screenManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _screenManager.Draw(gameTime);

        base.Draw(gameTime);
    }
}
