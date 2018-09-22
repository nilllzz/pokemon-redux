using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System.Collections.Generic;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Pokedex
{
    class PokedexSearchScreen : Screen
    {
        private const int SELECTOR_FLICKER_DELAY = 7;
        private const int NO_RESULTS_DELAY = 120;

        private readonly Screen _preScreen;
        private readonly Screen _prePokedexScreen;
        private readonly PokedexListMode _listMode;

        private SpriteBatch _batch;
        private Texture2D _overlay, _search, _arrow, _noResultsOverlay;
        private PokemonFontRenderer _fontRenderer;
        private string[] _type1Options, _type2Options;

        private int _index;
        private int _type1Index = 0;
        private int _type2Index = 0;
        private bool _selectorVisible = true;
        private int _selectorFlickerDelay = SELECTOR_FLICKER_DELAY;
        private int _noResultsDelay = 0;

        public PokedexSearchScreen(Screen preScreen, Screen prePokedexScreen, PokedexListMode listMode)
        {
            _preScreen = preScreen;
            _prePokedexScreen = prePokedexScreen;
            _listMode = listMode;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/searchOverlay.png");
            _search = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/search.png");
            _arrow = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/arrow.png");
            _noResultsOverlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/noResultsOverlay.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            // load type selections
            var type1Options = new List<string>();
            var type2Options = new List<string>
            {
                "----"
            };
            for (var i = 0; i < Pokemon.TYPES_AMOUNT; i++)
            {
                type1Options.Add(((PokemonType)(i + 1)).ToString().ToUpper());
                type2Options.Add(((PokemonType)(i + 1)).ToString().ToUpper());
            }
            _type1Options = type1Options.ToArray();
            _type2Options = type2Options.ToArray();
        }

        internal override void UnloadContent()
        {

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

            // title
            _fontRenderer.DrawText(_batch, "SEARCH",
                new Vector2(startX + unit * 2, unit), Border.DefaultWhite, Border.SCALE);

            // type selection
            void DrawTypeLine(string typeStr, int typeNo, int y)
            {
                while (typeStr.Length < 8)
                {
                    if (typeStr.Length % 2 == 1)
                    {
                        typeStr += " ";
                    }
                    else
                    {
                        typeStr = " " + typeStr;
                    }
                }
                typeStr = (_index == typeNo - 1 && _selectorVisible ? ">" : " ") +
                    "TYPE" + typeNo + " " + typeStr;

                _fontRenderer.DrawText(_batch, typeStr,
                    new Vector2(startX + unit * 2, y), Border.DefaultWhite, Border.SCALE);
            }
            DrawTypeLine(_type1Options[_type1Index], 1, unit * 4);
            DrawTypeLine(_type2Options[_type2Index], 2, unit * 6);

            // arrows
            var arrowSize = new Point((int)(_arrow.Width * Border.SCALE));
            _batch.Draw(_arrow, new Rectangle(new Point(startX + unit * 8, unit * 4), arrowSize), Color.White);
            _batch.Draw(_arrow, new Rectangle(new Point(startX + unit * 17, unit * 4), arrowSize),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            _batch.Draw(_arrow, new Rectangle(new Point(startX + unit * 8, unit * 6), arrowSize), Color.White);
            _batch.Draw(_arrow, new Rectangle(new Point(startX + unit * 17, unit * 6), arrowSize),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);

            // search graphic
            _batch.Draw(_search, new Rectangle(
                startX + unit * 8, unit * 9,
                (int)(Border.SCALE * _search.Width),
                (int)(Border.SCALE * _search.Height)), Color.White);

            // options
            var optionsText = (_index == 2 && _selectorVisible ? ">" : " ")
                + "BEGIN SEARCH!!" + NewLine +
                (_index == 3 && _selectorVisible ? ">" : " ")
                + "CANCEL";
            _fontRenderer.DrawText(_batch, optionsText,
                new Vector2(startX + unit * 2, unit * 13), Border.DefaultWhite, Border.SCALE);

            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            // no results overlay
            if (_noResultsDelay > 0)
            {
                _batch.Draw(_noResultsOverlay, new Rectangle(
                    startX, unit * 12,
                    (int)(Border.SCALE * _noResultsOverlay.Width),
                    (int)(Border.SCALE * _noResultsOverlay.Height)),
                    Color.White);

                _fontRenderer.DrawText(_batch, "The specified type\nwas not found.",
                    new Vector2(startX + unit, unit * 14), Border.DefaultWhite, Border.SCALE);
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            // when visible, only update the no results message
            if (_noResultsDelay > 0)
            {
                _noResultsDelay--;
                return;
            }

            _selectorFlickerDelay--;
            if (_selectorFlickerDelay == 0)
            {
                _selectorFlickerDelay = SELECTOR_FLICKER_DELAY;
                _selectorVisible = !_selectorVisible;
            }

            if (GameboyInputs.DownPressed() && _index < 3)
            {
                _index++;
            }
            else if (GameboyInputs.UpPressed() && _index > 0)
            {
                _index--;
            }
            else if (GameboyInputs.RightPressed())
            {
                switch (_index)
                {
                    case 0:
                        _type1Index++;
                        if (_type1Index == _type1Options.Length)
                        {
                            _type1Index = 0;
                        }
                        break;
                    case 1:
                        _type2Index++;
                        if (_type2Index == _type2Options.Length)
                        {
                            _type2Index = 0;
                        }
                        break;
                }
            }
            else if (GameboyInputs.LeftPressed())
            {
                switch (_index)
                {
                    case 0:
                        _type1Index--;
                        if (_type1Index == -1)
                        {
                            _type1Index = _type1Options.Length - 1;
                        }
                        break;
                    case 1:
                        _type2Index--;
                        if (_type2Index == -1)
                        {
                            _type2Index = _type2Options.Length - 1;
                        }
                        break;
                }
            }

            if (GameboyInputs.BPressed())
            {
                GetComponent<ScreenManager>().SetScreen(_prePokedexScreen);
            }
            else if (GameboyInputs.APressed())
            {
                switch (_index)
                {
                    case 0:
                        _type1Index++;
                        if (_type1Index == _type1Options.Length)
                        {
                            _type1Index = 0;
                        }
                        break;
                    case 1:
                        _type2Index++;
                        if (_type2Index == _type2Options.Length)
                        {
                            _type2Index = 0;
                        }
                        break;
                    case 2:
                        ConfirmSearch();
                        break;
                    case 3:
                        GetComponent<ScreenManager>().SetScreen(_prePokedexScreen);
                        break;
                }
            }
        }

        private void ConfirmSearch()
        {
            var type1 = (PokemonType)(_type1Index + 1);
            var type2 = (PokemonType)_type2Index;
            var resultEntries = PokedexEntry.GetTypeFiltered(type1, type2, _listMode);
            if (resultEntries.Length > 0)
            {
                var resultScreen = new PokedexResultScreen(this, type1, type2, _listMode);
                resultScreen.LoadContent();
                GetComponent<ScreenManager>().SetScreen(resultScreen);

                // reset ui
                _type1Index = 0;
                _type2Index = 0;
            }
            else
            {
                // display no results message
                _noResultsDelay = NO_RESULTS_DELAY;
                _selectorVisible = false;
            }

            // push cursor to top
            _index = 0;
        }
    }
}
