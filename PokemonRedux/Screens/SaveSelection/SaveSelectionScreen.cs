using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Overworld;
using PokemonRedux.Screens.Options;
using PokemonRedux.Screens.Overworld;
using PokemonRedux.Screens.Save;
using PokemonRedux.Screens.Title;
using PokemonRedux.Screens.Transition;
using System;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens.SaveSelection
{
    class SaveSelectionScreen : Screen
    {
        private readonly TitleScreen _titleScreen;

        private SpriteBatch _batch;
        private OptionsBox _optionsBox;
        private PokemonFontRenderer _fontRenderer;
        private string _infoText = null;
        private bool _showInfoText = false;

        public SaveSelectionScreen(TitleScreen titleScreen)
        {
            _titleScreen = titleScreen;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.BufferUp = 1;
            _optionsBox.BufferRight = 4;
            _optionsBox.CanCancel = false;
            _optionsBox.CloseAfterSelection = false;

            var options = new List<string>() { "NEW GAME", "OPTION" };
            if (PlayerData.SaveFileExists())
            {
                // load player file if it exists
                Controller.ActivePlayer = new Player();
                Controller.ActivePlayer.Load();

                options.Insert(0, "CONTINUE");
            }
            _optionsBox.Show(options.ToArray());
            _optionsBox.OptionSelected += OptionSelected;

            var unit = Border.UNIT * Border.SCALE;
            var offset = Controller.ClientRectangle.Height / 2 - Border.SCREEN_HEIGHT * unit / 2;
            _optionsBox.OffsetY = (int)(_optionsBox.Height * unit + offset);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        internal override void UnloadContent()
        {
        }

        internal override void Draw(GameTime gameTime)
        {
            _batch.Begin(samplerState: SamplerState.PointClamp);

            _batch.DrawRectangle(Controller.ClientRectangle, Border.DefaultWhite);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();
            if (_showInfoText)
            {
                var offsetY = Controller.ClientRectangle.Height / 2 - Border.SCREEN_HEIGHT * unit / 2 + unit * 8;
                var offsetX = startX + unit * 4;
                Border.Draw(_batch, offsetX, offsetY, 16, 10, Border.SCALE, Border.DefaultWhite);

                _fontRenderer.DrawText(_batch, _infoText, new Vector2(offsetX + unit, offsetY + unit * 2), Color.Black, Border.SCALE);
            }
            else
            {
                _optionsBox.Draw(_batch, Border.DefaultWhite);

                var offsetY = Controller.ClientRectangle.Height / 2 - Border.SCREEN_HEIGHT * unit / 2 + unit * 12;
                Border.Draw(_batch, startX, offsetY, 15, 6, Border.SCALE, Border.DefaultWhite);
                var timeStr = DateHelper.GetDisplayDayOfWeek(DateTime.Now).ToUpper() + Environment.NewLine +
                    "   " + DateHelper.GetDisplayDaytime(World.DetermineDaytime()).PadRight(4).ToUpper() + DateTime.Now.ToString("h:mm").PadLeft(5);
                _fontRenderer.DrawText(_batch, timeStr, new Vector2(startX + unit, offsetY + unit * 2), Color.Black, Border.SCALE);
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (!_showInfoText)
            {
                _optionsBox.Update();
            }
            else
            {
                if (GameboyInputs.APressed())
                {
                    ContinueGame(gameTime);
                }
            }

            if (GameboyInputs.BPressed())
            {
                if (_showInfoText)
                {
                    _showInfoText = false;
                }
                else
                {
                    GetComponent<ScreenManager>().SetScreen(_titleScreen);
                }
            }
        }

        private void OptionSelected(string option, int index)
        {
            switch (option)
            {
                case "CONTINUE":
                    _showInfoText = true;
                    _infoText = SaveScreen.GetInfoText();
                    break;
                case "OPTION":
                    var optionsScreen = new OptionsScreen(this, true);
                    optionsScreen.LoadContent();
                    GetComponent<ScreenManager>().SetScreen(optionsScreen);
                    break;
            }
        }

        private void ContinueGame(GameTime gameTime)
        {
            var worldScreen = new WorldScreen();
            worldScreen.LoadContent();
            // update the world screen once to properly load the player entity
            worldScreen.Update(gameTime);

            var transitionScreen = new FadeTransitionScreen(this, worldScreen, 0.02f);
            transitionScreen.LoadContent();

            GetComponent<ScreenManager>().SetScreen(transitionScreen);
        }
    }
}
