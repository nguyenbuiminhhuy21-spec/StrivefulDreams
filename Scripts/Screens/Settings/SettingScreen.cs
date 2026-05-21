using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Code_Game.Scripts.Constants;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Services.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Code_Game.Scripts.Screens.Settings;

public class SettingScreen : Screen
{
    // frame / decoration
    private Texture2D _frameTexture;
    private Texture2D _dividerTexture;
    private Texture2D _tabTexture;
    private Texture2D _overlayTexture;

    // language selector
    private Texture2D _inputTexture;     // 64×16
    private Texture2D _dotTexture;       //  8×8
    private Texture2D _dropdownTexture;  // 68×84

    private SpriteFont _font;

    private Matrix _uiScaleMatrix = Matrix.Identity;
    private MouseState _prevMouseState;
    private SettingLayoutData _layout;
    private readonly Action _onLanguageChanged;

    // tab animation
    private List<TabFrame> _tabFrames = new();
    private int[]   _tabCurrentFrame;
    private float[] _tabTimer;
    private bool[]  _tabAnimating;
    private bool[]  _tabFinished;
    private float[] _tabXOffset;
    private float[] _tabXOffsetTarget;

    // dropdown state
    private bool _isDropdownOpen;
    private int _selectedOption;
    private int _hoveredOption = -1;

    private static readonly (string Locale, string Label)[] Options =
    {
        (Locales.Vietnamese, "Tiếng Việt"),
        (Locales.English,    "English"),
    };

    public SettingScreen(Game game, ScreenManager screenManager, Action onLanguageChanged = null)
        : base(game, screenManager)
    {
        _onLanguageChanged = onLanguageChanged;
    }

    public override void LoadContent()
    {
        _frameTexture    = Game.Content.Load<Texture2D>("Graphics/Screens/Settings/frame-setting");
        _dividerTexture  = Game.Content.Load<Texture2D>("Graphics/Screens/Settings/divider-setting");
        _tabTexture      = Game.Content.Load<Texture2D>("Graphics/Screens/Settings/tab");
        _inputTexture    = Game.Content.Load<Texture2D>("Graphics/Screens/Settings/input");
        _dotTexture      = Game.Content.Load<Texture2D>("Graphics/Screens/Settings/dot-select");
        _dropdownTexture = Game.Content.Load<Texture2D>("Graphics/Screens/Settings/dropdown");
        _font            = Game.Content.Load<SpriteFont>("Fonts/DefaultFont");

        _overlayTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
        _overlayTexture.SetData(new[] { new Color(0, 0, 0, 160) });

        // sync initial selection
        string current = LocalizationService.Instance.CurrentLocale;
        for (int i = 0; i < Options.Length; i++)
            if (Options[i].Locale == current) { _selectedOption = i; break; }

        LoadLayout();
        LoadTabFrames();
        InitTabStates();
    }

    private void InitTabStates()
    {
        int count = _layout.Tabs?.Count ?? 0;
        _tabCurrentFrame  = new int[count];
        _tabTimer         = new float[count];
        _tabAnimating     = new bool[count];
        _tabFinished      = new bool[count];
        _tabXOffset       = new float[count];
        _tabXOffsetTarget = new float[count];
    }

