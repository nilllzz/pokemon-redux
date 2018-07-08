using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Mails;
using PokemonRedux.Screens.Pokemons;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Computer
{
    class MailboxScreen : Screen
    {
        private const int MAILS_VISIBLE = 4;

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private OptionsBox _optionsBox, _confirmationBox;
        private Textbox _textbox;
        private Texture2D _arrow;

        private Mail[] _mails;
        private int _index, _scrollIndex;

        public MailboxScreen(Screen preScreen)
        {
            _preScreen = preScreen;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _arrow = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Computer/arrow2.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.BufferUp = 1;

            _confirmationBox = new OptionsBox();
            _confirmationBox.LoadContent();
            _confirmationBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 14);
            _confirmationBox.OffsetY = _textbox.OffsetY;

            SetMails();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // border
            Border.Draw(_batch, startX + unit * 7, 0, 13, 12, Border.SCALE);

            // mail list
            var visibleMails = _mails
                .Skip(_scrollIndex)
                .Take(MAILS_VISIBLE)
                .Select(m => m.Author)
                .ToList();
            if (visibleMails.Count < MAILS_VISIBLE)
            {
                visibleMails.Add("CANCEL");
            }
            var listText = "";
            for (int i = 0; i < visibleMails.Count; i++)
            {
                if (i == _index)
                {
                    if (DialogVisible)
                    {
                        listText += "^>>";
                    }
                    else
                    {
                        listText += ">";
                    }
                }
                else
                {
                    listText += " ";
                }

                listText += visibleMails[i] + NewLine;
            }
            _fontRenderer.DrawText(_batch, listText,
                new Vector2(startX + unit * 8, unit * 2), Color.Black, Border.SCALE);

            // up/down arrows
            if (_scrollIndex > 0)
            {
                _batch.Draw(_arrow, new Rectangle(startX + unit * 18, unit, unit, unit),
                    null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
            }
            if (_scrollIndex < _mails.Length - MAILS_VISIBLE + 1)
            {
                _batch.Draw(_arrow, new Rectangle(startX + unit * 18, unit * 10, unit, unit), Color.White);
            }

            _textbox.Draw(_batch, Border.DefaultWhite);
            _optionsBox.Draw(_batch, Border.DefaultWhite);
            _confirmationBox.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (!DialogVisible)
            {
                if (GameboyInputs.DownPressed() && _index + _scrollIndex < _mails.Length)
                {
                    _index++;
                    if (_index == MAILS_VISIBLE)
                    {
                        _index--;
                        _scrollIndex++;
                    }
                }
                else if (GameboyInputs.UpPressed() && _index + _scrollIndex > 0)
                {
                    _index--;
                    if (_index == -1)
                    {
                        _index++;
                        _scrollIndex--;
                    }
                }

                if (GameboyInputs.APressed())
                {
                    if (_mails.Length > _index + _scrollIndex)
                    {
                        SelectMail();
                    }
                    else
                    {
                        Close();
                    }
                }
                else if (GameboyInputs.BPressed())
                {
                    Close();
                }
            }
            else if (_confirmationBox.Visible)
            {
                _confirmationBox.Update();
            }
            else if (_optionsBox.Visible)
            {
                _optionsBox.Update();
            }
            else if (_textbox.Visible)
            {
                _textbox.Update();
            }
        }

        private void SelectMail()
        {
            _optionsBox.Show(new[] { "READ MAIL", "PUT IN PACK", "ATTACH MAIL", "CANCEL" });
            _optionsBox.OffsetY = (int)(_optionsBox.Height * Border.SCALE * Border.UNIT);
            _optionsBox.OptionSelected += MailOptionSelected;
        }

        private void MailOptionSelected(string option, int index)
        {
            _optionsBox.OptionSelected -= MailOptionSelected;
            switch (index)
            {
                case 0: // read
                    {
                        var viewScreen = new MailViewScreen(this, _mails[_index + _scrollIndex]);
                        viewScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(viewScreen);
                    }
                    break;
                case 1: // put in pack
                    PutInPack();
                    break;
                case 2: // attach
                    {
                        var partyScreen = new PartyScreen(this, _mails[_index + _scrollIndex]);
                        partyScreen.LoadContent();
                        partyScreen.AttachedMail += AttachedMail;
                        GetComponent<ScreenManager>().SetScreen(partyScreen);
                    }
                    break;
            }
        }

        private void AttachedMail()
        {
            SetMails();
        }

        private void PutInPack()
        {
            _textbox.Show("The MAIL^'s message\nwill be lost. OK?");
            _textbox.Finished += PutInPackQuestionFinished;
        }

        private void PutInPackQuestionFinished()
        {
            _textbox.Finished -= PutInPackQuestionFinished;
            _confirmationBox.Show(new[] { "YES", "NO" });
            _confirmationBox.OptionSelected += PutInPackOptionSelected;
        }

        private void PutInPackOptionSelected(string option, int index)
        {
            _confirmationBox.OptionSelected -= PutInPackOptionSelected;
            if (index == 0) // yes
            {
                var mailData = _mails[_index + _scrollIndex];
                var mailItem = mailData.GetItem();
                Controller.ActivePlayer.RemoveMailFromPC(mailData);
                Controller.ActivePlayer.AddItem(mailItem.Name, 1);

                _textbox.Show("The cleared MAIL\nwas put away.");
                _textbox.Closed += PutInPackFinished;
            }
            else // no
            {
                _textbox.Close();
            }
        }

        private void PutInPackFinished()
        {
            _textbox.Closed -= PutInPackFinished;
            SetMails();
        }

        private bool DialogVisible =>
            _optionsBox.Visible || _textbox.Visible || _confirmationBox.Visible;

        private void SetMails()
        {
            _mails = Controller.ActivePlayer.Mails;
            // if the cursor/scroll is in an invalid position, scroll up
            if (_scrollIndex + MAILS_VISIBLE > _mails.Length + 1) // +1 because cancel
            {
                _scrollIndex--;
            }
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
