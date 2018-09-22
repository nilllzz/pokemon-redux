using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using static Core;

namespace PokemonRedux.Screens.Mails
{
    class MailComposeScreen : Screen
    {
        private const int CHARS_PER_LINE = 10;
        private const int SELECTOR_FLICKER_DELAY = 3;
        private const int MENU_OPTIONS_COUNT = 3;
        private const int ICON_ANIMATION_DELAY = 14;
        private static readonly string[] UPPER_CHARS = new[]
        {
            "A","B","C","D","E","F","G","H","I","J",
            "K","L","M","N","O","P","Q","R","S","T",
            "U","V","W","X","Y","Z"," ",",","?","!",
            "1","2","3","4","5","6","7","8","9","0",
            "^PK","^MN","^PO","^Ké","é","♂","♀","$","^..","*"
        };
        private static readonly string[] LOWER_CHARS = new[]
        {
            "a","b","c","d","e","f","g","h","i","j",
            "k","l","m","n","o","p","q","r","s","t",
            "u","v","w","x","y","z"," ",".","-","/",
            "^'d","^'l","^'m","^'r","^'s","^'t","^'v","&","(",")",
            "^[[","^]]","[","]","'",":",";"," "," "," "
        };

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private Texture2D _border, _selector, _placeholders, _selectorMenu, _icon;

        private int _charX, _charY = 0;
        private int _menuIndex = 0;
        private bool _menuActive = false; // if the menu at the bottom of the screen is being controlled
        private string _message = string.Empty;
        private bool _selectorVisible = true;
        private int _selectorDelay = SELECTOR_FLICKER_DELAY;
        private bool _isLower = false; // if the lower charset is selected
        private int _iconFrame = 0;
        private int _iconFrameDelay = ICON_ANIMATION_DELAY;

        private int MenuColumn => MathHelper.Clamp((int)Math.Floor((double)_charX / 3), 0, 2);
        private string[] ActiveCharset => _isLower ? LOWER_CHARS : UPPER_CHARS;

        public event Action<string> MailDataGenerated;

        public MailComposeScreen(Screen preScreen)
        {
            _preScreen = preScreen;
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
            _icon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Mail/composeIcon.png");
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

            // borders
            for (var x = 0; x < Border.SCREEN_WIDTH; x++)
            {
                for (var y = 0; y < 6; y++)
                {
                    if (y == 0 || x == 0 || x == Border.SCREEN_WIDTH - 1 || y == 5)
                    {
                        _batch.Draw(_border, new Rectangle(startX + x * unit, y * unit, unit, unit), Color.White);
                    }
                }
            }

            // compose icon
            _batch.Draw(_icon, new Rectangle(
                startX, 0,
                (int)(Border.SCALE * _icon.Height),
                (int)(Border.SCALE * _icon.Height)),
                new Rectangle(_iconFrame * 16, 0, 16, 16),
                Color.White);

            // message
            var lines = Mail.GetLinesFromMessage(_message);
            _fontRenderer.DrawText(_batch, lines[0],
                new Vector2(startX + unit * 2, unit * 2), Color.Black, Border.SCALE);
            if (lines[1] != null)
            {
                _fontRenderer.DrawText(_batch, lines[1],
                    new Vector2(startX + unit * 2, unit * 4), Color.Black, Border.SCALE);
            }

            // placeholders
            var messageLength = PokemonFontRenderer.PrintableCharAmount(_message);
            for (var i = messageLength; i < Mail.MESSAGE_MAX_LENGTH; i++)
            {
                var y = 0;
                var x = i;
                if (x >= Mail.MESSAGE_CHARS_PER_LINE)
                {
                    y++;
                    x -= Mail.MESSAGE_CHARS_PER_LINE;
                }
                var textureOffset = i > messageLength ? 1 : 0;
                _batch.Draw(_placeholders,
                    new Rectangle(
                        startX + unit * 2 + unit * x, unit * 2 + y * unit * 2,
                        (int)(_placeholders.Height * Border.SCALE),
                        (int)(_placeholders.Height * Border.SCALE)),
                    new Rectangle(textureOffset * 8, 0, 8, 8), Color.White);
            }

            // char list
            var charListText = "";
            for (var i = 0; i < ActiveCharset.Length; i++)
            {
                charListText += ActiveCharset[i];
                if ((i + 1) % CHARS_PER_LINE == 0)
                {
                    charListText += Environment.NewLine;
                }
                else
                {
                    charListText += " ";
                }
            }
            _fontRenderer.DrawText(_batch, charListText,
                new Vector2(startX + unit, unit * 7), Color.Black, Border.SCALE);

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
                new Vector2(startX + unit, unit * 17), Color.Black, Border.SCALE);

            // selector
            if (_selectorVisible)
            {
                if (_menuActive)
                {
                    _batch.Draw(_selectorMenu, new Rectangle(
                        startX + unit + unit * _menuIndex * 6,
                        (int)(unit * 17 - Border.SCALE),
                        (int)(_selectorMenu.Width * Border.SCALE),
                        (int)(_selectorMenu.Height * Border.SCALE)), Color.White);
                }
                else
                {
                    _batch.Draw(_selector, new Rectangle(
                        (int)(startX + unit * _charX * 2 + unit - Border.SCALE),
                        (int)(unit * _charY * 2 + unit * 7 - Border.SCALE),
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

            _iconFrameDelay--;
            if (_iconFrameDelay == 0)
            {
                _iconFrameDelay = ICON_ANIMATION_DELAY;
                _iconFrame++;
                if (_iconFrame == 2)
                {
                    _iconFrame = 0;
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
                BackspaceMessage();
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
                            BackspaceMessage();
                            break;
                        case 2: // END
                            Close();
                            break;
                    }
                }
                else
                {
                    var messageLength = PokemonFontRenderer.PrintableCharAmount(_message);
                    if (messageLength < Mail.MESSAGE_MAX_LENGTH)
                    {
                        var activeChar = ActiveCharset[_charX + _charY * CHARS_PER_LINE];
                        _message += activeChar;
                        messageLength = PokemonFontRenderer.PrintableCharAmount(_message);
                        if (messageLength == Mail.MESSAGE_MAX_LENGTH)
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

        private void BackspaceMessage()
        {
            if (_message.Length > 0)
            {
                // check for escape char
                if (_message.Length >= 3 && _message[_message.Length - 3] == PokemonFontRenderer.ESCAPE_CHAR)
                {
                    _message = _message.Remove(_message.Length - 3, 3);
                }
                else
                {
                    _message = _message.Remove(_message.Length - 1, 1);
                }
            }
        }

        private void Close()
        {
            var itemData = Mail.GenerateItemData(Controller.ActivePlayer.Name, _message);
            MailDataGenerated?.Invoke(itemData);
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