    private void LoadLayout()
    {
        try
        {
            string path = Path.Combine(Game.Content.RootDirectory, "Data/Screens/SettingLayout.json");
            if (File.Exists(path))
                _layout = JsonSerializer.Deserialize<SettingLayoutData>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { }
        _layout ??= new SettingLayoutData();
    }

    private void LoadTabFrames()
    {
        try
        {
            string path = Path.Combine(Game.Content.RootDirectory, "Graphics/Screens/Settings/tab.json");
            if (!File.Exists(path)) return;
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            foreach (JsonProperty p in doc.RootElement.GetProperty("frames").EnumerateObject())
            {
                var f = p.Value.GetProperty("frame");
                _tabFrames.Add(new TabFrame
                {
                    Source   = new Rectangle(f.GetProperty("x").GetInt32(), f.GetProperty("y").GetInt32(),
                                             f.GetProperty("w").GetInt32(), f.GetProperty("h").GetInt32()),
                    Duration = p.Value.GetProperty("duration").GetInt32()
                });
            }
        }
        catch { }
    }

    public override void UnloadContent() => _overlayTexture?.Dispose();

    public override void Update(GameTime gameTime)
    {
        var viewport = Game.GraphicsDevice.Viewport;
        float res = viewport.Height / 720f;
        _uiScaleMatrix = Matrix.CreateScale(res, res, 1.0f);

        var ms       = Mouse.GetState();
        var mousePos = Vector2.Transform(new Vector2(ms.X, ms.Y), Matrix.Invert(_uiScaleMatrix));
        var mp       = new Point((int)mousePos.X, (int)mousePos.Y);

        int scale        = _layout.Frame.Scale;
        var frameRect    = GetFrameRect(viewport);
        var inputRect    = GetInputRect(frameRect, scale);
        var dropdownRect = GetDropdownRect(inputRect, scale);

        // tab animation — mỗi tab độc lập, chỉ chạy khi click, dừng ở frame cuối
        if (_tabFrames.Count > 0 && _tabAnimating != null)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            for (int i = 0; i < _tabAnimating.Length; i++)
            {
                if (!_tabAnimating[i]) continue;
                _tabTimer[i] += dt;
                if (_tabTimer[i] >= _tabFrames[_tabCurrentFrame[i]].Duration)
                {
                    _tabTimer[i] -= _tabFrames[_tabCurrentFrame[i]].Duration;
                    if (_tabCurrentFrame[i] < _tabFrames.Count - 1)
                        _tabCurrentFrame[i]++;
                    else
                    {
                        _tabAnimating[i] = false;
                        _tabFinished[i]  = true;
                        RecalcTabOffsetTargets(scale);
                    }
                }
            }

            // animate X offsets toward targets
            for (int j = 0; j < _tabXOffset.Length; j++)
                _tabXOffset[j] = MathHelper.Lerp(_tabXOffset[j], _tabXOffsetTarget[j], 0.15f);
        }

        // hover option when dropdown open
        _hoveredOption = -1;
        if (_isDropdownOpen)
        {
            int rowH = GetOptionRowHeight(scale);
            for (int i = 0; i < Options.Length; i++)
            {
                var row = new Rectangle(dropdownRect.X, dropdownRect.Y + i * rowH, dropdownRect.Width, rowH);
                if (row.Contains(mp)) { _hoveredOption = i; break; }
            }
        }

        // clicks
        if (ms.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed)
        {
            // click tab → reset và bắt đầu chạy animation của tab đó
            if (_tabTexture != null && _tabFrames.Count > 0 && _layout.Tabs != null)
            {
                var src0   = _tabFrames[0].Source;
                int tabW   = src0.Width  * scale;
                int tabH   = src0.Height * scale;
                int count  = _layout.Tabs.Count;
                int gap    = (_layout.TabGap - 2 * _layout.TabInset) * scale;
                int startX = frameRect.X + (frameRect.Width - (tabW * count + gap * (count - 1))) / 2;

                for (int i = 0; i < count; i++)
                {
                    var t       = _layout.Tabs[i];
                    var tabRect = new Rectangle(startX + i * (tabW + gap) + (int)_tabXOffset[i],
                                                frameRect.Y + t.OffsetY * scale,
                                                tabW, tabH);
                    if (tabRect.Contains(mp) && !_tabFinished[i] && !_tabAnimating[i])
                    {
                        _tabCurrentFrame[i] = 0;
                        _tabTimer[i]        = 0;
                        _tabAnimating[i]    = true;
                        break;
                    }
                }
            }

            if (inputRect.Contains(mp))
            {
                _isDropdownOpen = !_isDropdownOpen;
            }
            else if (_isDropdownOpen && dropdownRect.Contains(mp))
            {
                int rowH = GetOptionRowHeight(scale);
                for (int i = 0; i < Options.Length; i++)
                {
                    var row = new Rectangle(dropdownRect.X, dropdownRect.Y + i * rowH, dropdownRect.Width, rowH);
                    if (row.Contains(mp))
                    {
                        _selectedOption = i;
                        _isDropdownOpen = false;
                        LocalizationService.Instance.SetLanguage(Options[i].Locale);
                        _onLanguageChanged?.Invoke();
                        break;
                    }
                }
            }
            else if (!frameRect.Contains(mp))
            {
                if (_isDropdownOpen) _isDropdownOpen = false;
                else ScreenManager.RemoveScreen(this);
            }
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            if (_isDropdownOpen) _isDropdownOpen = false;
            else ScreenManager.RemoveScreen(this);
        }

        _prevMouseState = ms;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var viewport = Game.GraphicsDevice.Viewport;
        float res = viewport.Height / 720f;
        int virtualWidth = (int)(viewport.Width / res);
        int scale = _layout.Frame.Scale;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _uiScaleMatrix);

