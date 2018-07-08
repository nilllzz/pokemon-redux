using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Pokedex
{
    class PokedexOptionScreen : Screen
    {
        private const int SELECTOR_FLICKER_DELAY = 7;
        private static readonly string[] OPTIONS = new[]
        {
            "NEW POKéDEX MODE",
            "OLD POKéDEX MODE",
            "A to Z MODE",
            "UNOWN MODE"
        };
        private static readonly string[] DESCRIPTIONS = new[]
        {
            "^PK^MN are listed by\nevolution type.",
            "^PK^MN are listed by\nofficial type.",
            "^PK^MN are listed\nalphabetically.",
            "UNOWN are listed\nin catching order."
        };

        private readonly Screen _preScreen;
        private readonly PokedexListScreen _prePokedexScreen;

        private SpriteBatch _batch;
        private Texture2D _overlay;
        private PokemonFontRenderer _fontRenderer;

        private string[] _options;
        private int _index = 0;
        private bool _selectorVisible = true;
        private int _selectorFlickerDelay = SELECTOR_FLICKER_DELAY;

        public PokedexOptionScreen(Screen preScreen, PokedexListScreen prePokedexScreen)
        {
            _preScreen = preScreen;
            _prePokedexScreen = prePokedexScreen;

            _index = (int)_prePokedexScreen.ListMode;

            // initialize modes
            var options = new List<string>(OPTIONS);
            if (!Controller.ActivePlayer.HasUnownMode)
            {
                options.RemoveAt(options.Count - 1);
            }
            _options = options.ToArray();
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/optionOverlay.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
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
            _fontRenderer.DrawText(_batch, "OPTION",
                new Vector2(startX + unit * 2, unit), Border.DefaultWhite, Border.SCALE);

            // options
            var optionsText = string.Join(Environment.NewLine, _options.Select((o, i) =>
            {
                if (i == _index && _selectorVisible)
                {
                    return ">" + o;
                }
                else
                {
                    return " " + o;
                }
            }));
            _fontRenderer.DrawText(_batch, optionsText,
                new Vector2(startX + unit * 2, unit * 4), Border.DefaultWhite, Border.SCALE);

            // description
            _fontRenderer.DrawText(_batch, DESCRIPTIONS[_index],
                new Vector2(startX + unit, unit * 14), Border.DefaultWhite, Border.SCALE);

            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _selectorFlickerDelay--;
            if (_selectorFlickerDelay == 0)
            {
                _selectorFlickerDelay = SELECTOR_FLICKER_DELAY;
                _selectorVisible = !_selectorVisible;
            }

            if (GameboyInputs.DownPressed() && _index < _options.Length - 1)
            {
                _index++;
            }
            else if (GameboyInputs.UpPressed() && _index > 0)
            {
                _index--;
            }

            if (GameboyInputs.BPressed())
            {
                // return to previous screen without change
                GetComponent<ScreenManager>().SetScreen(_prePokedexScreen);
            }
            else if (GameboyInputs.APressed())
            {
                var newListMode = (PokedexListMode)_index;
                if (newListMode == _prePokedexScreen.ListMode) // no change to list mode
                {
                    // return to previous screen without change
                    GetComponent<ScreenManager>().SetScreen(_prePokedexScreen);
                }
                else
                {   // change loaded regional screen instead of setting a new one:
                    if (newListMode == PokedexListMode.Regional && _prePokedexScreen.ListMode == PokedexListMode.AtoZ)
                    {
                        (_prePokedexScreen as RegionalPokedexListScreen).SetListMode(PokedexListMode.Regional);
                        GetComponent<ScreenManager>().SetScreen(_prePokedexScreen);
                    }
                    else if (newListMode == PokedexListMode.AtoZ && _prePokedexScreen.ListMode == PokedexListMode.Regional)
                    {
                        (_prePokedexScreen as RegionalPokedexListScreen).SetListMode(PokedexListMode.AtoZ);
                        GetComponent<ScreenManager>().SetScreen(_prePokedexScreen);
                    }
                    else
                    {
                        // load new screen
                        Screen newScreen = null;
                        switch (newListMode)
                        {
                            case PokedexListMode.Regional:
                                newScreen = new RegionalPokedexListScreen(_preScreen, PokedexListMode.Regional);
                                break;
                            case PokedexListMode.National:
                                newScreen = new NationalPokedexListScreen(_preScreen);
                                break;
                            case PokedexListMode.AtoZ:
                                newScreen = new RegionalPokedexListScreen(_preScreen, PokedexListMode.AtoZ);
                                break;
                            case PokedexListMode.Unown:
                                newScreen = new PokedexUnownScreen(this);
                                break;
                        }
                        newScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(newScreen);
                    }
                }
            }
        }
    }
}
