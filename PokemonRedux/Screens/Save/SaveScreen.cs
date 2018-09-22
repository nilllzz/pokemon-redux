using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Overworld.Entities;
using System;
using static Core;

namespace PokemonRedux.Screens.Save
{
    class SaveScreen : Screen
    {
        private const int SAVED_DELAY = 60;

        private readonly Screen _preScreen;
        private readonly PlayerCharacter _playerEntity;
        private readonly string _infoText;

        private Textbox _textbox;
        private OptionsBox _optionsBox;
        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;

        private bool _saved = false;
        private int _savedDelay; // delay until the "Saved the game" message closes itself

        public static string GetInfoText()
        {
            var badges = Controller.ActivePlayer.Badges.Length.ToString();

            var (hours, minutes) = Controller.ActivePlayer.GetDisplayTime();
            var time = hours + "^:1" + minutes;

            var dexStr = string.Empty;
            // only put "POKEDEX" information if the player has received one yet
            if (Controller.ActivePlayer.HasPokedex)
            {
                var pokedex = (Controller.ActivePlayer.PokedexSeen.Length + Controller.ActivePlayer.PokedexCaught.Length).ToString();
                dexStr = "POKéDEX" + pokedex.PadLeft(7);
            }

            var text = $"PLAYER {Controller.ActivePlayer.Name}" + Environment.NewLine +
                "BADGES" + badges.PadLeft(8) + Environment.NewLine +
                 dexStr + Environment.NewLine +
                "TIME" + time.PadLeft(12);

            return text;
        }

        public SaveScreen(Screen preScreen, PlayerCharacter playerEntity)
        {
            _preScreen = preScreen;
            _playerEntity = playerEntity;
            _infoText = GetInfoText();
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.Show("Would you like to\nsave the game?");
            _textbox.Finished += TextboxFinished;

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.OptionSelected += SelectedQuestion;

            var unit = (int)(Border.UNIT * Border.SCALE);
            _optionsBox.OffsetY = unit * 12;
            _textbox.OffsetY = unit * 12;
        }

        internal override void UnloadContent()
        {

        }

        internal override void Update(GameTime gameTime)
        {
            if (_saved)
            {
                _savedDelay--;
                if (_savedDelay == 0)
                {
                    Close();
                }
            }
            else
            {
                if (!_optionsBox.Visible)
                {
                    _textbox.Update();
                }
                _optionsBox.Update();
            }
        }

        private void TextboxFinished()
        {
            _optionsBox.Show(new[] { "YES", "NO" });
        }

        private void SavedMessageFinished()
        {
            _saved = true;
            _savedDelay = SAVED_DELAY;
        }

        private void SelectedQuestion(string option, int index)
        {
            switch (index)
            {
                case 0: // yes
                    if (PlayerData.SaveFileExists())
                    {
                        _textbox.Show("There is already a\nsave file. Is it\nOK to overwrite?");
                        _optionsBox.OptionSelected -= SelectedQuestion;
                        _optionsBox.OptionSelected += SelectedOverwrite;
                    }
                    else
                    {
                        OverwriteSave();
                    }
                    break;

                case 1: // no
                    Close();
                    break;
            }
        }

        private void SelectedOverwrite(string option, int index)
        {
            switch (index)
            {
                case 0: // yes
                    OverwriteSave();
                    break;

                case 1: // no
                    Close();
                    break;
            }
        }

        private void OverwriteSave()
        {
            Controller.ActivePlayer.Save(_playerEntity);

            _textbox.Show($"{Controller.ActivePlayer.Name} saved\nthe game.");
            _textbox.Finished -= TextboxFinished;
            _textbox.Finished += SavedMessageFinished;
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            // draw stats
            var unit = (int)(Border.UNIT * Border.SCALE);
            var startX = (int)(Controller.ClientRectangle.Width / 2f + (-Border.SCREEN_WIDTH / 2f + 4) * unit);
            Border.Draw(_batch, startX, 0, 16, 10, Border.SCALE);

            _fontRenderer.DrawText(_batch, _infoText, new Vector2(startX + unit, unit * 2), Color.Black, Border.SCALE);

            _textbox.Draw(_batch, Border.DefaultWhite);
            _optionsBox.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }
    }
}
