using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Naming;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Computer
{
    class StorageSystemChooseBoxScreen : Screen
    {
        private const int VISIBLE_BOXES = 4;

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private OptionsBox _optionsBox;

        private int _index;
        private int _scrollIndex;

        public StorageSystemChooseBoxScreen(Screen preScreen)
        {
            _preScreen = preScreen;
            // set index to active box
            _index = Controller.ActivePlayer.ActiveBoxIndex;
            while (_index > VISIBLE_BOXES - 1)
            {
                _index--;
                _scrollIndex++;
            }
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.BufferUp = 1;
            _optionsBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 11);
            _optionsBox.OffsetY -= (int)(Border.SCALE * Border.UNIT * 3);
            _optionsBox.OptionSelected += OptionSelected;
            _optionsBox.CloseAfterSelection = false;
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
            Border.Draw(_batch, startX, 0, Border.SCREEN_WIDTH, 4, Border.SCALE);
            Border.Draw(_batch, startX, unit * 4, 11, 10, Border.SCALE);
            Border.Draw(_batch, startX + unit * 11, unit * 7, 9, 7, Border.SCALE);
            Border.Draw(_batch, startX, unit * 14, Border.SCREEN_WIDTH, 4, Border.SCALE);

            // title
            var currentBox = Controller.ActivePlayer.Boxes[Controller.ActivePlayer.ActiveBoxIndex];
            _fontRenderer.DrawText(_batch, "CURRENT   " + currentBox.Name,
                new Vector2(startX + unit, unit * 2), Color.Black, Border.SCALE);

            // box list
            var visibleBoxes = Controller.ActivePlayer.Boxes
                .Skip(_scrollIndex)
                .Take(VISIBLE_BOXES)
                .Select(b => b.Name).ToList();
            if (visibleBoxes.Count < VISIBLE_BOXES)
            {
                visibleBoxes.Add("CANCEL");
            }
            var boxListText = string.Join(NewLine, visibleBoxes.Select((b, i) =>
            {
                if (i == _index)
                {
                    if (_optionsBox.Visible)
                    {
                        return "^>>" + b;
                    }
                    else
                    {
                        return ">" + b;
                    }
                }
                else
                {
                    return " " + b;
                }
            }));
            _fontRenderer.DrawText(_batch, boxListText,
                new Vector2(startX + unit, unit * 6), Color.Black, Border.SCALE);

            // box text
            if (StorageBox.BOX_COUNT > _index + _scrollIndex)
            {
                var selectedBox = Controller.ActivePlayer.Boxes[_index + _scrollIndex];
                var boxText = "POKéMON" + NewLine +
                    selectedBox.PokemonCount.ToString().PadLeft(3) + "/" + StorageBox.POKEMON_PER_BOX.ToString();
                _fontRenderer.DrawText(_batch, boxText,
                    new Vector2(startX + unit * 12, unit * 9), Color.Black, Border.SCALE);
            }

            // message
            var message = "Choose a BOX.";
            if (_optionsBox.Visible)
            {
                message = "What^'s up?";
            }
            _fontRenderer.DrawText(_batch, message,
                new Vector2(startX + unit, unit * 16), Color.Black, Border.SCALE);

            _optionsBox.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (!_optionsBox.Visible)
            {
                if (GameboyInputs.DownPressed() && _index + _scrollIndex < StorageBox.BOX_COUNT)
                {
                    _index++;
                    if (_index == VISIBLE_BOXES)
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
                    if (_index + _scrollIndex == StorageBox.BOX_COUNT)
                    {
                        Close();
                    }
                    else
                    {
                        _optionsBox.Show(new[] { "SWITCH", "NAME", "QUIT" });
                    }
                }
                else if (GameboyInputs.BPressed())
                {
                    Close();
                }
            }
            else
            {
                _optionsBox.Update();
            }
        }

        private void OptionSelected(string option, int index)
        {
            switch (index)
            {
                case 0: // switch
                    Controller.ActivePlayer.ActiveBoxIndex = _index + _scrollIndex;
                    _optionsBox.Close();
                    break;
                case 1: // name
                    {
                        var namingScreen = new NamingScreen(this, "BOX NAME?", StorageBox.MAX_NAME_LENGTH);
                        namingScreen.LoadContent();
                        namingScreen.SetIcon(Controller.Content.LoadDirect<Texture2D>("Textures/UI/Computer/boxIcon.png"));
                        namingScreen.NameSelected += ConfirmBoxName;
                        GetComponent<ScreenManager>().SetScreen(namingScreen);
                    }
                    break;
                case 2:
                    _optionsBox.Close();
                    break;
            }
        }

        private void ConfirmBoxName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var box = Controller.ActivePlayer.Boxes[_index + _scrollIndex];
                box.Name = name;
            }
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
