using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Naming
{
    abstract class NamingScreen : Screen
    {
        private const int CHARS_PER_LINE = 9;
        private const int SELECTOR_FLICKER_DELAY = 3;
        private const int MENU_OPTIONS_COUNT = 3;
        private const int ICON_ANIMATION_DELAY = 10;

        protected abstract string[] UpperChars { get; }
        protected abstract string[] LowerChars { get; }

        private readonly Screen _preScreen;
        private readonly int _maxLength;
        private readonly string _title;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private Texture2D _border, _selector, _placeholders, _selectorMenu;

        private int _charX, _charY = 0;
        private int _menuIndex = 0;
        private bool _menuActive = false; // if the menu at the bottom of the screen is being controlled
        private string _name;
        private bool _selectorVisible = true;
        private int _selectorDelay = SELECTOR_FLICKER_DELAY;
        private bool _isLower = false; // if the lower charset is selected

        private int _iconFrame = 0;
        private int _iconFrames = 0; // number of frames
        private int _iconFrameSize;
        private int _iconFrameDelay = ICON_ANIMATION_DELAY;
        private Texture2D _icon;

        public event Action<string> NameSelected;

        private int MenuColumn => (int)Math.Floor((double)_charX / 3);
        private string[] ActiveCharset => _isLower ? LowerChars : UpperChars;

        protected NamingScreen(Screen preScreen, string title, int maxLength, string currentName = "")
        {
            _preScreen = preScreen;
            _title = title;
            _maxLength = maxLength;
            _name = currentName;
        }

        public void SetIcon(Texture2D icon)
        {
            _icon = icon;
            _iconFrames = _icon.Width / _icon.Height;
            _iconFrameSize = _icon.Height;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _border = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Naming/border.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Naming/selector.png");
            _placeholders = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Naming/placeholders.png");
            _selectorMenu = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Naming/selectorMenu.png");
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            Border.DrawCenter(_batch, startX, 0, Border.SCREEN_WIDTH, Border.SCREEN_HEIGHT, Border.SCALE);

            // draw border
            // vertical
            for (int i = 0; i < Border.SCREEN_HEIGHT; i++)
            {
                _batch.Draw(_border, new Rectangle(startX, unit * i, unit, unit), Color.White);
                _batch.Draw(_border, new Rectangle(startX + unit * 19, unit * i, unit, unit), Color.White);
            }

            var verticalOffset = (5 - ActiveCharset.Length / CHARS_PER_LINE) * 2;
            // horizontal
            var horizontalBars = new[] { 0, 5 + verticalOffset, 15, 17 };
            for (int i = 1; i < Border.SCREEN_WIDTH - 1; i++)
            {
                foreach (var y in horizontalBars)
                {
                    _batch.Draw(_border, new Rectangle(startX + i * unit, unit * y, unit, unit), Color.White);
                }
            }

            // title
            _fontRenderer.DrawText(_batch, _title,
                new Vector2(startX + unit * 5, unit * 2), Color.Black, Border.SCALE);

            // icon
            if (_icon != null)
            {
                _batch.Draw(_icon, new Rectangle(
                    startX + 2 * unit, (int)(unit * 1.5),
                    (int)(Border.SCALE * _iconFrameSize),
                    (int)(Border.SCALE * _iconFrameSize)),
                    new Rectangle(_iconFrame * _iconFrameSize, 0, _iconFrameSize, _iconFrameSize),
                    Color.White);
            }

            // name
            _fontRenderer.DrawText(_batch, _name,
                new Vector2(startX + unit * 5, unit * (4 + verticalOffset)), Color.Black, Border.SCALE);
            var nameLength = PokemonFontRenderer.PrintableCharAmount(_name);
            for (int i = nameLength; i < _maxLength; i++)
            {
                var textureOffset = i > nameLength ? 1 : 0;
                _batch.Draw(_placeholders,
                    new Rectangle(
                        startX + unit * 5 + unit * i, unit * (4 + verticalOffset),
                        (int)(_placeholders.Height * Border.SCALE),
                        (int)(_placeholders.Height * Border.SCALE)),
                    new Rectangle(textureOffset * 8, 0, 8, 8), Color.White);
            }

            // char list
            var charListText = "";
            for (int i = 0; i < ActiveCharset.Length; i++)
            {
                charListText += ActiveCharset[i];
                if ((i + 1) % CHARS_PER_LINE == 0)
                {
                    charListText += NewLine;
                }
                else
                {
                    charListText += " ";
                }
            }
            _fontRenderer.DrawText(_batch, charListText,
                new Vector2(startX + unit * 2, unit * (6 + verticalOffset)), Color.Black, Border.SCALE);

            // menu
            var menuText = "";
            if (_isLower)
            {
                menuText += "UPPER";
            }
            else
            {
                menuText += "lower";
            }
            menuText += "  DEL   END";
            _fontRenderer.DrawText(_batch, menuText,
                new Vector2(startX + unit * 2, unit * 16), Color.Black, Border.SCALE);

            // selector
            if (_selectorVisible)
            {
                if (_menuActive)
                {
                    _batch.Draw(_selectorMenu, new Rectangle(
                        startX + unit * 2 + unit * _menuIndex * 6,
                        (int)(unit * 16 - Border.SCALE),
                        (int)(_selectorMenu.Width * Border.SCALE),
                        (int)(_selectorMenu.Height * Border.SCALE)), Color.White);
                }
                else
                {
                    _batch.Draw(_selector, new Rectangle(
                        (int)(startX + unit * _charX * 2 + unit * 2 - Border.SCALE),
                        (int)(unit * _charY * 2 + unit * (6 + verticalOffset) - Border.SCALE),
                        (int)(_selector.Width * Border.SCALE),
                        (int)(_selector.Height * Border.SCALE)), Color.White);
                }
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _selectorDelay--;
            if (_selectorDelay == 0)
            {
                _selectorDelay = SELECTOR_FLICKER_DELAY;
                _selectorVisible = !_selectorVisible;
            }

            if (_iconFrames > 1)
            {
                _iconFrameDelay--;
                if (_iconFrameDelay == 0)
                {
                    _iconFrameDelay = ICON_ANIMATION_DELAY;
                    _iconFrame++;
                    if (_iconFrame == _iconFrames)
                    {
                        _iconFrame = 0;
                    }
                }
            }

            if (GameboyInputs.RightPressed())
            {
                if (_menuActive)
                {
                    _menuIndex++;
                    // wrap around to the beginning of the line
                    if (_menuIndex == MENU_OPTIONS_COUNT)
                    {
                        _menuIndex = 0;
                    }
                }
                else
                {
                    _charX++;
                    // wrap around to the beginning of the line
                    if (_charX == CHARS_PER_LINE)
                    {
                        _charX = 0;
                    }
                }
            }
            else if (GameboyInputs.LeftPressed())
            {
                if (_menuActive)
                {
                    _menuIndex--;
                    // wrap around to the end of the line
                    if (_menuIndex == -1)
                    {
                        _menuIndex = MENU_OPTIONS_COUNT - 1;
                    }
                }
                else
                {
                    _charX--;
                    // wrap around to the end of the line
                    if (_charX == -1)
                    {
                        _charX = CHARS_PER_LINE - 1;
                    }
                }
            }
            else if (GameboyInputs.DownPressed())
            {
                if (_menuActive)
                {
                    _charY = 0;
                    if (MenuColumn != _menuIndex)
                    {
                        _charX = _menuIndex * 3;
                    }
                    _menuActive = false;
                }
                else
                {
                    _charY++;
                    if (_charY == ActiveCharset.Length / CHARS_PER_LINE)
                    {
                        _menuActive = true;
                        _menuIndex = MenuColumn;
                    }
                }
            }
            else if (GameboyInputs.UpPressed())
            {
                if (_menuActive)
                {
                    _charY = ActiveCharset.Length / CHARS_PER_LINE - 1;
                    if (MenuColumn != _menuIndex)
                    {
                        _charX = _menuIndex * 3;
                    }
                    _menuActive = false;
                }
                else
                {
                    _charY--;
                    if (_charY == -1)
                    {
                        _menuActive = true;
                        _menuIndex = MenuColumn;
                    }
                }
            }

            if (GameboyInputs.BPressed())
            {
                BackspaceName();
            }
            else if (GameboyInputs.APressed())
            {
                if (_menuActive)
                {
                    switch (_menuIndex)
                    {
                        case 0: // upper/lower switch
                            _isLower = !_isLower;
                            break;
                        case 1: // DEL
                            BackspaceName();
                            break;
                        case 2: // END
                            Close();
                            break;
                    }
                }
                else
                {
                    var nameLength = PokemonFontRenderer.PrintableCharAmount(_name);
                    if (nameLength < _maxLength)
                    {
                        var activeChar = ActiveCharset[_charX + _charY * CHARS_PER_LINE];
                        _name += activeChar;
                        nameLength = PokemonFontRenderer.PrintableCharAmount(_name);
                        if (nameLength == _maxLength)
                        {
                            // select END button
                            _menuActive = true;
                            _menuIndex = 2;
                        }
                    }
                }
            }
            else if (GameboyInputs.StartPressed())
            {
                // select END button
                _menuActive = true;
                _menuIndex = 2;
            }
            else if (GameboyInputs.SelectPressed())
            {
                // switch upper/lower
                _isLower = !_isLower;
            }
        }

        private void BackspaceName()
        {
            if (_name.Length > 0)
            {
                // check for escape char
                if (_name.Length >= 3 && _name[_name.Length - 3] == PokemonFontRenderer.ESCAPE_CHAR)
                {
                    _name = _name.Remove(_name.Length - 3, 3);
                }
                else
                {
                    _name = _name.Remove(_name.Length - 1, 1);
                }
            }
        }

        private void Close()
        {
            NameSelected?.Invoke(_name);
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