        // overlay
        spriteBatch.Draw(_overlayTexture, new Rectangle(0, 0, virtualWidth, 720), Color.White);

        // frame
        var frameRect = GetFrameRect(viewport);
        spriteBatch.Draw(_frameTexture, frameRect, Color.White);

        // divider
        if (_dividerTexture != null)
        {
            var d = _layout.Divider;
            spriteBatch.Draw(_dividerTexture, Scaled(frameRect.X + d.OffsetX * scale,
                                                     frameRect.Y + d.OffsetY * scale,
                                                     _dividerTexture, scale), Color.White);
        }

        // tabs — X tự căn giữa trong frame, Y chỉnh tay qua OffsetY
        if (_tabTexture != null && _tabFrames.Count > 0 && _layout.Tabs != null)
        {
            var src0   = _tabFrames[0].Source;
            int tabW   = src0.Width  * scale;
            int tabH   = src0.Height * scale;
            int count  = _layout.Tabs.Count;
            int gap    = _layout.TabGap * scale;
            int startX = frameRect.X + (frameRect.Width - (tabW * count + gap * (count - 1))) / 2;

            for (int i = 0; i < count; i++)
            {
                var t   = _layout.Tabs[i];
                var src = _tabFrames[_tabCurrentFrame[i]].Source;
                spriteBatch.Draw(_tabTexture,
                    new Rectangle(startX + i * (tabW + gap) + (int)_tabXOffset[i], frameRect.Y + t.OffsetY * scale,
                                  tabW, tabH), src, Color.White);
            }
        }

        // input box (natural size × scale, no stretch)
        var inputRect = GetInputRect(frameRect, scale);
        if (_inputTexture != null)
            spriteBatch.Draw(_inputTexture, inputRect, Color.White);

        // dot-select vẽ đè lên input
        if (_dotTexture != null)
        {
            var ld = _layout.LanguageDot;
            spriteBatch.Draw(_dotTexture,
                new Rectangle(inputRect.X + ld.OffsetX * scale,
                               inputRect.Y + ld.OffsetY * scale,
                               _dotTexture.Width  * scale,
                               _dotTexture.Height * scale),
                Color.White);
        }

        // current language label inside input
        if (_font != null)
        {
            string label = Options[_selectedOption].Label;
            float fs = 0.5f;
            var ts = _font.MeasureString(label) * fs;
            spriteBatch.DrawString(_font, label,
                new Vector2(inputRect.X + (inputRect.Width  - ts.X) / 2f,
                            inputRect.Y + (inputRect.Height - ts.Y) / 2f),
                Color.Black, 0f, Vector2.Zero, fs, SpriteEffects.None, 0f);
        }

