using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game;
using PokemonRedux.Game.Data;
using PokemonRedux.Screens.TownMap;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Pokegear
{
    class PokegearScreen : Screen, ITextboxScreen
    {
        private const int MODULES_COUNT = 4;
        private const int MAX_PHONE_CONTACTS = 10;
        private const int CONTACTS_VISIBLE = 4;
        private static Color TINTED_WHITE = new Color(224, 248, 160);

        private readonly Screen _preScreen;
        private readonly Contact _emptyContact; // visible representation of an empty contact

        private SpriteBatch _batch;
        private Texture2D _arrow, _buttons, _switchButton, _timeBox, _mapOverlay, _phoneBackground, _reception;
        private PokemonFontRenderer _fontRenderer, _contactsFontRenderer;
        private Textbox _exitTextbox; // textbox shown at the first screen instructing the player how to exit
        private Textbox _phoneTextbox; // displays "Whom?" question and "..." call animation
        private Textbox _callTextbox; // used for call text
        private OptionsBox _phoneOptionsBox, _deleteNumberOptionsBox;
        private TownMapScreen _mapScreen;
        private Phonebook _phonebook;

        private int _pageIndex = 0;
        private int _phoneScroll = 0;
        private int _phoneIndex = 0;

        public PokegearScreen(Screen preScreen)
        {
            _preScreen = preScreen;
            _emptyContact = new Contact
            {
                name = "----------",
                important = true,
                title = ""
            };
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _arrow = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/arrow.png");
            _buttons = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/buttons.png");
            _switchButton = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/switchButton.png");
            _timeBox = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/time.png");
            _mapOverlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/mapOverlay.png");
            _phoneBackground = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/phoneBackground.png");
            _reception = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokegear/reception.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _contactsFontRenderer = new PokemonFontRenderer();
            _contactsFontRenderer.LoadContent();
            _contactsFontRenderer.LineGap = 0;

            _exitTextbox = new Textbox();
            _exitTextbox.LoadContent();
            _exitTextbox.ShowAndSkip("Press any button\nto exit.");
            _exitTextbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _phoneTextbox = new Textbox();
            _phoneTextbox.LoadContent();
            _phoneTextbox.ShowAndSkip("Whom do you want\nto call?");
            _phoneTextbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _phoneOptionsBox = new OptionsBox();
            _phoneOptionsBox.LoadContent();
            _phoneOptionsBox.OffsetY = _phoneTextbox.OffsetY;
            _phoneOptionsBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 9);
            _phoneOptionsBox.BufferUp = 1;
            _phoneOptionsBox.BufferRight = 1;

            _deleteNumberOptionsBox = new OptionsBox();
            _deleteNumberOptionsBox.LoadContent();
            _deleteNumberOptionsBox.OffsetY = _phoneTextbox.OffsetY;
            _deleteNumberOptionsBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 14);

            // TODO: get correct location from world
            _mapScreen = new TownMapScreen(null, "kanto", "Pallet Town");
            _mapScreen.LoadContent();

            _phonebook = Phonebook.Load();
        }

        internal override void UnloadContent()
        {
            _mapScreen?.UnloadContent();
            _mapScreen = null;
        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var unit = (int)(Border.SCALE * Border.UNIT);
            var width = Border.SCREEN_WIDTH * unit;
            var height = Border.SCREEN_HEIGHT * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            // draw black background
            _batch.DrawRectangle(new Rectangle(startX, 0, width, height), Color.Black);

            // draw pages
            switch (_pageIndex)
            {
                case 0:
                    DrawInfoPage(startX, unit, width);
                    break;
                case 1:
                    DrawMapPage(gameTime, startX);
                    break;
                case 2:
                    DrawPhonePage(startX, unit, width);
                    break;
                case 3:
                    DrawRadioPage(startX, unit);
                    break;
            }

            // draw buttons and selection arrow
            for (var i = 0; i < MODULES_COUNT; i++)
            {
                if ((i == 0) || // time/exit
                    (i == 1 && Controller.ActivePlayer.HasMapModule) ||
                    (i == 2) || // phone
                    (i == 3 && Controller.ActivePlayer.HasRadioModule))
                {
                    _batch.Draw(_buttons, new Rectangle(startX + i * unit * 2, 0, unit * 2, unit * 2),
                        new Rectangle(i * 16, 0, 16, 16), Color.White);
                }

                if (i == _pageIndex)
                {
                    _batch.Draw(_arrow, new Rectangle(
                        startX + i * unit * 2,
                        (int)(unit * 2 - 3 * Border.SCALE),
                        (int)(Border.SCALE * _arrow.Width),
                        (int)(Border.SCALE * _arrow.Height)), Color.White);
                }
            }

            _batch.End();
        }

        private void DrawInfoPage(int startX, int unit, int width)
        {
            // switch button
            var switchButtonX = (int)(startX + width - _switchButton.Width * Border.SCALE);

            _batch.Draw(_switchButton, new Rectangle(
                switchButtonX, 0,
                (int)(_switchButton.Width * Border.SCALE),
                (int)(_switchButton.Height * Border.SCALE)), Color.White);

            _fontRenderer.DrawText(_batch, "SWITCH>",
                new Vector2(switchButtonX + unit, unit), Color.Black, Border.SCALE);

            // time/date info
            _batch.Draw(_timeBox, new Rectangle(
                startX + unit * 2, unit * 4,
                (int)(_timeBox.Width * Border.SCALE),
                (int)(_timeBox.Height * Border.SCALE)), Color.White);

            var dateTime = DateTime.Now;
            var time = DateHelper.GetAmPmTime(dateTime);
            if (time.StartsWith("0"))
            {
                time = " " + time.Remove(0, 1);
            }
            _fontRenderer.DrawText(_batch,
                DateHelper.GetDisplayDayOfWeek(dateTime).ToUpper() + Environment.NewLine +
                time,
                new Vector2(startX + unit * 6, unit * 6), Color.Black, Border.SCALE);

            _exitTextbox.Draw(_batch, TINTED_WHITE);
        }

        private void DrawMapPage(GameTime gameTime, int startX)
        {
            _mapScreen.Draw(gameTime, _batch);

            // draw pokegear overlay over the top left corner of the map
            _batch.Draw(_mapOverlay, new Rectangle(
                startX, 0,
                (int)(_mapOverlay.Width * Border.SCALE),
                (int)(_mapOverlay.Height * Border.SCALE)), Color.White);
        }

        private void DrawPhonePage(int startX, int unit, int width)
        {
            // background
            _batch.Draw(_phoneBackground, new Rectangle(
                startX, 0,
                (int)(_phoneBackground.Width * Border.SCALE),
                (int)(_phoneBackground.Height * Border.SCALE)), Color.White);

            // reception
            // TODO: get reception from map file
            var hasReception = true;
            _batch.Draw(_reception, new Rectangle(
                startX + width - unit * 3, unit,
                (int)(16 * Border.SCALE),
                (int)(16 * Border.SCALE)),
                new Rectangle(hasReception ? 0 : 16, 0, 16, 16), Color.White);

            // contacts
            var visibleContacts = new Contact[CONTACTS_VISIBLE];
            var contacts = Controller.ActivePlayer.Contacts;
            for (var i = 0; i < visibleContacts.Length; i++)
            {
                var index = i + _phoneScroll;
                if (contacts.Length > index)
                {
                    visibleContacts[i] = _phonebook.GetContact(contacts[index]);
                }
                else
                {
                    visibleContacts[i] = _emptyContact;
                }
            }
            var contactsText = string.Join(Environment.NewLine, visibleContacts.Select((c, i) =>
            {
                var selector = ">";
                if (_phoneOptionsBox.Visible || _deleteNumberOptionsBox.Visible)
                {
                    selector = "^>>";
                }
                return
                    (_phoneIndex == i ? selector : " ") +
                    c.name + ":" + Environment.NewLine +
                    "    " + c.title;
            }));

            _contactsFontRenderer.DrawText(_batch, contactsText,
                new Vector2(startX + unit, unit * 4),
                Color.Black, Border.SCALE);

            _phoneOptionsBox.Draw(_batch, TINTED_WHITE);
            _deleteNumberOptionsBox.Draw(_batch, TINTED_WHITE);
            _phoneTextbox.Draw(_batch, TINTED_WHITE);
        }

        private void DrawRadioPage(int startX, int unit)
        {
            _fontRenderer.DrawText(_batch,
                " NOT IMPLEMENTED \nPLEASE LEAVE NOW!",
                new Vector2(startX + unit, unit * 8), TINTED_WHITE, Border.SCALE);
        }

        internal override void Update(GameTime gameTime)
        {
            switch (_pageIndex)
            {
                case 0:
                    UpdateInfoPage();
                    break;
                case 1:
                    UpdateMapPage(gameTime);
                    break;
                case 2:
                    UpdatePhonePage();
                    break;
                case 3:
                    UpdateRadioPage();
                    break;
            }
        }

        private void UpdateChangePage()
        {
            if (GameboyInputs.RightPressed())
            {
                _pageIndex++;
                if ((_pageIndex == 1 && !Controller.ActivePlayer.HasMapModule) ||
                    (_pageIndex == 3 && !Controller.ActivePlayer.HasRadioModule))
                {
                    _pageIndex = 2;
                }
                else if (_pageIndex == 4)
                {
                    _pageIndex = 3;
                }
            }
            else if (GameboyInputs.LeftPressed())
            {
                _pageIndex--;
                if (_pageIndex == -1 || (_pageIndex == 1 && !Controller.ActivePlayer.HasMapModule))
                {
                    _pageIndex = 0;
                }
            }
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }

        private void UpdateInfoPage()
        {
            if (GameboyInputs.APressed() ||
                GameboyInputs.BPressed() ||
                GameboyInputs.SelectPressed() ||
                GameboyInputs.StartPressed())
            {
                Close();
            }
            else
            {
                UpdateChangePage();
            }
        }

        private void UpdateMapPage(GameTime gameTime)
        {
            _mapScreen.Update(gameTime);

            if (GameboyInputs.BPressed())
            {
                Close();
            }
            else
            {
                UpdateChangePage();
            }
        }

        private void UpdatePhonePage()
        {
            if (!_phoneOptionsBox.Visible && !_deleteNumberOptionsBox.Visible)
            {
                if (GameboyInputs.DownPressed())
                {
                    if (_phoneIndex < CONTACTS_VISIBLE - 1)
                    {
                        _phoneIndex++;
                    }
                    else if (_phoneScroll < MAX_PHONE_CONTACTS - CONTACTS_VISIBLE)
                    {
                        _phoneScroll++;
                    }
                }
                else if (GameboyInputs.UpPressed())
                {
                    if (_phoneIndex > 0)
                    {
                        _phoneIndex--;
                    }
                    else if (_phoneScroll > 0)
                    {
                        _phoneScroll--;
                    }
                }

                if (GameboyInputs.APressed())
                {
                    if (Controller.ActivePlayer.Contacts.Length > _phoneIndex + _phoneScroll)
                    {
                        var contact = _phonebook.GetContact(Controller.ActivePlayer.Contacts[_phoneIndex + _phoneScroll]);
                        if (contact.important)
                        {
                            _phoneOptionsBox.Show(new[] { "CALL", "CANCEL" });
                        }
                        else
                        {
                            _phoneOptionsBox.Show(new[] { "CALL", "DELETE", "CANCEL" });
                        }
                        _phoneOptionsBox.OptionSelected += CallOptionsSelected;
                    }
                }
                else if (GameboyInputs.BPressed())
                {
                    Close();
                }
                else
                {
                    UpdateChangePage();
                }
            }
            else if (_deleteNumberOptionsBox.Visible)
            {
                _deleteNumberOptionsBox.Update();
            }
            else if (_phoneOptionsBox.Visible)
            {
                _phoneOptionsBox.Update();
            }
        }

        private void CallOptionsSelected(string option, int index)
        {
            _phoneOptionsBox.OptionSelected -= CallOptionsSelected;
            switch (option)
            {
                // TODO: call with reception
                case "CALL":
                    break;
                case "DELETE":
                    _phoneTextbox.ShowAndSkip("Delete this stored\nphone number?");
                    _deleteNumberOptionsBox.Show(new[] { "YES", "NO" });
                    _deleteNumberOptionsBox.OptionSelected += DeleteNumberOptionsSelected;
                    break;
            }
        }

        private void DeleteNumberOptionsSelected(string option, int index)
        {
            _deleteNumberOptionsBox.OptionSelected -= DeleteNumberOptionsSelected;
            switch (index)
            {
                case 0: // yes
                    var contact = _phonebook.GetContact(Controller.ActivePlayer.Contacts[_phoneIndex + _phoneScroll]);
                    Controller.ActivePlayer.DeleteContact(contact.name);
                    break;
            }
            _phoneTextbox.ShowAndSkip("Whom do you want\nto call?");
        }

        private void UpdateRadioPage()
        {
            if (GameboyInputs.BPressed())
            {
                Close();
            }
            else
            {
                UpdateChangePage();
            }
        }

        // used for calls
        public void ShowTextbox(string text, bool skip)
        {
            _callTextbox.Show(text);
        }
    }
}