        // dropdown (natural size × scale, no stretch)
        if (_isDropdownOpen && _dropdownTexture != null)
        {
            var dropRect = GetDropdownRect(inputRect, scale);
            spriteBatch.Draw(_dropdownTexture, dropRect, Color.White);

            int rowH = GetOptionRowHeight(scale);
            int padX = 4 * scale;

            for (int i = 0; i < Options.Length; i++)
            {
                int rowY = dropRect.Y + i * rowH;

                // hover highlight
                if (i == _hoveredOption)
                    spriteBatch.Draw(_overlayTexture,
                        new Rectangle(dropRect.X, rowY, dropRect.Width, rowH),
                        new Color(0, 0, 0, 40));

                // option text
                if (_font != null)
                {
                    float fs = 0.5f;
                    var ts = _font.MeasureString(Options[i].Label) * fs;
                    spriteBatch.DrawString(_font, Options[i].Label,
                        new Vector2(dropRect.X + padX,
                                    rowY + (rowH - ts.Y) / 2f),
                        i == _selectedOption ? Color.Black : new Color(100, 60, 20),
                        0f, Vector2.Zero, fs, SpriteEffects.None, 0f);
                }
            }
        }

        spriteBatch.End();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private Rectangle GetFrameRect(Viewport viewport)
    {
        float res = viewport.Height / 720f;
        int vw = (int)(viewport.Width / res);
        int s  = _layout.Frame.Scale;
        int fw = _frameTexture.Width  * s;
        int fh = _frameTexture.Height * s;
        return new Rectangle((vw - fw) / 2, (720 - fh) / 2, fw, fh);
    }

    private Rectangle GetInputRect(Rectangle frameRect, int scale)
    {
        var li = _layout.LanguageInput;
        int divBottom = frameRect.Y + (_layout.Divider.OffsetY + _dividerTexture.Height) * scale;
        return new Rectangle(
            frameRect.X + li.OffsetX * scale,
            divBottom   + li.OffsetY * scale,
            _inputTexture.Width  * scale,
            _inputTexture.Height * scale);
    }

    private Rectangle GetDropdownRect(Rectangle inputRect, int scale)
    {
        var ls = _layout.LanguageSelect;
        return new Rectangle(
            inputRect.X + ls.OffsetX * scale,
            inputRect.Bottom + ls.OffsetY * scale,
            _dropdownTexture.Width  * scale,
            _dropdownTexture.Height * scale);
    }

    private int GetOptionRowHeight(int scale)
        => (_dropdownTexture.Height * scale) / Options.Length;

    private static Rectangle Scaled(int x, int y, Texture2D tex, int scale)
        => new Rectangle(x, y, tex.Width * scale, tex.Height * scale);

    private void RecalcTabOffsetTargets(int scale)
    {
        int count = _layout.Tabs.Count;
        int extra = _layout.TabInset * scale;
        for (int j = 0; j < count; j++)
        {
            float target = 0;
            for (int k = 0; k < count; k++)
            {
                if (!_tabFinished[k]) continue;
                if (j < k) target -= extra;
                else if (j > k) target += extra;
            }
            _tabXOffsetTarget[j] = target;
        }
    }

    // ── data ──────────────────────────────────────────────────────────────────
    public class SettingLayoutData
    {
        public FrameConfig       Frame   { get; set; } = new();
        public OffsetConfig      Divider { get; set; } = new() { OffsetY = 30 };
        public int               TabGap    { get; set; } = 1;
        public int               TabInset  { get; set; } = 3;
        public List<OffsetConfig> Tabs   { get; set; } = new() {
            new() { Name = "general",  OffsetX = 0,  OffsetY = 0 },
            new() { Name = "audio",    OffsetX = 32, OffsetY = 0 },
            new() { Name = "controls", OffsetX = 64, OffsetY = 0 },
        };
        public OffsetConfig LanguageInput { get; set; } = new() { OffsetX = 8, OffsetY = 8 };
        public OffsetConfig LanguageSelect{ get; set; } = new() { OffsetX = -2, OffsetY = 2 };
        public OffsetConfig LanguageDot  { get; set; } = new() { OffsetX = 4, OffsetY = 4 };

        public class FrameConfig  { public int Scale   { get; set; } = 3; }
        public class OffsetConfig { public int OffsetX { get; set; } = 0; public int OffsetY { get; set; } = 0;
                                    public string Name { get; set; } = ""; }
    }

    private class TabFrame { public Rectangle Source; public int Duration; }
}
